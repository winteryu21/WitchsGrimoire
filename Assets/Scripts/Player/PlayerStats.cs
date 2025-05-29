using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float attackRange = 1.0f;
    public float cooldownReduction = 0.0f;
    public float moveSpeed = 5.0f;

    public void ApplyPassive(string id, float value)
    {
        switch (id)
        {
            case "cooldown": cooldownReduction += value; break;
            case "range": attackRange += value; break;
            // 등등
        }
    }
}
