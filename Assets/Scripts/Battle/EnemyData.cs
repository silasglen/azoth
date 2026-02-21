using UnityEngine;

/// <summary>
/// Definition of an enemy type for battle encounters.
/// Create via Assets > Create > Azoth > Enemy Data.
/// </summary>
[CreateAssetMenu(fileName = "NewEnemy", menuName = "Azoth/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public Sprite sprite;

    [Header("Stats")]
    [Min(1)]  public int maxHP  = 30;
    [Min(0)]  public int maxMP  = 10;
    [Min(0)]  public int attack  = 8;
    [Min(0)]  public int defense = 3;
    [Min(1)]  public int speed   = 5;

    [Header("Element")]
    public Element element = Element.None;

    [Header("Skills")]
    public SkillData[] knownSkills;
}
