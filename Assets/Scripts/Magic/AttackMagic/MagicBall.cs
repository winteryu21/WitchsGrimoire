using UnityEngine;

public class MagicBall : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 15;
    public float damageInterval = 0.5f;

    [Header("Orbit Settings")]
    public float orbitRadius = 1.5f;
    public float orbitSpeed = 120f;
    public float orbitHeight = 0.5f;

    [Header("Visual Effects")]
    public Color ballColor = new Color(0.2f, 0.7f, 1.0f, 1.0f);
    public float glowIntensity = 1.2f;
    
    private Transform playerTransform;
    private SpriteRenderer spriteRenderer;
    private float currentAngle = 0f;
    private float lastDamageTime = 0f;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // 구체 모양과 색상 설정
        spriteRenderer.sprite = CreateCircleSprite();
        spriteRenderer.color = ballColor;
    }
    
    void Start()
    {
        // 플레이어 찾기
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError("MagicBall: Player not found!");
        }
        
        // 시작 각도 랜덤화 (여러 구체가 있을 경우 분산)
        currentAngle = Random.Range(0f, 360f);
        
        // 콜라이더 추가
        if (GetComponent<CircleCollider2D>() == null)
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;  // 중요: 반드시 트리거로 설정
            collider.radius = 0.45f;
        }
        
        // 중요: 플레이어와 충돌하지 않도록 레이어 설정
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), playerTransform.GetComponent<Collider2D>(), true);
        
        // 자체 Rigidbody2D 추가 (옵션)
        if (GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.isKinematic = true;  // 물리 영향 받지 않음
            rb.gravityScale = 0f;   // 중력 무시
        }
    }
    
    void LateUpdate()  // Update에서 LateUpdate로 변경: 플레이어 이동 후 위치 계산
    {
        if (playerTransform == null) return;
        
        // 회전 각도 업데이트
        currentAngle += orbitSpeed * Time.deltaTime;
        if (currentAngle >= 360f) currentAngle -= 360f;
        
        // 플레이어 주위의 위치 계산
        float x = Mathf.Cos(currentAngle * Mathf.Deg2Rad) * orbitRadius;
        float y = Mathf.Sin(currentAngle * Mathf.Deg2Rad) * orbitRadius;
        
        // 플레이어 기준으로 계산된 위치로 이동
        transform.position = playerTransform.position + new Vector3(x, y + orbitHeight, 0);
    }
    
    void OnTriggerStay2D(Collider2D collision)
    {
        // 플레이어와 충돌 무시
        if (collision.CompareTag("Player")) return;
        
        // 데미지 간격 확인
        if (Time.time < lastDamageTime + damageInterval) return;
        
        // 적 감지 및 데미지 처리
        if (collision.CompareTag("Enemy"))
        {
            EnemyController enemy = collision.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                lastDamageTime = Time.time;
                PlayHitEffect();
            }
        }
    }
    
    void PlayHitEffect()
    {
        StartCoroutine(PulseEffect());
    }
    
    System.Collections.IEnumerator PulseEffect()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.3f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = originalScale;
    }
    
    // 동적으로 원형 스프라이트 생성
    private Sprite CreateCircleSprite()
    {
        // 기본 원형 스프라이트가 있으면 사용
        Sprite defaultCircle = Resources.Load<Sprite>("Circle");
        if (defaultCircle != null) return defaultCircle;
        
        // 없으면 간단한 원형 텍스처 생성
        int resolution = 64;
        Texture2D texture = new Texture2D(resolution, resolution);
        
        float centerX = resolution / 2f;
        float centerY = resolution / 2f;
        float radius = resolution / 2f - 2f;
        
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                float distance = Mathf.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));
                Color color = distance <= radius ? Color.white : Color.clear;
                texture.SetPixel(x, y, color);
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f));
    }
}