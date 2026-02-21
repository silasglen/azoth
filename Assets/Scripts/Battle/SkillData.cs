using UnityEngine;

/// <summary>
/// Definition of an alchemy skill usable in battle.
/// Create via Assets > Create > Azoth > Skill Data.
/// </summary>
[CreateAssetMenu(fileName = "NewSkill", menuName = "Azoth/Skill Data")]
public class SkillData : ScriptableObject
{
    public string skillName;

    [TextArea(1, 3)]
    public string description;

    public Sprite icon;

    public Element element = Element.None;

    [Min(0)] public int basePower = 10;
    [Min(0)] public int mpCost = 5;

    public bool targetsAll;
}
