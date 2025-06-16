using UnityEngine;
using System.Collections;

public class MagicSlash2D : MonoBehaviour
{
    public Collider2D slashCollider;
    public float slashDuration = 0.15f;
    public int slashDamage = 20;

    void Start() {
        if (slashCollider != null)
            slashCollider.enabled = false;
    }

    public void TriggerSlash()
    {
        // 게임 오브젝트가 활성화되어 있을 때만 코루틴 실행
        if (gameObject.activeSelf)
        {
            StartCoroutine(DoSlash());
        }
        else
        {
            Debug.LogWarning("MagicSlashArea is inactive. Activating first...");
            gameObject.SetActive(true);
            StartCoroutine(DoSlash());
        }
    }

    private IEnumerator DoSlash()
    {
        slashCollider.enabled = true;
        yield return new WaitForSeconds(slashDuration);
        slashCollider.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!slashCollider.enabled) return;
        if (other.CompareTag("Enemy"))
        {
            var enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
                enemy.TakeDamage(slashDamage);
        }
    }
}