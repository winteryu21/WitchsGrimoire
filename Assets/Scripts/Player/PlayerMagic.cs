using UnityEngine;

public class PlayerMagic : MonoBehaviour
{
    [Header("Auto Magic Settings")]
    public bool isMagicActive = true;
    public float castInterval = 1f;
    public float detectionRadius = 10f;
    
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public int projectileDamage = 20;
    public float projectileSpeed = 8f;
    
    private float castTimer = 0f;
    
    void Start()
    {
        // 시작 시 즉시 발사
        castTimer = castInterval;
        
        // 디버그: 적 레이어 확인
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Debug.Log($"적 정보: {enemy.name}, 레이어: {LayerMask.LayerToName(enemy.layer)}, " + 
                      $"콜라이더: {enemy.GetComponent<Collider2D>() != null}, " +
                      $"EnemyController: {enemy.GetComponent<EnemyController>() != null}");
        }
    }
    
    void Update()
    {
        if (!isMagicActive) return;
        
        // 타이머 업데이트
        castTimer += Time.deltaTime;
        
        // 발동 간격마다 마법 시전
        if (castTimer >= castInterval)
        {
            CastMagic();
            castTimer = 0f;
        }
    }
    
    void CastMagic()
    {
        // 가장 가까운 적 찾기
        GameObject nearestEnemy = FindNearestEnemy();
        
        if (nearestEnemy != null)
        {
            LaunchProjectileAt(nearestEnemy.transform.position);
        }
    }
    
    GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        GameObject nearest = null;
        float nearestDistance = detectionRadius;
        
        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            
            if (distance < nearestDistance)
            {
                nearest = enemy;
                nearestDistance = distance;
            }
        }
        
        return nearest;
    }
    
    void LaunchProjectileAt(Vector3 targetPosition)
    {
        // 발사체 생성
        GameObject projectile = new GameObject("MagicProjectile");
        
        // 스프라이트 렌더러 추가
        SpriteRenderer renderer = projectile.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateSimpleSprite(16, Color.cyan);
        renderer.color = Color.cyan;
        renderer.sortingOrder = 5; // 다른 오브젝트보다 앞에 표시
        
        // MagicProjectile 스크립트 추가
        MagicProjectile magicScript = projectile.AddComponent<MagicProjectile>();
        magicScript.damage = projectileDamage;
        magicScript.speed = projectileSpeed;
        magicScript.projectileColor = Color.cyan;
        
        // 발사 방향 설정
        Vector3 direction = (targetPosition - transform.position).normalized;
        magicScript.SetDirection(direction);
        
        // 플레이어 위치에서 약간 앞으로 조정
        projectile.transform.position = transform.position + direction * 0.7f;
    }
    
    Sprite CreateSimpleSprite(int size, Color color)
    {
        Texture2D texture = new Texture2D(size, size);
        
        float center = size / 2f;
        float radius = size / 2f - 1;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float distance = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
                texture.SetPixel(x, y, distance <= radius ? color : Color.clear);
            }
        }
        
        texture.Apply();
        
        return Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f)
        );
    }
}