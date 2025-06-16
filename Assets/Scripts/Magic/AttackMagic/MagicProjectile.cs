using UnityEngine;
using System.Collections;

public class MagicProjectile : MonoBehaviour
{
    public float speed = 8f;
    public int damage = 20;
    public float lifetime = 3f;
    public Color projectileColor = Color.cyan;
    public float radius = 0.35f; // 발사체 감지 범위
    
    private Vector3 direction;
    private bool hasHit = false;
    private SpriteRenderer spriteRenderer;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.color = projectileColor;
        }
        
        // 중요: 모든 물리 컴포넌트 제거
        Destroy(GetComponent<Rigidbody2D>());
        Destroy(GetComponent<Collider2D>());
    }
    
    void Start()
    {
        // 정해진 시간 후 소멸
        Destroy(gameObject, lifetime);
        
        // 발사체 시각화만 담당하는 게임오브젝트로 취급
        // - 콜라이더 없음
        // - 리지드바디 없음
        // - 물리 충돌 없음
        
        Debug.Log("순수 시각효과 발사체 생성: " + gameObject.name);
    }
    
    void Update()
    {
        if (hasHit) return;
        
        // 1. 이동
        transform.position += direction * speed * Time.deltaTime;
        
        // 2. 수동 충돌 감지 (레이캐스트)
        DetectCollisions();
    }
    
    void DetectCollisions()
    {
        // 원형 레이캐스트로 충돌 검사
        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            transform.position, 
            radius, 
            direction, 
            0.1f, // 최소한의 거리만 검사
            LayerMask.GetMask("Default", "Enemy") // 벽과 적만 감지
        );
        
        foreach (RaycastHit2D hit in hits)
        {
            // 플레이어 무시
            if (hit.collider.CompareTag("Player")) continue;
            
            // 이미 처리된 충돌이면 무시
            if (hasHit) continue;
            
            Debug.Log("발사체 레이캐스트 히트: " + hit.collider.gameObject.name + ", 태그: " + hit.collider.tag);
            
            // 적과 충돌
            if (hit.collider.CompareTag("Enemy"))
            {
                hasHit = true;
                
                // 데미지 처리
                EnemyController enemy = hit.collider.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    Debug.Log("적 피격! 데미지: " + damage);
                    enemy.TakeDamage(damage);
                }
                else
                {
                    Debug.LogWarning("EnemyController 컴포넌트 없음: " + hit.collider.gameObject.name);
                }
                
                // 충돌 효과 및 삭제
                StartCoroutine(HitEffect());
                return;
            }
            // 벽과 충돌 (Enemy가 아닌 다른 물체)
            else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Default"))
            {
                hasHit = true;
                StartCoroutine(HitEffect());
                return;
            }
        }
    }
    
    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
        
        // 방향에 따라 회전
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    
    IEnumerator HitEffect()
    {
        // 시각 효과 재생 (폭발 효과 등을 여기에 추가 가능)
        speed = 0; // 이동 중지
        
        // 간단한 스케일 증가 및 페이드 아웃 효과
        float duration = 0.2f;
        float timer = 0f;
        
        Color startColor = spriteRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        Vector3 startScale = transform.localScale;
        Vector3 endScale = startScale * 1.5f;
        
        while (timer < duration)
        {
            float t = timer / duration;
            spriteRenderer.color = Color.Lerp(startColor, endColor, t);
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        // 발사체 제거
        Destroy(gameObject);
    }
}