using System;
using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float detectionRange = 10f;
    public float separationDistance = 1.0f;  // 다른 적과 유지할 최소 거리
    public float separationWeight = 1.5f;    // 분리 행동의 가중치
    
    [Header("Damage")]
    public int contactDamage = 10;
    public float damageInterval = 1f;
    private float lastDamageTime = 0f;
    
    [Header("Health")]
    public int maxHealth = 100;
    private int currentHealth;
    
    [Header("Hit Effect")]
    public float flashDuration = 0.2f; // 깜빡임 지속 시간
    public int flashCount = 3; // 깜빡임 횟수
    public Color flashColor = Color.red; // 깜빡임 색상
    private Color originalColor;
    private bool isFlashing = false;
    
    [Header("References")]
    public Animator animator;
    private Rigidbody2D rb;
    private Transform playerTransform;
    private SpriteRenderer spriteRenderer;
    
    private bool isDead = false;
    private Vector2 movementDirection;
    private bool canMove = true;

    [Header("Rewards")]
    public int expValue = 20; // 이 적을 처치했을 때 얻는 경험치
    
    // 이벤트
    public event Action<GameObject> OnEnemyDeath;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 스프라이트 렌더러가 있으면 원래 색상 저장
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // Kinematic Rigidbody 설정
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
            rb.useFullKinematicContacts = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        
        if (animator == null)
            animator = GetComponent<Animator>();
            
        currentHealth = maxHealth;
    }
    
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    void Update()
    {
        if (isDead || playerTransform == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // 플레이어가 감지 범위 안에 있을 때
        if (distanceToPlayer <= detectionRange)
        {
            // 플레이어 방향으로 이동 방향 설정
            Vector2 toPlayerDirection = (playerTransform.position - transform.position).normalized;
            
            // 다른 적들로부터의 분리 행동 계산
            Vector2 separationForce = CalculateSeparation();
            
            // 최종 이동 방향 결정 (플레이어 방향 + 분리 행동)
            movementDirection = (toPlayerDirection + separationForce * separationWeight).normalized;
            
            // 애니메이션 업데이트
            if (animator != null)
                animator.SetBool("isMoving", true);
        }
        else
        {
            // 대기 상태
            movementDirection = Vector2.zero;
            
            if (animator != null)
                animator.SetBool("isMoving", false);
        }
        
        // 스프라이트 방향 업데이트
        UpdateSpriteDirection();
    }
    
    // 다른 적들로부터 멀어지려는 분리 행동 계산
    private Vector2 CalculateSeparation()
    {
        Vector2 separationVector = Vector2.zero;
        int nearbyEnemies = 0;
        
        // 주변의 모든 적 찾기
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, separationDistance * 2f, LayerMask.GetMask("Enemy"));
        
        foreach (var hitCollider in hitColliders)
        {
            // 자기 자신은 제외
            if (hitCollider.gameObject == gameObject) continue;
            
            // 다른 적과의 거리 계산
            float distance = Vector2.Distance(transform.position, hitCollider.transform.position);
            
            // 너무 가까우면 멀어지는 방향 벡터 계산
            if (distance < separationDistance)
            {
                Vector2 awayDirection = (transform.position - hitCollider.transform.position).normalized;
                
                // 가까울수록 더 강한 반발력 적용
                float strength = (separationDistance - distance) / separationDistance;
                separationVector += awayDirection * strength;
                
                nearbyEnemies++;
            }
        }
        
        // 주변 적이 있으면 평균 방향 계산
        if (nearbyEnemies > 0)
        {
            separationVector /= nearbyEnemies;
        }
        
        return separationVector;
    }
    
    void FixedUpdate()
    {
        if (!canMove || movementDirection == Vector2.zero || isDead || playerTransform == null)
            return;

        // 이동 목표 위치 계산
        Vector2 targetPosition = rb.position + movementDirection * moveSpeed * Time.fixedDeltaTime;
        
        // 이동 방향으로 레이캐스트 수행 (벽 충돌 체크)
        RaycastHit2D hit = Physics2D.Raycast(
            rb.position, 
            movementDirection, 
            moveSpeed * Time.fixedDeltaTime,
            LayerMask.GetMask("Default", "Player")  // 벽과 플레이어 레이어 체크
        );
        
        if (hit.collider != null)
        {
            // 충돌이 감지되면 충돌 지점까지만 이동
            float distance = Vector2.Distance(rb.position, hit.point);
            Vector2 adjustedPosition = rb.position + movementDirection.normalized * (distance - 0.05f);
            rb.MovePosition(adjustedPosition);
        }
        else
        {
            // 충돌이 없으면 목표 위치로 이동
            rb.MovePosition(targetPosition);
        }
    }
    
    void UpdateSpriteDirection()
    {
        if (playerTransform != null && spriteRenderer != null)
        {
            spriteRenderer.flipX = playerTransform.position.x < transform.position.x;
        }
    }
    
    // 트리거 충돌 처리
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && Time.time >= lastDamageTime + damageInterval)
        {
            // 플레이어에게 데미지 전달
            PlayerStats playerStats = collision.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(contactDamage);
                lastDamageTime = Time.time; // 데미지 타이머 갱신
            }
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        // 명확한 디버그 로그 추가
        Debug.Log(gameObject.name + "이(가) " + damage + " 데미지를 받음! 현재 체력: " + currentHealth + " -> " + (currentHealth - damage));
        
        currentHealth -= damage;
        
        if (animator != null)
            animator.SetTrigger("hit");
            
        // 피격 효과 (기존에 있다면)
        if (!isFlashing && spriteRenderer != null)
        {
            StartCoroutine(FlashEffect());
        }
                
        if (currentHealth <= 0)
        {
            Die();
        }
    }    
    // 피격 시 깜빡임 효과 코루틴
    private IEnumerator FlashEffect()
    {
        isFlashing = true;
        
        // flashCount만큼 깜빡임 반복
        for (int i = 0; i < flashCount; i++)
        {
            // 색상을 flashColor로 변경
            spriteRenderer.color = flashColor;
            
            // flashDuration/2 동안 기다림
            yield return new WaitForSeconds(flashDuration / 2);
            
            // 원래 색상으로 복귀
            spriteRenderer.color = originalColor;
            
            // 다음 깜빡임 전까지 대기
            yield return new WaitForSeconds(flashDuration / 2);
        }
        
        // 원래 색상으로 최종 복귀 (안전장치)
        spriteRenderer.color = originalColor;
        isFlashing = false;
    }
    
    void Die()
    {
        isDead = true;
        canMove = false;
        
        if (animator != null)
            animator.SetTrigger("die");
            
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
        
        // 플레이어에게 경험치 부여
        RewardExperience();
            
        // 이벤트 호출
        OnEnemyDeath?.Invoke(gameObject);
        
        Destroy(gameObject, 2f);
    }
    
    // 플레이어에게 경험치 보상 제공
    private void RewardExperience()
    {
        // 플레이어 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                // 경험치 부여
                playerStats.GainExperience(expValue);
                
                // 디버그 메시지
                Debug.Log($"{gameObject.name}을(를) 처치하여 {expValue} 경험치 획득!");
            }
        }
    }
    
    // 난이도에 따라 몬스터의 체력과 주는 경험치 조정
    public void SetDifficulty(float difficultyMultiplier)
    {
        // 체력과 경험치를 난이도에 따라 조정
        maxHealth = Mathf.RoundToInt(maxHealth * difficultyMultiplier);
        currentHealth = maxHealth;
        
        // 난이도가 높을수록 더 많은 경험치 제공
        expValue = Mathf.RoundToInt(expValue * difficultyMultiplier);
    }

    
    public void SetMultipliers(float healthMultiplier, float speedMultiplier)
    {
        maxHealth = Mathf.RoundToInt(maxHealth * healthMultiplier);
        currentHealth = maxHealth;
        moveSpeed *= speedMultiplier;
    }
    
    // 디버그용: 분리 거리와 주변 적을 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, separationDistance);
    }
}