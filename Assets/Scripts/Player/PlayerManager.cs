using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public PlayerHealth health;
    public PlayerStats stats;
    public PlayerSkills skills;

    void Start()
    {
        health = GetComponent<PlayerHealth>();
        stats = GetComponent<PlayerStats>();
        skills = GetComponent<PlayerSkills>();
    }

    public void ApplyUpgrade(string upgradeId, float value)
    {
        stats.ApplyPassive(upgradeId, value);
    }

    public void GainSkill(string skillId)
    {
        skills.LearnSkill(skillId);
    }
}
