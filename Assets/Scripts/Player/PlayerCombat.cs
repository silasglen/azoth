using UnityEngine;

/// <summary>
/// Persistent player combat stats.  Attach alongside <see cref="PlayerController"/>.
/// HP/MP carry over between battles; use <see cref="FullRestore"/> to reset.
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    [Header("Base Stats")]
    [Min(1)]  public int maxHP  = 100;
    [Min(0)]  public int maxMP  = 50;
    [Min(0)]  public int attack  = 10;
    [Min(0)]  public int defense = 5;
    [Min(1)]  public int speed   = 10;

    [Header("Runtime")]
    [SerializeField] int _currentHP;
    [SerializeField] int _currentMP;

    public int CurrentHP
    {
        get => _currentHP;
        set => _currentHP = Mathf.Clamp(value, 0, maxHP);
    }

    public int CurrentMP
    {
        get => _currentMP;
        set => _currentMP = Mathf.Clamp(value, 0, maxMP);
    }

    [Header("Known Skills")]
    public SkillData[] knownSkills;

    void Awake()
    {
        _currentHP = maxHP;
        _currentMP = maxMP;
    }

    public void FullRestore()
    {
        _currentHP = maxHP;
        _currentMP = maxMP;
    }
}
