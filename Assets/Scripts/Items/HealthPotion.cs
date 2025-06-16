using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    [SerializeField] private float healAmount = 20f;
    [SerializeField] private float attractSpeed = 5f;
    [SerializeField] private float attractDistance = 2f;
    
    private Transform playerTransform;
    private bool isCollected = false;
    
    // 프로퍼티
    public float HealAmount => healAmount;
    
    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    private void Update()
    {
        if (playerTransform != null && !isCollected)
        {
            // 플레이어가 일정 거리 안에 들어오면 플레이어를 향해 이동
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= attractDistance)
            {
                Vector2 direction = (playerTransform.position - transform.position).normalized;
                transform.position = Vector2.MoveTowards(
                    transform.position, 
                    playerTransform.position, 
                    attractSpeed * Time.deltaTime
                );
            }
        }
    }
    
    public void Collect()
    {
        if (isCollected) return;
        
        isCollected = true;
        
        // 시각 효과 (필요시)
        PlayCollectEffect();
        
        // 오브젝트 비활성화 또는 파괴
        gameObject.SetActive(false);
        Destroy(gameObject, 0.1f);
    }
    
    private void PlayCollectEffect()
    {
        // 수집 효과 (파티클, 사운드 등)
        // 구현할 경우 여기에 추가
    }
}