using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkillSlot
{
    public string skillId;
    public int level;
}

public class PlayerSkills : MonoBehaviour
{
    public List<SkillSlot> activeSkills = new List<SkillSlot>();

    public void LearnSkill(string id)
    {
        var found = activeSkills.Find(s => s.skillId == id);
        if (found != null) found.level++;
        else activeSkills.Add(new SkillSlot { skillId = id, level = 1 });
    }
}
