using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private PlayerStats playerStats;
    
    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        /*
        // 경험치 아이템 충돌 처리
        if (other.CompareTag("ExperienceGem"))
        {
            ExperienceGem gem = other.GetComponent<ExperienceGem>();
            if (gem != null)
            {
                playerStats.GainExperience(gem.ExperienceValue);
                gem.Collect();
            }
        }

        // 체력 회복 아이템 충돌 처리
        else if (other.CompareTag("HealthPotion"))
        {
            HealthPotion potion = other.GetComponent<HealthPotion>();
            if (potion != null)
            {
                playerStats.Heal(potion.HealAmount);
                potion.Collect();
            }
        }
        */
    }
    
    private void OnCollisionStay2D(Collision2D collision)
    {
        // 적과의 충돌 처리
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                playerStats.TakeDamage(enemy.ContactDamage * Time.deltaTime);
            }
        }
    }
}