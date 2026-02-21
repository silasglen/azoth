using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime battle participant built from either <see cref="PlayerCombat"/>
/// or <see cref="EnemyData"/>.  Lives only during a battle.
/// </summary>
public class Combatant
{
    public string    Name      { get; private set; }
    public Sprite    Sprite    { get; private set; }
    public bool      IsPlayer  { get; private set; }
    public int       MaxHP     { get; private set; }
    public int       MaxMP     { get; private set; }
    public int       Attack    { get; private set; }
    public int       Defense   { get; private set; }
    public int       Speed     { get; private set; }
    public Element   Element   { get; private set; }
    public List<SkillData> Skills { get; private set; }

    public int  CurrentHP { get; set; }
    public int  CurrentMP { get; set; }
    public bool IsAlive   => CurrentHP > 0;

    // Reference for syncing back after battle.
    public PlayerCombat PlayerSource   { get; private set; }
    public Inventory    PlayerInventory { get; private set; }

    public static Combatant FromPlayer(PlayerCombat pc, Inventory inventory)
    {
        return new Combatant
        {
            Name            = "Player",
            Sprite          = null,
            IsPlayer        = true,
            MaxHP           = pc.maxHP,
            MaxMP           = pc.maxMP,
            Attack          = pc.attack,
            Defense         = pc.defense,
            Speed           = pc.speed,
            Element         = Element.None,
            Skills          = new List<SkillData>(pc.knownSkills ?? System.Array.Empty<SkillData>()),
            CurrentHP       = pc.CurrentHP,
            CurrentMP       = pc.CurrentMP,
            PlayerSource    = pc,
            PlayerInventory = inventory
        };
    }

    public static Combatant FromEnemy(EnemyData data)
    {
        return new Combatant
        {
            Name      = data.enemyName,
            Sprite    = data.sprite,
            IsPlayer  = false,
            MaxHP     = data.maxHP,
            MaxMP     = data.maxMP,
            Attack    = data.attack,
            Defense   = data.defense,
            Speed     = data.speed,
            Element   = data.element,
            Skills    = new List<SkillData>(data.knownSkills ?? System.Array.Empty<SkillData>()),
            CurrentHP = data.maxHP,
            CurrentMP = data.maxMP
        };
    }

    /// <summary>Write runtime HP/MP back to the persistent PlayerCombat component.</summary>
    public void SyncToPlayer()
    {
        if (PlayerSource == null) return;
        PlayerSource.CurrentHP = CurrentHP;
        PlayerSource.CurrentMP = CurrentMP;
    }
}
