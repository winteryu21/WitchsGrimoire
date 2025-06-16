using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Vector2 moveDirection;
    private Rigidbody2D rb;
    
    // 애니메이터 및 스프라이트 렌더러 참조 변수
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private Animator shadowAnimator;
    [SerializeField] private SpriteRenderer characterSprite;
    [SerializeField] private SpriteRenderer shadowSprite;
    
    // 대시 관련 변수
    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 3f;       // 대시 거리
    [SerializeField] private float dashDuration = 0.2f;     // 대시 지속 시간
    [SerializeField] private float dashCooldown = 1f;       // 대시 쿨다운
    [SerializeField] private bool isDashing = false;
    [SerializeField] private bool canDash = true;
    private Vector2 dashDirection;
    private float dashTimer;
    
    private PlayerStats stats;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction dashAction;  // 대시 입력 액션 추가
    private bool isDamagedAnimationActive = false;
    private Collider2D playerCollider;  // 플레이어 콜라이더 참조

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        dashAction = playerInput.actions["Dash"];  // "Dash" 액션 바인딩 필요
        playerCollider = GetComponent<Collider2D>();
        
        // Rigidbody2D를 Kinematic으로 설정 (충돌 시 밀리지 않도록)
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
            rb.useFullKinematicContacts = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        
        // 자식에서 컴포넌트 찾기 (할당되지 않은 경우)
        if (characterAnimator == null)
        {
            // "Character" 자식 오브젝트 찾기
            Transform characterChild = transform.Find("Character");
            if (characterChild != null)
            {
                characterAnimator = characterChild.GetComponent<Animator>();
                
                // 같은 오브젝트에서 SpriteRenderer도 찾기
                if (characterSprite == null)
                {
                    characterSprite = characterChild.GetComponent<SpriteRenderer>();
                }
            }
            
            // 자식에서 Animator 컴포넌트 찾기
            if (characterAnimator == null)
            {
                characterAnimator = GetComponentInChildren<Animator>();
            }
        }
        
        // SpriteRenderer 찾기 (아직 찾지 못한 경우)
        if (characterSprite == null)
        {
            characterSprite = GetComponentInChildren<SpriteRenderer>();
        }
        
        // 디버그 로그
        if (characterAnimator == null)
            Debug.LogWarning("Character animator not found! Animations won't work.");
        
        if (characterSprite == null)
            Debug.LogWarning("Character sprite renderer not found! Sprite flip won't work.");
    }

    private void Update()
    {
        // 게임 일시정지 상태 확인
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            return;
            
        // 입력 처리
        ProcessInputs();
        
        // 대시 입력 확인 및 처리
        CheckDashInput();
        
        // 대시 업데이트
        UpdateDash();
        
        // 애니메이션 업데이트
        UpdateAnimation();
        
        // 스프라이트 방향 업데이트
        UpdateSpriteDirection();
    }

    private void FixedUpdate()
    {
        // 대시 중이라면 대시 이동 처리
        if (isDashing)
        {
            PerformDash();
        }
        else
        {
            // 일반 이동
            Move();
        }
    }

    private void ProcessInputs()
    {
        // 새 Input System 사용
        Vector2 input = moveAction.ReadValue<Vector2>();
        moveDirection = input.normalized;
    }

    private void CheckDashInput()
    {
        // 대시 입력 확인 (스페이스바)
        if (dashAction.triggered && canDash && !isDashing && stats.CanMove)
        {
            StartDash();
        }
    }

    private void StartDash()
    {
        if (moveDirection.magnitude < 0.1f)
        {
            // 움직이지 않을 때는 마지막으로 바라보는 방향 또는 기본 아래쪽으로 대시
            dashDirection = characterSprite.flipX ? Vector2.left : Vector2.right;
        }
        else
        {
            // 현재 이동 방향으로 대시
            dashDirection = moveDirection.normalized;
        }

        isDashing = true;
        canDash = false;
        dashTimer = dashDuration;
        
        // 대시 중 충돌 무시
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }
        
        // 대시 이펙트 생성 (선택적)
        // Instantiate(dashEffectPrefab, transform.position, Quaternion.identity);
        
        // 대시 사운드 재생 (선택적)
        // AudioManager.Instance.PlaySFX("Dash");
    }

    private void UpdateDash()
    {
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            
            if (dashTimer <= 0)
            {
                EndDash();
            }
        }
    }

    private void EndDash()
    {
        isDashing = false;
        
        // 충돌 다시 활성화
        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }
        
        // 대시 쿨다운 시작
        StartCoroutine(DashCooldown());
    }

    private IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void PerformDash()
    {
        // 대시 중 이동 처리 (충돌 검사 없이 빠르게 이동)
        float dashSpeed = dashDistance / dashDuration;
        Vector2 dashVelocity = dashDirection * dashSpeed;
        rb.MovePosition(rb.position + dashVelocity * Time.fixedDeltaTime);
    }

    private void Move()
    {
        if (stats == null || !stats.CanMove || moveDirection == Vector2.zero)
            return;
        
        // 충돌 체크 레이캐스트 사용
        Vector2 targetPosition = rb.position + moveDirection * moveSpeed * stats.GetTotalSpeedMultiplier() * Time.fixedDeltaTime;
        
        // 이동 방향으로 레이캐스트 수행
        RaycastHit2D hit = Physics2D.Raycast(
            rb.position, 
            moveDirection, 
            moveSpeed * Time.fixedDeltaTime,
            LayerMask.GetMask("Default", "Enemy")  // 적절한 레이어 마스크 설정
        );
        
        if (hit.collider != null)
        {
            // 충돌이 감지되면 충돌 지점까지만 이동
            float distance = Vector2.Distance(rb.position, hit.point);
            Vector2 adjustedPosition = rb.position + moveDirection.normalized * (distance - 0.05f); // 약간의 여유 공간 추가
            rb.MovePosition(adjustedPosition);
        }
        else
        {
            // 충돌이 없으면 원래 목표 위치로 이동
            rb.MovePosition(targetPosition);
        }
    }

    // 스프라이트 방향 업데이트 메서드
    private void UpdateSpriteDirection()
    {
        if (characterSprite != null && Mathf.Abs(moveDirection.x) > 0.01f)
        {
            // X 이동 방향에 따라 스프라이트 반전
            characterSprite.flipX = moveDirection.x < 0;
        }
    }

    private void UpdateAnimation()
    {
        // null 체크 추가 - 애니메이터가 없으면 아무 작업도 하지 않음
        if (characterAnimator == null) return;

        bool isMoving = moveDirection.magnitude > 0.01f;
        characterAnimator.SetBool("isMoving", isMoving);
        
        if (shadowAnimator != null)
        {
            shadowAnimator.SetBool("isMoving", isMoving);
        }
    }
    
    // 트리거 콜라이더 이벤트로 변경
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 대시 중에는 충돌 무시
        if (isDashing) return;
        
        // 적과의 충돌 시
        if (collision.CompareTag("Enemy") && !isDamagedAnimationActive)
        {
            // 데미지 애니메이션 파라미터 설정
            if (characterAnimator != null)
            {
                StartCoroutine(ShowDamageAnimation());
            }
        }
    }
    
    // 데미지 애니메이션을 1초간 표시하는 코루틴
    private IEnumerator ShowDamageAnimation()
    {
        isDamagedAnimationActive = true;
        characterAnimator.SetBool("isDamaged", true);
        
        yield return new WaitForSeconds(1f);
        
        characterAnimator.SetBool("isDamaged", false);
        isDamagedAnimationActive = false;
    }
}