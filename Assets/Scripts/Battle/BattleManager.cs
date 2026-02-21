using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Turn-based battle state machine.  Driven entirely by coroutines.
/// Communicates with UI only through events — holds zero UI references.
/// </summary>
public class BattleManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────────────
    public static BattleManager Instance { get; private set; }

    // ── Events ───────────────────────────────────────────────────────────
    public event Action OnBattleStart;
    public event Action<bool> OnBattleEnd;             // true = victory
    public event Action<List<Combatant>> OnTurnOrderDecided;
    public event Action<Combatant> OnTurnStart;
    public event Action OnPlayerChooseSkill;
    public event Action<string> OnBattleLog;
    public event Action<Combatant, int> OnDamageDealt; // target, amount
    public event Action<Combatant> OnCombatantHPChanged;
    public event Action<Combatant> OnCombatantDefeated;

    // ── Public read-only accessors for UI ────────────────────────────────
    public Combatant PlayerCombatant => _playerCombatant;
    public IReadOnlyList<Combatant> Enemies => _enemies;
    public bool IsBattleActive => _battleActive;

    // ── Internal state ───────────────────────────────────────────────────
    Combatant _playerCombatant;
    readonly List<Combatant> _enemies = new();
    readonly List<Combatant> _allCombatants = new();
    bool _battleActive;

    // Player choice communicated from UI.
    SkillData _chosenSkill;   // null = basic Attack
    Combatant _chosenTarget;
    bool _playerHasChosen;

    // ── Lifecycle ────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ── Public API ───────────────────────────────────────────────────────

    /// <summary>Start a battle against the given enemies.</summary>
    public void StartBattle(EnemyData[] enemyDatas)
    {
        if (_battleActive) return;

        var player = GameManager.Instance.Player;
        var pc     = player.GetComponent<PlayerCombat>();
        var inv    = player.Inventory;

        _playerCombatant = Combatant.FromPlayer(pc, inv);

        _enemies.Clear();
        foreach (var ed in enemyDatas)
            _enemies.Add(Combatant.FromEnemy(ed));

        _allCombatants.Clear();
        _allCombatants.Add(_playerCombatant);
        _allCombatants.AddRange(_enemies);

        _battleActive = true;
        GameManager.Instance.State = GameManager.GameState.Battle;

        StartCoroutine(BattleLoop());
    }

    /// <summary>Called by BattleUI when the player picks a skill and target.</summary>
    public void SubmitPlayerChoice(SkillData skill, Combatant target)
    {
        _chosenSkill  = skill;
        _chosenTarget = target;
        _playerHasChosen = true;
    }

    // ── Coroutine state machine ──────────────────────────────────────────

    IEnumerator BattleLoop()
    {
        OnBattleStart?.Invoke();
        Log("Battle start!");
        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            // Build turn order sorted by speed descending, random tiebreak.
            var turnOrder = new List<Combatant>(_allCombatants);
            turnOrder.RemoveAll(c => !c.IsAlive);
            turnOrder.Sort((a, b) =>
            {
                int cmp = b.Speed.CompareTo(a.Speed);
                return cmp != 0 ? cmp : UnityEngine.Random.Range(-1, 2);
            });
            OnTurnOrderDecided?.Invoke(turnOrder);

            foreach (var actor in turnOrder)
            {
                if (!actor.IsAlive) continue;

                OnTurnStart?.Invoke(actor);
                yield return new WaitForSeconds(0.3f);

                if (actor.IsPlayer)
                    yield return PlayerTurn(actor);
                else
                    yield return EnemyTurn(actor);

                // Check end conditions after each action.
                if (!_playerCombatant.IsAlive)
                {
                    yield return EndBattle(false);
                    yield break;
                }
                if (AllEnemiesDead())
                {
                    yield return EndBattle(true);
                    yield break;
                }
            }
        }
    }

    IEnumerator PlayerTurn(Combatant player)
    {
        _playerHasChosen = false;
        OnPlayerChooseSkill?.Invoke();

        // Wait until UI calls SubmitPlayerChoice.
        while (!_playerHasChosen)
            yield return null;

        yield return ExecuteAction(player, _chosenSkill, _chosenTarget);
    }

    IEnumerator EnemyTurn(Combatant enemy)
    {
        // Pick a random usable skill; fall back to basic Attack.
        SkillData skill = PickEnemySkill(enemy);

        Combatant target = _playerCombatant; // enemies always target the player

        if (skill != null)
            Log($"{enemy.Name} uses {skill.skillName}!");
        else
            Log($"{enemy.Name} attacks!");

        yield return ExecuteAction(enemy, skill, target);
    }

    IEnumerator ExecuteAction(Combatant attacker, SkillData skill, Combatant target)
    {
        // Validate MP and fall back to basic Attack if insufficient.
        if (skill != null && attacker.CurrentMP < skill.mpCost)
            skill = null;

        // Deduct MP.
        if (skill != null)
            attacker.CurrentMP -= skill.mpCost;

        if (skill != null && skill.targetsAll)
        {
            // Hit all enemies (or just the player if attacker is enemy).
            var targets = attacker.IsPlayer
                ? new List<Combatant>(_enemies)
                : new List<Combatant> { _playerCombatant };

            foreach (var t in targets)
            {
                if (!t.IsAlive) continue;
                ApplyDamage(attacker, skill, t);
            }
        }
        else if (target != null && target.IsAlive)
        {
            ApplyDamage(attacker, skill, target);
        }

        yield return new WaitForSeconds(0.5f);
    }

    void ApplyDamage(Combatant attacker, SkillData skill, Combatant target)
    {
        float effectivePower;
        if (skill == null)
        {
            effectivePower = attacker.Attack;
        }
        else
        {
            int catalystBonus = 0;
            if (attacker.IsPlayer && attacker.PlayerInventory != null)
                catalystBonus = attacker.PlayerInventory.GetCatalystBonus(skill.element);
            effectivePower = skill.basePower * (1f + catalystBonus * 0.01f);
        }

        float rawDamage = effectivePower + attacker.Attack - target.Defense;

        float weaknessMultiplier = 1f;
        if (skill != null && skill.element != Element.None)
        {
            if (IsWeakTo(target.Element, skill.element))
            {
                weaknessMultiplier = 1.5f;
                Log("It's super effective!");
            }
        }

        int finalDamage = Mathf.Max(1, Mathf.FloorToInt(rawDamage * weaknessMultiplier));

        target.CurrentHP = Mathf.Max(0, target.CurrentHP - finalDamage);

        string skillLabel = skill != null ? skill.skillName : "Attack";
        Log($"{attacker.Name}'s {skillLabel} deals {finalDamage} damage to {target.Name}!");

        OnDamageDealt?.Invoke(target, finalDamage);
        OnCombatantHPChanged?.Invoke(target);

        if (!target.IsAlive)
        {
            Log($"{target.Name} is defeated!");
            OnCombatantDefeated?.Invoke(target);
        }
    }

    IEnumerator EndBattle(bool victory)
    {
        yield return new WaitForSeconds(0.8f);

        if (victory)
            Log("Victory!");
        else
            Log("Defeat...");

        // On victory, sync HP/MP back; on defeat, restore to full.
        if (victory)
            _playerCombatant.SyncToPlayer();
        else
            _playerCombatant.PlayerSource?.FullRestore();

        yield return new WaitForSeconds(1f);

        _battleActive = false;
        _enemies.Clear();
        _allCombatants.Clear();

        GameManager.Instance.State = GameManager.GameState.Playing;
        OnBattleEnd?.Invoke(victory);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    bool AllEnemiesDead()
    {
        foreach (var e in _enemies)
            if (e.IsAlive) return false;
        return true;
    }

    SkillData PickEnemySkill(Combatant enemy)
    {
        var usable = new List<SkillData>();
        foreach (var s in enemy.Skills)
        {
            if (s != null && enemy.CurrentMP >= s.mpCost)
                usable.Add(s);
        }
        if (usable.Count == 0) return null;
        return usable[UnityEngine.Random.Range(0, usable.Count)];
    }

    void Log(string msg) => OnBattleLog?.Invoke(msg);

    /// <summary>Returns true if <paramref name="defenderElement"/> is weak to <paramref name="attackElement"/>.</summary>
    static bool IsWeakTo(Element defenderElement, Element attackElement)
    {
        return (defenderElement, attackElement) switch
        {
            (Element.Ventus, Element.Ignis)  => true,
            (Element.Terra,  Element.Ventus) => true,
            (Element.Aqua,   Element.Terra)  => true,
            (Element.Ignis,  Element.Aqua)   => true,
            (Element.Umbra,  Element.Lux)    => true,
            (Element.Lux,    Element.Umbra)  => true,
            _ => false
        };
    }
}
