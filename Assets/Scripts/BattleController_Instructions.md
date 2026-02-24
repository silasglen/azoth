# BattleController.cs — Claude Code Agent Build Spec (v4)

> **Agent directive:** Read this ENTIRE document before writing any code. Follow it exactly. Do not improvise, skip sections, or add features not described here. If something is marked `// TODO`, leave it as a TODO — do not implement it. Every code block in this spec is the canonical source of truth — copy it verbatim unless a prose note says otherwise.

---

## 0. Pre-Flight

1. Check if the directory `Assets/Scripts/Battle/` exists. If not, create it.
2. Check if the directory `Assets/Scripts/Battle/Interfaces/` exists. If not, create it.
3. You will create exactly **4 files** (listed in Section 1).
4. All files use namespace `Battle`.
5. Target: Unity 2022.3+ / C# 9. Do NOT use C# 10+ features (no `global using`, no file-scoped namespaces, no `record` types).
6. Use **traditional namespace blocks** with braces, not file-scoped namespace declarations.
7. After writing all files, read back through each file and verify zero syntax errors. Pay special attention to: matching braces, semicolons after field declarations, and correct generic type parameters on events.
8. Do NOT add `using UnityEngine.UI;`, `using TMPro;`, `using UnityEngine.InputSystem;`, or any UI-related using statements. The only usings needed are `System`, `System.Collections`, `System.Collections.Generic`, and `UnityEngine`.

---

## 1. Files To Create

| # | Path | Purpose |
|---|------|---------|
| 1 | `Assets/Scripts/Battle/Interfaces/IBattleUnit.cs` | Interface for any combatant |
| 2 | `Assets/Scripts/Battle/Interfaces/IEnemyAI.cs` | Interface for enemy decision-making + EnemyIntent struct |
| 3 | `Assets/Scripts/Battle/BattleEnums.cs` | All enums used by the battle system |
| 4 | `Assets/Scripts/Battle/BattleController.cs` | The main controller (bulk of the work) |

Create them **in this order** so dependencies resolve top-down. Do NOT create any additional files.

---

## 2. File #1 — IBattleUnit.cs

```csharp
// Assets/Scripts/Battle/Interfaces/IBattleUnit.cs
using System;

namespace Battle
{
    /// <summary>
    /// Contract for any entity that participates in battle (player or enemy).
    /// Implementations must ensure:
    /// - TakeDamage clamps HP to >= 0
    /// - IsAlive returns false when HP reaches 0
    /// - DodgeBonus and HasShield reflect current gear state
    /// </summary>
    public interface IBattleUnit
    {
        string UnitName { get; }
        UnitType UnitType { get; }
        ElementType Element { get; }
        int CurrentHP { get; set; }
        int MaxHP { get; }
        int AttackPower { get; }
        int Defense { get; }
        bool IsAlive { get; }

        // --- Defensive gear stats ---

        /// <summary>
        /// Additive dodge bonus from gear (0.0 = no bonus, 0.10 = +10%).
        /// Combined with base 75% in BattleController. Must not be negative.
        /// </summary>
        float DodgeBonus { get; }

        /// <summary>
        /// True if this unit has a shield equipped.
        /// Upgrades Block from 99% → 100% and block damage from 1% → 0%.
        /// </summary>
        bool HasShield { get; }

        // --- Skill resource ---

        /// <summary>
        /// Current skill resource: MP for Magus, Charges for Alchemist, 0 for Knight.
        /// Consumed when using skills. Implementation manages the value.
        /// </summary>
        int CurrentResource { get; set; }

        /// <summary>
        /// Maximum skill resource. Set by class/catalyst/level.
        /// 0 means this unit type has no resource (e.g., Knight).
        /// </summary>
        int MaxResource { get; }

        /// <summary>
        /// Display label for the resource: "MP" for Magus, Catalyst name for Alchemist, "" for Knight.
        /// UI uses this to label the resource bar.
        /// </summary>
        string ResourceLabel { get; }

        /// <summary>
        /// Apply damage to this unit. Implementation MUST:
        /// 1. Subtract amount from CurrentHP
        /// 2. Clamp CurrentHP to >= 0
        /// 3. Set IsAlive = false when CurrentHP reaches 0
        /// </summary>
        void TakeDamage(int amount);
    }
}
```

**Copy this verbatim.** Do not add, remove, or rename any members. Note: `UnitType` and `ElementType` enums are defined in `BattleEnums.cs`.

---

## 3. File #2 — IEnemyAI.cs

```csharp
// Assets/Scripts/Battle/Interfaces/IEnemyAI.cs
using System.Collections.Generic;

namespace Battle
{
    /// <summary>
    /// Contract for enemy decision-making AI.
    /// Implemented per-enemy-type (e.g., MagusGruntAI, MagusBossAI).
    /// BattleController calls DecideAction once per living enemy each turn
    /// during the EnemyIntent phase.
    /// </summary>
    public interface IEnemyAI
    {
        /// <summary>
        /// Called during EnemyIntent phase. The enemy picks a target
        /// from the living player party members and declares whether
        /// its upcoming attack is unblockable.
        /// </summary>
        /// <param name="playerParty">Only living members. Guaranteed non-empty.</param>
        /// <returns>An EnemyIntent struct. Target MUST be from the provided list.</returns>
        EnemyIntent DecideAction(IReadOnlyList<IBattleUnit> playerParty);
    }

    /// <summary>
    /// Immutable data container for one enemy's declared intent for this turn.
    /// Created during EnemyIntent phase, consumed during EnemyTurn phase.
    /// </summary>
    public readonly struct EnemyIntent
    {
        public readonly IBattleUnit Target;
        public readonly bool IsUnblockable;
        public readonly int EstimatedDamage;

        public EnemyIntent(IBattleUnit target, bool isUnblockable, int estimatedDamage)
        {
            Target = target;
            IsUnblockable = isUnblockable;
            EstimatedDamage = estimatedDamage;
        }
    }
}
```

**Copy this verbatim.** Note: `EnemyIntent` is now a `readonly struct` to prevent accidental mutation after declaration.

---

## 4. File #3 — BattleEnums.cs

```csharp
// Assets/Scripts/Battle/BattleEnums.cs
namespace Battle
{
    /// <summary>
    /// Top-level state machine states for the battle.
    /// Transitions are managed exclusively by BattleController.
    /// External code can read the current state but must never set it.
    /// </summary>
    public enum BattleState
    {
        None,
        BattleStart,
        PlayerTurn,
        EnemyIntent,
        EnemyTurn,
        TurnEnd,
        BattleWon,
        BattleLost
    }

    /// <summary>
    /// The six player actions available during PlayerTurn phase.
    /// </summary>
    public enum PlayerAction
    {
        None,
        Attack,
        Skill,
        Item,
        Block,
        Dodge
    }

    /// <summary>
    /// Tracks what defensive stance a unit has committed to this turn.
    /// Set during PlayerTurn, active during EnemyTurn, cleared after EnemyTurn.
    /// </summary>
    public enum DefensiveStance
    {
        None,
        Blocking,
        Dodging
    }

    /// <summary>
    /// Unit archetypes. Determines which element pool a unit draws from.
    /// </summary>
    public enum UnitType
    {
        Alchemist,  // Classical elements (Ignis, Ventus, Terra, Aqua)
        Magus,      // Arcane elements (Lux, Umbra)
        Knight      // Melee / element-neutral (mercenaries of the Magus faction)
    }

    /// <summary>
    /// Skill element types.
    /// Classical cycle: Ignis > Ventus > Terra > Aqua > Ignis
    /// Arcane duality: Lux ↔ Umbra (mutually super-effective)
    /// None: element-neutral melee attacks
    /// </summary>
    public enum ElementType
    {
        None,   // Melee / element-neutral
        // --- Classical (Alchemy) ---
        Ignis,  // Fire  — strong vs Ventus, weak vs Aqua
        Ventus, // Wind  — strong vs Terra, weak vs Ignis
        Terra,  // Earth — strong vs Aqua,  weak vs Ventus
        Aqua,   // Water — strong vs Ignis, weak vs Terra
        // --- Arcane (Magus) ---
        Lux,    // Light — mutual with Umbra
        Umbra   // Dark  — mutual with Lux
    }
}
```

**Copy this verbatim.**

---

## 5. File #4 — BattleController.cs (Main Build)

This is the big one. Below is the complete specification broken into labeled sections. Implement **every** section. Use the **EXACT** method signatures, field names, and event signatures shown. Place sections in the order listed.

---

### 5.1 — Class Shell & Usings

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Central orchestrator for turn-based combat in Alchemists vs Magus.
    /// Manages turn flow, action budgets, defensive resolution, element effectiveness, and win/loss.
    /// Player party members act one at a time in order. Each member gets their own AP budget.
    /// Does NOT handle UI rendering — exposes events for a separate UI layer.
    /// </summary>
    public class BattleController : MonoBehaviour
    {
        // ... all contents described in sections 5.2–5.16 go here ...
    }
}
```

**Hard constraints on this class:**
- Do NOT make this a singleton. No `static Instance`. No `static` members of any kind.
- Do NOT use `Update()`, `FixedUpdate()`, or `LateUpdate()` anywhere in this file.
- All turn sequencing uses **Coroutines** (`StartCoroutine` / `yield return`).
- No `async`/`await`. Unity coroutines only.
- No `public` fields. Every serialized field is `[SerializeField] private`.

---

### 5.2 — Serialized Configuration Fields

Place these at the top of the class body inside a `#region Configuration`:

```csharp
#region Configuration

[Header("=== Timing ===")]
[SerializeField, Tooltip("Seconds to display enemy intents before they act")]
private float _intentDisplayDuration = 2.5f;

[SerializeField, Tooltip("Delay between each enemy's attack during EnemyTurn")]
private float _delayBetweenEnemyAttacks = 0.8f;

[SerializeField, Tooltip("Brief pause after player confirms an action")]
private float _postActionDelay = 0.3f;

[SerializeField, Tooltip("Pause before battle start events fire")]
private float _battleStartDelay = 1.0f;

[SerializeField, Tooltip("Pause after battle ends before result event fires")]
private float _battleEndDelay = 1.5f;

[Header("=== Defensive Mechanics ===")]
[SerializeField, Tooltip("Base block chance without shield (0.0 to 1.0)")]
private float _baseBlockChance = 0.99f;

[SerializeField, Tooltip("Block chance when unit has a shield equipped")]
private float _shieldBlockChance = 1.00f;

[SerializeField, Tooltip("Damage multiplier applied when block succeeds (0.01 = 1% damage leaks through)")]
private float _blockDamageMultiplier = 0.01f;

[SerializeField, Tooltip("Damage multiplier when blocking with shield (0.0 = perfect block, no damage)")]
private float _shieldBlockDamageMultiplier = 0.00f;

[SerializeField, Tooltip("Base dodge chance without gear bonus (0.0 to 1.0)")]
private float _baseDodgeChance = 0.75f;

[SerializeField, Tooltip("Maximum dodge chance after gear bonuses (hard cap)")]
private float _maxDodgeChance = 0.95f;

[Header("=== Action Points ===")]
[SerializeField, Tooltip("Action points the player receives each turn")]
private int _maxActionPoints = 2;

[Header("=== Combatants (assign in Inspector or set via InitBattle) ===")]
[SerializeField, Tooltip("Player party units — assign MonoBehaviours that implement IBattleUnit")]
private List<MonoBehaviour> _playerPartyRaw = new List<MonoBehaviour>();

[SerializeField, Tooltip("Enemy units — assign MonoBehaviours that implement IBattleUnit AND IEnemyAI")]
private List<MonoBehaviour> _enemyUnitsRaw = new List<MonoBehaviour>();

#endregion
```

**CRITICAL RULES for these fields:**
- Every number that could be tweaked is a `[SerializeField]`. Zero magic numbers in method bodies.
- `_playerPartyRaw` and `_enemyUnitsRaw` use `MonoBehaviour` type because Unity Inspector cannot serialize interface fields. They are cast to interfaces at runtime in `InitBattle()`.
- Do NOT add `[Range]` attributes — designers should be able to enter any value for testing.

---

### 5.3 — Runtime State Fields

Place inside `#region Runtime State`:

```csharp
#region Runtime State

private BattleState _currentState = BattleState.None;
private int _currentActionPoints;
private DefensiveStance _playerStance = DefensiveStance.None;
private bool _isPlayerInputEnabled;
private int _currentTurnNumber;

// Index of the party member currently taking their turn (into _playerParty).
// Cycles through all living members each turn round.
private int _activePartyIndex;

// Validated combatant lists (populated in InitBattle)
private readonly List<IBattleUnit> _playerParty = new List<IBattleUnit>();
private readonly List<IBattleUnit> _enemyUnits = new List<IBattleUnit>();
private readonly List<IEnemyAI> _enemyAIs = new List<IEnemyAI>();

// Per-unit defensive stances for the current turn round.
// Key = party member, Value = their stance choice.
// Populated as each member acts, cleared after all enemy attacks resolve.
private readonly Dictionary<IBattleUnit, DefensiveStance> _partyStances =
    new Dictionary<IBattleUnit, DefensiveStance>();

// IMPORTANT: _enemyUnits[i] and _enemyAIs[i] MUST correspond to the same
// enemy entity. InitBattle() populates them in lockstep. Never add to one
// without adding to the other at the same index.

// Ordered list of declared intents for the current turn.
// Using List<> (not Dictionary) to guarantee execution order matches declaration order.
private readonly List<(IBattleUnit enemy, EnemyIntent intent)> _declaredIntents =
    new List<(IBattleUnit, EnemyIntent)>();

// Coroutine handle so we can stop the battle loop if needed
private Coroutine _battleCoroutine;

#endregion
```

**KEY CHANGE from previous versions:** `_declaredIntents` is now a `List<(IBattleUnit, EnemyIntent)>` instead of a `Dictionary`. This guarantees enemies execute attacks in the same order they declared intents. `Dictionary<>` does not guarantee iteration order across all .NET runtimes.

---

### 5.4 — Events

Place inside `#region Events`. Use `System.Action` delegates, NOT `UnityEvent`:

```csharp
#region Events

// --- Battle lifecycle ---
/// <summary>Fired once at battle start after combatants are initialized.</summary>
public event Action OnBattleStart;

/// <summary>Fired when the player wins (all enemies dead).</summary>
public event Action OnBattleWon;

/// <summary>Fired when the player loses (all party members dead).</summary>
public event Action OnBattleLost;

// --- State changes ---
/// <summary>
/// Fired whenever the battle state changes.
/// Args: (previous state, new state).
/// UI can use this to swap panels, show/hide elements, etc.
/// </summary>
public event Action<BattleState, BattleState> OnStateChanged;

// --- Turn lifecycle ---
/// <summary>Fired at the start of each new turn. int = turn number.</summary>
public event Action<int> OnTurnStart;

/// <summary>
/// Fired when the player turn begins (once per party member).
/// int = available action points for this party member.
/// Check ActiveUnit to see which party member is acting.
/// </summary>
public event Action<int> OnPlayerTurnStart;

/// <summary>Fired whenever action points change. int = remaining AP.</summary>
public event Action<int> OnActionPointsChanged;

/// <summary>Fired when player input is enabled/disabled. bool = enabled.</summary>
public event Action<bool> OnPlayerInputStateChanged;

/// <summary>Fired when the player turn ends (all AP spent or Attack used).</summary>
public event Action OnPlayerTurnEnd;

/// <summary>Fired when enemy intent phase begins.</summary>
public event Action OnEnemyIntentPhaseStart;

/// <summary>
/// Fired for EACH enemy declaring intent.
/// Args: (enemy unit, target unit, is attack unblockable, estimated damage).
/// UI MUST use this to:
///   1. Draw a targeting line from the enemy to its target
///   2. If isUnblockable == true, apply a RED GLOW to the enemy sprite
/// </summary>
public event Action<IBattleUnit, IBattleUnit, bool, int> OnEnemyIntentDeclared;

/// <summary>Fired when intent display period ends. UI should remove targeting lines and glows.</summary>
public event Action OnEnemyIntentsCleared;

/// <summary>Fired when enemy execution phase begins.</summary>
public event Action OnEnemyTurnStart;

/// <summary>Fired when all enemies have finished attacking.</summary>
public event Action OnEnemyTurnEnd;

/// <summary>Fired at the end of a full turn (after enemy attacks, before next player turn).</summary>
public event Action<int> OnTurnEnd;

// --- Combat resolution ---
/// <summary>Fired when any attack is executed. Args: (attacker, target, raw damage before defense).</summary>
public event Action<IBattleUnit, IBattleUnit, int> OnAttackExecuted;

/// <summary>Fired when a unit successfully dodges. Args: (dodging unit, attacker).</summary>
public event Action<IBattleUnit, IBattleUnit> OnDodgeSuccess;

/// <summary>Fired when a dodge attempt fails. Args: (dodging unit, attacker).</summary>
public event Action<IBattleUnit, IBattleUnit> OnDodgeFailed;

/// <summary>Fired when a block succeeds. Args: (blocking unit, attacker, reduced damage dealt).</summary>
public event Action<IBattleUnit, IBattleUnit, int> OnBlockSuccess;

/// <summary>Fired when a block is bypassed by an unblockable attack. Args: (blocking unit, attacker).</summary>
public event Action<IBattleUnit, IBattleUnit> OnBlockBypassed;

/// <summary>Fired after final damage is applied. Args: (target, attacker, final damage after all modifiers).</summary>
public event Action<IBattleUnit, IBattleUnit, int> OnDamageTaken;

/// <summary>Fired when a unit's HP reaches 0. Args: (dead unit, killing unit).</summary>
public event Action<IBattleUnit, IBattleUnit> OnUnitDied;

// --- Player action feedback ---
/// <summary>
/// Fired when the player selects any action. Args: (action type, target or null).
/// UI can use this for selection animations/sounds.
/// </summary>
public event Action<PlayerAction, IBattleUnit> OnPlayerActionSelected;

/// <summary>
/// Fired when the player's defensive stance changes.
/// Args: (new stance). Fired when stance is SET (Block/Dodge) and when it is CLEARED (None).
/// UI can use this to show/hide shield icon or dodge animation.
/// </summary>
public event Action<DefensiveStance> OnDefensiveStanceChanged;

#endregion
```

**New in v4:** `OnStateChanged`, `OnTurnEnd`, `OnDefensiveStanceChanged` — these were missing and critical for UI integration.

---

### 5.5 — Public Properties (Read-Only Access)

```csharp
#region Public Properties

/// <summary>Current phase of the battle state machine.</summary>
public BattleState CurrentState => _currentState;

/// <summary>How many action points the player has remaining this turn.</summary>
public int CurrentActionPoints => _currentActionPoints;

/// <summary>The player's current defensive commitment (None, Blocking, or Dodging).</summary>
public DefensiveStance PlayerStance => _playerStance;

/// <summary>True when the player can select actions. False during enemy turns and transitions.</summary>
public bool IsPlayerInputEnabled => _isPlayerInputEnabled;

/// <summary>Which turn number we're on (1-indexed, increments each full turn cycle).</summary>
public int CurrentTurnNumber => _currentTurnNumber;

/// <summary>
/// The party member currently taking their turn.
/// Null when it's not the player phase.
/// UI should display this unit's name/portrait as the active actor.
/// </summary>
public IBattleUnit ActiveUnit =>
    _activePartyIndex >= 0 && _activePartyIndex < _playerParty.Count
        ? _playerParty[_activePartyIndex]
        : null;

/// <summary>The player's party. Read-only view. Check IsAlive on each unit.</summary>
public IReadOnlyList<IBattleUnit> PlayerParty => _playerParty;

/// <summary>The enemy roster. Read-only view. Check IsAlive on each unit.</summary>
public IReadOnlyList<IBattleUnit> EnemyUnits => _enemyUnits;

/// <summary>Maximum AP per turn (default 2). Exposed for UI display.</summary>
public int MaxActionPoints => _maxActionPoints;

#endregion
```

**New in v4:** `MaxActionPoints` property — the UI needs to know the max to draw AP pips/bars.

---

### 5.6 — State Transition Helper

```csharp
#region State Management

/// <summary>
/// Transitions to a new battle state and fires OnStateChanged.
/// All state changes MUST go through this method.
/// </summary>
private void SetState(BattleState newState)
{
    if (_currentState == newState) return;
    BattleState previous = _currentState;
    _currentState = newState;
    OnStateChanged?.Invoke(previous, newState);
}

#endregion
```

**CRITICAL:** Every line in BattleController that previously did `_currentState = X` must instead call `SetState(X)`. This ensures `OnStateChanged` always fires. The agent must NOT directly assign `_currentState` anywhere outside this method.

---

### 5.7 — Initialization

```csharp
#region Initialization

/// <summary>
/// Call this to start a battle. Can be called from a scene loader,
/// cutscene manager, or directly via StartBattle() for testing.
/// If combatants were assigned in Inspector, pass null for both params.
/// </summary>
/// <param name="playerParty">Player units, or null to use Inspector list.</param>
/// <param name="enemies">Enemy units (must also implement IEnemyAI), or null to use Inspector list.</param>
public void InitBattle(List<IBattleUnit> playerParty = null, List<IBattleUnit> enemies = null)
{
    // Prevent starting on a disabled GameObject (coroutines won't run)
    if (!gameObject.activeInHierarchy)
    {
        Debug.LogError("[BattleController] Cannot start battle: GameObject is inactive! Coroutines require an active GameObject.");
        return;
    }

    // Clear previous state
    _playerParty.Clear();
    _enemyUnits.Clear();
    _enemyAIs.Clear();
    _declaredIntents.Clear();
    _partyStances.Clear();
    _activePartyIndex = -1;

    // --- Populate player party ---
    if (playerParty != null)
    {
        _playerParty.AddRange(playerParty);
    }
    else
    {
        foreach (var mb in _playerPartyRaw)
        {
            if (mb == null)
            {
                Debug.LogWarning("[BattleController] Null entry in player party Inspector list. Skipping.");
                continue;
            }
            if (mb is IBattleUnit unit)
                _playerParty.Add(unit);
            else
                Debug.LogError($"[BattleController] {mb.name} does not implement IBattleUnit!", mb);
        }
    }

    // --- Populate enemies (must implement BOTH IBattleUnit and IEnemyAI) ---
    if (enemies != null)
    {
        foreach (var enemy in enemies)
        {
            if (enemy == null)
            {
                Debug.LogWarning("[BattleController] Null enemy passed to InitBattle. Skipping.");
                continue;
            }
            _enemyUnits.Add(enemy);
            if (enemy is IEnemyAI ai)
                _enemyAIs.Add(ai);
            else
                Debug.LogError($"[BattleController] Enemy '{enemy.UnitName}' does not implement IEnemyAI! AI will be missing.");
        }
    }
    else
    {
        foreach (var mb in _enemyUnitsRaw)
        {
            if (mb == null)
            {
                Debug.LogWarning("[BattleController] Null entry in enemy Inspector list. Skipping.");
                continue;
            }
            if (mb is IBattleUnit unit && mb is IEnemyAI ai)
            {
                _enemyUnits.Add(unit);
                _enemyAIs.Add(ai);
            }
            else
            {
                Debug.LogError($"[BattleController] {mb.name} must implement both IBattleUnit and IEnemyAI!", mb);
            }
        }
    }

    // --- Validate parallel arrays ---
    if (_enemyUnits.Count != _enemyAIs.Count)
    {
        Debug.LogError($"[BattleController] Enemy list mismatch: {_enemyUnits.Count} units but {_enemyAIs.Count} AIs. " +
                       "Every enemy must implement both IBattleUnit and IEnemyAI.");
        return;
    }

    // --- Validate minimum combatants ---
    if (_playerParty.Count == 0)
    {
        Debug.LogError("[BattleController] Cannot start battle: no player party members!");
        return;
    }
    if (_enemyUnits.Count == 0)
    {
        Debug.LogError("[BattleController] Cannot start battle: no enemies!");
        return;
    }

    // --- Reset runtime state ---
    _currentTurnNumber = 0;
    _playerStance = DefensiveStance.None;
    _isPlayerInputEnabled = false;
    _currentActionPoints = 0;

    // --- Stop any existing battle coroutine ---
    if (_battleCoroutine != null)
    {
        StopCoroutine(_battleCoroutine);
        _battleCoroutine = null;
    }

    SetState(BattleState.None);
    _battleCoroutine = StartCoroutine(BattleLoop());
}

#endregion
```

**Changes from v2:**
- Added `gameObject.activeInHierarchy` check (coroutines silently fail on inactive objects)
- Added parallel array count validation (`_enemyUnits.Count != _enemyAIs.Count`)
- Null check on programmatic `enemies` parameter (was only checking Inspector list)
- Uses `SetState()` instead of direct assignment

---

### 5.8 — Battle Loop (Coroutine)

This is the heart of the controller. The key change from v3 is that the player phase now **cycles through each living party member one at a time**. Each member gets their own AP budget and can independently choose Attack/Skill/Item/Block/Dodge. After ALL living members have acted, the enemy phase begins.

Implement EXACTLY this flow:

```csharp
#region Battle Loop

private IEnumerator BattleLoop()
{
    // === BATTLE START ===
    SetState(BattleState.BattleStart);
    yield return new WaitForSeconds(_battleStartDelay);
    OnBattleStart?.Invoke();

    // === MAIN LOOP ===
    while (true)
    {
        _currentTurnNumber++;

        // --- PLAYER PHASE: each living party member acts in order ---
        SetState(BattleState.PlayerTurn);
        _partyStances.Clear();
        OnTurnStart?.Invoke(_currentTurnNumber);

        for (_activePartyIndex = 0; _activePartyIndex < _playerParty.Count; _activePartyIndex++)
        {
            IBattleUnit member = _playerParty[_activePartyIndex];

            // Skip dead party members
            if (!member.IsAlive) continue;

            // Reset per-member state
            _currentActionPoints = _maxActionPoints;
            SetPlayerStance(DefensiveStance.None);
            _isPlayerInputEnabled = true;

            OnPlayerTurnStart?.Invoke(_currentActionPoints);
            OnActionPointsChanged?.Invoke(_currentActionPoints);
            OnPlayerInputStateChanged?.Invoke(true);

            // Wait until this member has used all AP or chosen Attack
            yield return new WaitUntil(() => !_isPlayerInputEnabled);

            yield return new WaitForSeconds(_postActionDelay);

            // Check if enemies all died from this member's actions
            if (CheckBattleEnd())
            {
                _battleCoroutine = null;
                yield break;
            }
        }

        // All party members have acted
        _activePartyIndex = -1;
        OnPlayerTurnEnd?.Invoke();

        // --- ENEMY INTENT PHASE ---
        SetState(BattleState.EnemyIntent);
        _declaredIntents.Clear();
        OnEnemyIntentPhaseStart?.Invoke();

        DeclareAllEnemyIntents();

        // Hold so the player can read the intents / targeting lines / red glows
        yield return new WaitForSeconds(_intentDisplayDuration);

        OnEnemyIntentsCleared?.Invoke();

        // --- ENEMY TURN ---
        SetState(BattleState.EnemyTurn);
        OnEnemyTurnStart?.Invoke();

        yield return ExecuteEnemyAttacks();

        OnEnemyTurnEnd?.Invoke();

        // Clear all party stances AFTER all enemies have attacked
        _partyStances.Clear();
        SetPlayerStance(DefensiveStance.None);

        // --- CHECK: Did player party all die? ---
        if (CheckBattleEnd())
        {
            _battleCoroutine = null;
            yield break;
        }

        // --- TURN END ---
        SetState(BattleState.TurnEnd);
        // TODO: Tick status effects, cooldowns, buffs/debuffs, poison damage, regen, etc.
        OnTurnEnd?.Invoke(_currentTurnNumber);
        yield return new WaitForSeconds(_postActionDelay);
    }
}

#endregion
```

**CRITICAL DETAILS the agent MUST get right:**
1. Party members act **one at a time** in list order. Dead members are skipped. Each member gets fresh AP.
2. `_playerStance` is reset to `None` at the START of each member's sub-turn. `_partyStances` dictionary accumulates each member's choice. `_partyStances` is cleared AFTER all enemy attacks resolve.
3. `_isPlayerInputEnabled` is the per-member gate. The `WaitUntil` checks every frame. Player action methods set it to `false` when AP runs out or Attack is chosen.
4. `CheckBattleEnd()` is called **after each party member acts** AND after the enemy turn. When it returns true, set `_battleCoroutine = null` then use `yield break`.
5. `_activePartyIndex` is set to `-1` after the party loop completes (so `ActiveUnit` returns null during enemy phases).
6. All state changes use `SetState()`, never direct assignment.
7. `OnTurnEnd` fires with the turn number at the end of each complete turn cycle.

---

### 5.9 — Defensive Stance Helper

```csharp
#region Defensive Stance

/// <summary>
/// Sets the active party member's defensive stance and fires the OnDefensiveStanceChanged event.
/// Also stores the stance in _partyStances so ResolveDefense can look it up per-unit
/// when enemies attack during the enemy phase.
/// All stance changes MUST go through this method.
/// </summary>
private void SetPlayerStance(DefensiveStance newStance)
{
    if (_playerStance == newStance) return;
    _playerStance = newStance;

    // Record this unit's stance choice for the enemy turn
    IBattleUnit active = ActiveUnit;
    if (active != null && newStance != DefensiveStance.None)
    {
        _partyStances[active] = newStance;
    }

    OnDefensiveStanceChanged?.Invoke(newStance);
}

/// <summary>
/// Returns the defensive stance a specific party member chose this turn.
/// Used during enemy attack resolution to check each target's individual stance.
/// </summary>
private DefensiveStance GetUnitStance(IBattleUnit unit)
{
    return _partyStances.TryGetValue(unit, out DefensiveStance stance) ? stance : DefensiveStance.None;
}

#endregion
```

**CRITICAL:** Every line that previously did `_playerStance = X` must instead call `SetPlayerStance(X)`. This ensures `OnDefensiveStanceChanged` always fires. The `_partyStances` dictionary accumulates each member's stance choice across the full party turn round — it is only cleared after all enemy attacks resolve.

---

### 5.10 — Enemy Intent Declaration

```csharp
#region Enemy Intent

private void DeclareAllEnemyIntents()
{
    List<IBattleUnit> livingPlayers = GetLivingUnits(_playerParty);

    if (livingPlayers.Count == 0)
        return;

    for (int i = 0; i < _enemyUnits.Count; i++)
    {
        IBattleUnit enemy = _enemyUnits[i];
        if (!enemy.IsAlive) continue;

        IEnemyAI ai = _enemyAIs[i];
        EnemyIntent intent = ai.DecideAction(livingPlayers.AsReadOnly());

        // Safety: if AI returned a dead/null target, default to first living player
        IBattleUnit validTarget = intent.Target;
        if (validTarget == null || !validTarget.IsAlive)
        {
            Debug.LogWarning($"[BattleController] Enemy '{enemy.UnitName}' AI returned invalid target. Defaulting to first living player.");
            validTarget = livingPlayers[0];
            intent = new EnemyIntent(validTarget, intent.IsUnblockable, intent.EstimatedDamage);
        }

        _declaredIntents.Add((enemy, intent));

        // Broadcast for UI:
        // 1. Draw a targeting line from this enemy to intent.Target
        // 2. If intent.IsUnblockable == true, apply RED GLOW on the enemy sprite
        OnEnemyIntentDeclared?.Invoke(enemy, validTarget, intent.IsUnblockable, intent.EstimatedDamage);
    }
}

#endregion
```

**Change from v2:** Reconstructs the `EnemyIntent` with the corrected target when fallback occurs (the `readonly struct` prevents mutation).

---

### 5.11 — Enemy Attack Execution

```csharp
#region Enemy Attack Execution

private IEnumerator ExecuteEnemyAttacks()
{
    // Iterate over the ordered intents list (order matches declaration order)
    // We snapshot the count so additions during iteration don't affect us
    int count = _declaredIntents.Count;

    for (int i = 0; i < count; i++)
    {
        IBattleUnit attacker = _declaredIntents[i].enemy;
        EnemyIntent intent = _declaredIntents[i].intent;
        IBattleUnit target = intent.Target;

        // Skip dead enemies (may have died from counterattack effects or traps)
        if (!attacker.IsAlive) continue;

        // Retarget if original target died from a previous enemy's attack this phase
        if (!target.IsAlive)
        {
            IBattleUnit newTarget = GetFirstLivingUnit(_playerParty);
            if (newTarget == null)
            {
                // All players dead — stop attacking immediately
                yield break;
            }
            target = newTarget;
        }

        int rawDamage = CalculateRawDamage(attacker, target);
        OnAttackExecuted?.Invoke(attacker, target, rawDamage);

        int finalDamage = ResolveDefense(target, attacker, rawDamage, intent.IsUnblockable);

        if (finalDamage > 0)
        {
            target.TakeDamage(finalDamage);
            OnDamageTaken?.Invoke(target, attacker, finalDamage);

            if (!target.IsAlive)
            {
                OnUnitDied?.Invoke(target, attacker);
            }
        }

        yield return new WaitForSeconds(_delayBetweenEnemyAttacks);
    }
}

#endregion
```

**Change from v2:** Uses indexed `for` loop over the `List<>` instead of `foreach` over a `Dictionary` snapshot. Cleaner, safer, and guarantees execution order.

---

### 5.12 — Defense Resolution

This is the most complex logic. Implement it EXACTLY as follows:

```csharp
#region Defense Resolution

/// <summary>
/// Resolves dodge/block mechanics against an incoming attack.
/// Resolution order: Dodge first → Block second → Full damage.
/// Returns the final damage to apply (0 if fully dodged or perfectly blocked with shield).
/// 
/// IMPORTANT DESIGN RULES:
/// - Dodge CAN avoid unblockable attacks (evasion is not blocked)
/// - Block CANNOT stop unblockable attacks (brute force breaks the guard)
/// - Only player party members can dodge/block (not enemies)
/// - Each party member's stance is looked up individually from _partyStances
/// </summary>
private int ResolveDefense(IBattleUnit target, IBattleUnit attacker, int rawDamage, bool isUnblockable)
{
    bool isPlayerTarget = _playerParty.Contains(target);
    DefensiveStance targetStance = isPlayerTarget ? GetUnitStance(target) : DefensiveStance.None;

    // ========================================
    // STEP 1: CHECK DODGE
    // Dodge CAN avoid unblockable attacks.
    // ========================================
    if (isPlayerTarget && targetStance == DefensiveStance.Dodging)
    {
        float dodgeChance = Mathf.Clamp(
            _baseDodgeChance + target.DodgeBonus,
            0f,
            _maxDodgeChance
        );

        // Unity's Random.Range(float, float) returns [min, max) — 0f inclusive, 1f exclusive.
        // So roll < dodgeChance correctly represents the probability.
        float roll = UnityEngine.Random.Range(0f, 1f);

        if (roll < dodgeChance)
        {
            OnDodgeSuccess?.Invoke(target, attacker);
            return 0; // Complete miss — no damage
        }
        else
        {
            OnDodgeFailed?.Invoke(target, attacker);
            // Fall through — dodge failed, check block next
        }
    }

    // ========================================
    // STEP 2: CHECK BLOCK
    // Block CANNOT stop unblockable attacks.
    // ========================================
    if (isPlayerTarget && targetStance == DefensiveStance.Blocking)
    {
        if (isUnblockable)
        {
            // Unblockable attack — block is completely bypassed
            OnBlockBypassed?.Invoke(target, attacker);
            // Fall through to full damage — do NOT fire OnBlockSuccess
        }
        else
        {
            float blockChance = target.HasShield ? _shieldBlockChance : _baseBlockChance;
            float damageMultiplier = target.HasShield ? _shieldBlockDamageMultiplier : _blockDamageMultiplier;

            float roll = UnityEngine.Random.Range(0f, 1f);

            if (roll < blockChance)
            {
                // Block SUCCESS
                // CeilToInt ensures block always leaks at least 1 damage unless multiplier is exactly 0.0
                // With shield (multiplier 0.0): CeilToInt(rawDamage * 0.0) = 0 → perfect block
                // Without shield (multiplier 0.01): CeilToInt(100 * 0.01) = CeilToInt(1.0) = 1 → 1 damage leaks
                int reducedDamage = Mathf.CeilToInt(rawDamage * damageMultiplier);
                OnBlockSuccess?.Invoke(target, attacker, reducedDamage);
                return reducedDamage;
            }
            // Block failed (1% chance without shield) — full damage
        }
    }

    // ========================================
    // STEP 3: FULL DAMAGE
    // ========================================
    return rawDamage;
}

#endregion
```

**RULES the agent MUST NOT violate:**
1. Dodge is checked BEFORE Block. Always. Every time. Never reversed.
2. Dodge works against unblockable attacks. Block does NOT work against unblockable attacks.
3. `_playerParty.Contains(target)` ensures defense only applies to player units.
4. Use `UnityEngine.Random.Range(0f, 1f)`, NOT `System.Random`. Unity's version is `[0, 1)` for floats.
5. `Mathf.CeilToInt` for block damage (rounds UP). With shield's 0.0 multiplier, `CeilToInt(0) = 0`.
6. When block is bypassed by unblockable, fire ONLY `OnBlockBypassed`. Do NOT also fire `OnBlockSuccess`.
7. Each party member can only be in ONE stance (Dodging OR Blocking, never both). The `DefensiveStance` enum and stance validation in SelectBlock/SelectDodge enforce this per member. `ResolveDefense` uses `GetUnitStance(target)` to look up the targeted member's individual stance from `_partyStances`. The `if` checks (not `else if`) are intentional: a failed dodge SHOULD fall through to a block check for extensibility.

---

### 5.13 — Damage Calculation & Element Effectiveness

```csharp
#region Damage Calculation

[Header("=== Element Effectiveness ===")]
[SerializeField, Tooltip("Damage multiplier when attacker's element is strong vs target's element")]
private float _elementStrongMultiplier = 1.5f;

[SerializeField, Tooltip("Damage multiplier when attacker's element is weak vs target's element")]
private float _elementWeakMultiplier = 0.5f;

/// <summary>
/// Calculates base damage before defensive modifiers, including element effectiveness.
/// Formula: (attacker.AttackPower - target.Defense) * elementMultiplier, minimum 1.
/// Guaranteed to return at least 1 so no attack is completely wasted.
/// </summary>
private int CalculateRawDamage(IBattleUnit attacker, IBattleUnit target)
{
    int baseDamage = attacker.AttackPower - target.Defense;
    float multiplier = GetElementMultiplier(attacker.Element, target.Element);
    int damage = Mathf.RoundToInt(baseDamage * multiplier);
    return Mathf.Max(damage, 1);
}

/// <summary>
/// Returns the damage multiplier based on element matchup.
/// Classical cycle: Ignis > Ventus > Terra > Aqua > Ignis
/// Arcane duality: Lux ↔ Umbra (both strong against each other)
/// None or same element: neutral (1.0x)
/// Cross-family (e.g. Ignis vs Lux): neutral (1.0x)
/// </summary>
private float GetElementMultiplier(ElementType attackerElement, ElementType targetElement)
{
    if (attackerElement == ElementType.None || targetElement == ElementType.None)
        return 1f;
    if (attackerElement == targetElement)
        return 1f;

    // --- Classical cycle: Ignis > Ventus > Terra > Aqua > Ignis ---
    bool isStrong =
        (attackerElement == ElementType.Ignis  && targetElement == ElementType.Ventus) ||
        (attackerElement == ElementType.Ventus && targetElement == ElementType.Terra)  ||
        (attackerElement == ElementType.Terra  && targetElement == ElementType.Aqua)   ||
        (attackerElement == ElementType.Aqua   && targetElement == ElementType.Ignis);

    if (isStrong) return _elementStrongMultiplier;

    bool isWeak =
        (attackerElement == ElementType.Ignis  && targetElement == ElementType.Aqua)   ||
        (attackerElement == ElementType.Ventus && targetElement == ElementType.Ignis)  ||
        (attackerElement == ElementType.Terra  && targetElement == ElementType.Ventus) ||
        (attackerElement == ElementType.Aqua   && targetElement == ElementType.Terra);

    if (isWeak) return _elementWeakMultiplier;

    // --- Arcane duality: Lux ↔ Umbra (mutually super-effective) ---
    bool isArcane =
        (attackerElement == ElementType.Lux   && targetElement == ElementType.Umbra) ||
        (attackerElement == ElementType.Umbra && targetElement == ElementType.Lux);

    if (isArcane) return _elementStrongMultiplier;

    // Cross-family or unmatched — neutral
    return 1f;
}

#endregion
```

**Element effectiveness rules:**
- **Classical cycle (Alchemy):** Ignis > Ventus > Terra > Aqua > Ignis. Strong = 1.5x, Weak = 0.5x.
- **Arcane duality (Magus):** Lux ↔ Umbra — mutually super-effective at 1.5x in both directions.
- **Same element, None, or cross-family:** neutral 1.0x (e.g. Ignis vs Lux = neutral).
- Multipliers are `[SerializeField]` for tuning.

---

### 5.14 — Player Input Methods

These are called by UI buttons. Each must validate state before executing.

```csharp
#region Player Input

/// <summary>
/// Player attacks a target. Costs 1 AP. ALWAYS ends the turn immediately
/// regardless of remaining AP. This is the ONLY action that force-ends the turn.
/// </summary>
public void SelectAttack(IBattleUnit target)
{
    if (!ValidateInput(PlayerAction.Attack, 1)) return;
    if (target == null || !target.IsAlive)
    {
        Debug.LogWarning("[BattleController] SelectAttack: invalid or dead target.");
        return;
    }

    OnPlayerActionSelected?.Invoke(PlayerAction.Attack, target);

    // The attacker is the currently active party member
    IBattleUnit attacker = ActiveUnit;
    if (attacker == null || !attacker.IsAlive)
    {
        Debug.LogError("[BattleController] SelectAttack: no active living party member!");
        return;
    }

    int rawDamage = CalculateRawDamage(attacker, target);
    OnAttackExecuted?.Invoke(attacker, target, rawDamage);

    target.TakeDamage(rawDamage);
    OnDamageTaken?.Invoke(target, attacker, rawDamage);

    if (!target.IsAlive)
    {
        OnUnitDied?.Invoke(target, attacker);
    }

    // Attack ALWAYS ends the turn — set AP to 0 regardless of how much was remaining
    _currentActionPoints = 0;
    OnActionPointsChanged?.Invoke(0);
    EndPlayerInput();
}

/// <summary>
/// Player uses a skill. Costs 1 AP. Does NOT end the turn.
/// Player can still take another action if AP remains.
/// </summary>
public void SelectSkill(int skillIndex, IBattleUnit target)
{
    if (!ValidateInput(PlayerAction.Skill, 1)) return;

    OnPlayerActionSelected?.Invoke(PlayerAction.Skill, target);

    // TODO: Implement skill system — look up skill by index from party member's skill list,
    // execute effect (damage, heal, buff, debuff), calculate costs, apply cooldowns
    Debug.Log($"[BattleController] Skill {skillIndex} used on {target?.UnitName ?? "no target"}. (Not yet implemented)");

    ConsumeActionPoints(1);
}

/// <summary>
/// Player uses an item. Costs 1 AP. Does NOT end the turn.
/// Player can still take another action if AP remains.
/// </summary>
public void SelectItem(int itemIndex, IBattleUnit target)
{
    if (!ValidateInput(PlayerAction.Item, 1)) return;

    OnPlayerActionSelected?.Invoke(PlayerAction.Item, target);

    // TODO: Implement item system — look up item from inventory by index,
    // apply effect (heal, cure, throwable damage, buff), consume from inventory
    Debug.Log($"[BattleController] Item {itemIndex} used on {target?.UnitName ?? "no target"}. (Not yet implemented)");

    ConsumeActionPoints(1);
}

/// <summary>
/// Player chooses to Block. Costs ALL remaining AP. Sets defensive stance to Blocking.
/// Cannot be chosen if already in a defensive stance this turn.
/// Block: 99% chance to reduce damage to 1% (100% / 0% with shield).
/// Bypassed entirely by unblockable attacks.
/// </summary>
public void SelectBlock()
{
    if (!ValidateInput(PlayerAction.Block, 1)) return;
    if (_playerStance != DefensiveStance.None)
    {
        Debug.LogWarning("[BattleController] Already in a defensive stance this turn. Cannot Block.");
        return;
    }

    OnPlayerActionSelected?.Invoke(PlayerAction.Block, null);
    SetPlayerStance(DefensiveStance.Blocking);

    // Consumes ALL remaining AP — player is committing to defense for this turn
    _currentActionPoints = 0;
    OnActionPointsChanged?.Invoke(0);
    EndPlayerInput();
}

/// <summary>
/// Player chooses to Dodge. Costs ALL remaining AP. Sets defensive stance to Dodging.
/// Cannot be chosen if already in a defensive stance this turn.
/// Dodge: 75% base chance (increased by gear, capped at 95%).
/// CAN avoid unblockable attacks (unlike Block).
/// </summary>
public void SelectDodge()
{
    if (!ValidateInput(PlayerAction.Dodge, 1)) return;
    if (_playerStance != DefensiveStance.None)
    {
        Debug.LogWarning("[BattleController] Already in a defensive stance this turn. Cannot Dodge.");
        return;
    }

    OnPlayerActionSelected?.Invoke(PlayerAction.Dodge, null);
    SetPlayerStance(DefensiveStance.Dodging);

    // Consumes ALL remaining AP — player is committing to defense for this turn
    _currentActionPoints = 0;
    OnActionPointsChanged?.Invoke(0);
    EndPlayerInput();
}

#endregion
```

**Change from v3:** `SelectAttack` now uses `ActiveUnit` — the currently acting party member — instead of `GetFirstLivingUnit`. Each party member attacks on their own sub-turn.

---

### 5.15 — Input Validation & AP Management

```csharp
#region Input Validation

/// <summary>
/// Validates that the player can perform an action right now.
/// Checks: input enabled, correct state, sufficient AP.
/// Returns false and logs a warning if any check fails.
/// </summary>
private bool ValidateInput(PlayerAction action, int apCost)
{
    if (!_isPlayerInputEnabled)
    {
        Debug.LogWarning($"[BattleController] Input not enabled. Cannot perform {action}.");
        return false;
    }
    if (_currentState != BattleState.PlayerTurn)
    {
        Debug.LogWarning($"[BattleController] Not in PlayerTurn state (currently {_currentState}). Cannot perform {action}.");
        return false;
    }
    if (_currentActionPoints < apCost)
    {
        Debug.LogWarning($"[BattleController] Not enough AP ({_currentActionPoints}/{_maxActionPoints}) for {action} (costs {apCost}).");
        return false;
    }
    return true;
}

/// <summary>
/// Deducts AP and checks if the turn should end naturally (AP exhausted).
/// Called by non-turn-ending actions: Skill, Item.
/// NOT called by Attack (force-ends), Block, or Dodge (consume all AP directly).
/// </summary>
private void ConsumeActionPoints(int amount)
{
    _currentActionPoints = Mathf.Max(0, _currentActionPoints - amount);
    OnActionPointsChanged?.Invoke(_currentActionPoints);

    if (_currentActionPoints <= 0)
    {
        EndPlayerInput();
    }
}

/// <summary>
/// Disables player input, which unblocks the WaitUntil in BattleLoop.
/// This is the ONLY mechanism that advances past the player turn.
/// </summary>
private void EndPlayerInput()
{
    _isPlayerInputEnabled = false;
    OnPlayerInputStateChanged?.Invoke(false);
}

#endregion
```

---

### 5.16 — UI Query Helpers

These methods let the UI determine what actions are currently available without duplicating validation logic:

```csharp
#region UI Query Helpers

/// <summary>
/// Returns true if the given action can be performed right now.
/// UI should call this to determine which buttons to enable/disable.
/// Does NOT execute the action or log warnings.
/// </summary>
public bool CanPerformAction(PlayerAction action)
{
    if (!_isPlayerInputEnabled) return false;
    if (_currentState != BattleState.PlayerTurn) return false;
    if (_currentActionPoints < 1) return false;

    // Block and Dodge require no existing defensive stance
    if (action == PlayerAction.Block || action == PlayerAction.Dodge)
    {
        if (_playerStance != DefensiveStance.None) return false;
    }

    return true;
}

/// <summary>
/// Returns the AP cost for a given action. UI can display this on buttons.
/// Note: Block and Dodge consume ALL remaining AP, but require at least 1.
/// </summary>
public int GetActionPointCost(PlayerAction action)
{
    switch (action)
    {
        case PlayerAction.Attack: return 1;
        case PlayerAction.Skill:  return 1;
        case PlayerAction.Item:   return 1;
        case PlayerAction.Block:  return _currentActionPoints; // All remaining
        case PlayerAction.Dodge:  return _currentActionPoints; // All remaining
        default: return 0;
    }
}

/// <summary>
/// Returns a list of all currently available actions.
/// Convenience method — calls CanPerformAction for each PlayerAction.
/// </summary>
public List<PlayerAction> GetAvailableActions()
{
    List<PlayerAction> available = new List<PlayerAction>();
    foreach (PlayerAction action in Enum.GetValues(typeof(PlayerAction)))
    {
        if (action == PlayerAction.None) continue;
        if (CanPerformAction(action))
            available.Add(action);
    }
    return available;
}

#endregion
```

**New in v4.** These are essential for UI integration — without them, every UI script has to reverse-engineer the validation rules.

---

### 5.17 — Win/Loss Checking

```csharp
#region Win/Loss

/// <summary>
/// Checks if all enemies or all players are dead.
/// If so, starts the end battle sequence and returns true.
/// Caller MUST yield break when this returns true.
/// </summary>
private bool CheckBattleEnd()
{
    bool allEnemiesDead = GetFirstLivingUnit(_enemyUnits) == null;
    bool allPlayersDead = GetFirstLivingUnit(_playerParty) == null;

    if (allEnemiesDead)
    {
        StartCoroutine(EndBattle(true));
        return true;
    }

    if (allPlayersDead)
    {
        StartCoroutine(EndBattle(false));
        return true;
    }

    return false;
}

/// <summary>
/// Final sequence after a battle concludes. Disables input, waits, fires result event.
/// Runs as its own coroutine because BattleLoop has already yield-breaked.
/// </summary>
private IEnumerator EndBattle(bool playerWon)
{
    _isPlayerInputEnabled = false;
    OnPlayerInputStateChanged?.Invoke(false);

    yield return new WaitForSeconds(_battleEndDelay);

    if (playerWon)
    {
        SetState(BattleState.BattleWon);
        OnBattleWon?.Invoke();
    }
    else
    {
        SetState(BattleState.BattleLost);
        OnBattleLost?.Invoke();
    }
}

#endregion
```

---

### 5.18 — Utility Methods

```csharp
#region Utilities

/// <summary>
/// Returns the first living unit in a list, or null if all are dead.
/// </summary>
private IBattleUnit GetFirstLivingUnit(List<IBattleUnit> units)
{
    for (int i = 0; i < units.Count; i++)
    {
        if (units[i] != null && units[i].IsAlive)
            return units[i];
    }
    return null;
}

/// <summary>
/// Returns a new list containing only the living units from the input list.
/// </summary>
private List<IBattleUnit> GetLivingUnits(List<IBattleUnit> units)
{
    List<IBattleUnit> living = new List<IBattleUnit>();
    for (int i = 0; i < units.Count; i++)
    {
        if (units[i] != null && units[i].IsAlive)
            living.Add(units[i]);
    }
    return living;
}

/// <summary>
/// Convenience entry point that uses Inspector-assigned combatants.
/// Can be called from Awake(), Start(), or a UI "Start Battle" button.
/// </summary>
public void StartBattle()
{
    InitBattle(null, null);
}

/// <summary>
/// Stops the battle immediately. Use for flee, abort, or scene transition scenarios.
/// Does NOT fire OnBattleWon or OnBattleLost.
/// </summary>
public void AbortBattle()
{
    if (_battleCoroutine != null)
    {
        StopCoroutine(_battleCoroutine);
        _battleCoroutine = null;
    }

    _isPlayerInputEnabled = false;
    _activePartyIndex = -1;
    SetPlayerStance(DefensiveStance.None);
    _partyStances.Clear();
    _declaredIntents.Clear();
    OnPlayerInputStateChanged?.Invoke(false);
    SetState(BattleState.None);
}

#endregion
```

---

## 6. Valid Action Combinations (Reference Table)

This table defines every legal 2-AP sub-turn **per party member**. Each member independently gets 2 AP and can choose any valid combination. The agent does NOT need to encode this table in code — it emerges naturally from the rules. This is here so the agent can **cross-check correctness** after implementation.

| First Action | Second Action | Result |
|-------------|---------------|--------|
| Attack | *(forfeited)* | Turn ends immediately. Second action is lost. |
| Skill | Attack | Skill executes, then attack ends turn. |
| Skill | Skill | Both execute. Turn ends (0 AP). |
| Skill | Item | Both execute. Turn ends (0 AP). |
| Skill | Block | Skill executes, then Blocking stance. Turn ends. |
| Skill | Dodge | Skill executes, then Dodging stance. Turn ends. |
| Item | Attack | Item executes, then attack ends turn. |
| Item | Item | Both execute. Turn ends (0 AP). |
| Item | Skill | Both execute. Turn ends (0 AP). |
| Item | Block | Item executes, then Blocking stance. Turn ends. |
| Item | Dodge | Item executes, then Dodging stance. Turn ends. |
| Block | *(forfeited)* | Blocking stance. All AP consumed. Turn ends. |
| Dodge | *(forfeited)* | Dodging stance. All AP consumed. Turn ends. |
| Block then Dodge | ❌ INVALID | Stance check rejects Dodge (already Blocking). |
| Dodge then Block | ❌ INVALID | Stance check rejects Block (already Dodging). |
| Attack then anything | ❌ IMPOSSIBLE | Attack sets AP to 0, input disabled. No second action possible. |

---

## 7. Edge Cases the Agent MUST Handle

| # | Edge Case | Where It's Handled | What Goes Wrong If Missed |
|---|-----------|-------------------|---------------------------|
| 1 | Player attacks as first action → second action forfeited | `SelectAttack` sets AP to 0, calls `EndPlayerInput()` | Player gets free extra action |
| 2 | Player tries to Block AND Dodge same turn | `SelectBlock`/`SelectDodge` check `_playerStance != None` | Player gets both defenses simultaneously |
| 3 | Player tries to act when input disabled | `ValidateInput` checks `_isPlayerInputEnabled` | Actions fire during enemy turn |
| 4 | Player tries to act outside PlayerTurn state | `ValidateInput` checks `_currentState` | Actions fire during wrong phase |
| 5 | Enemy targets player who dies mid-enemy-turn | `ExecuteEnemyAttacks` retargets to first living player | NullRef or damage to dead unit |
| 6 | All players die mid-enemy-turn | `ExecuteEnemyAttacks` yield-breaks if no living target | Enemies attack dead units |
| 7 | Enemy AI returns null/dead target | `DeclareAllEnemyIntents` defaults to first living player | NullRef in intent phase |
| 8 | All enemies die during player turn | `CheckBattleEnd()` after player turn detects win | Battle loop continues with no enemies |
| 9 | Enemy dies mid-enemy-turn (future effects) | `ExecuteEnemyAttacks` skips dead attackers | Dead enemy attacks |
| 10 | Unblockable vs Dodge | `ResolveDefense`: dodge checked BEFORE unblockable flag | Player can't dodge unblockable (wrong!) |
| 11 | Unblockable vs Block | `ResolveDefense`: block checks `isUnblockable` flag | Block incorrectly stops unblockable |
| 12 | Shield: 100% block + 0% damage | Separate `_shieldBlockChance` / `_shieldBlockDamageMultiplier` | Shield doesn't work properly |
| 13 | Defensive stance persists through enemy turn | `SetPlayerStance(None)` called AFTER `ExecuteEnemyAttacks` | Stance drops before last enemy |
| 14 | Defensive stance cleared for next turn | `SetPlayerStance(None)` at START of PlayerTurn | Stance carries into next turn |
| 15 | No combatants assigned | `InitBattle` validates counts, returns early | NullRef / infinite empty loop |
| 16 | Battle started while another runs | `InitBattle` stops existing coroutine first | Two battle loops run in parallel |
| 17 | 0 or negative raw damage | `CalculateRawDamage` floors at 1 | Attacks deal no damage |
| 18 | Null entries in Inspector lists | `InitBattle` null-checks each MonoBehaviour | NullRef during interface cast |
| 19 | Enemy attack order must be deterministic | `_declaredIntents` is `List<>` not `Dictionary<>` | Random/inconsistent attack order |
| 20 | Inactive GameObject prevents coroutines | `InitBattle` checks `gameObject.activeInHierarchy` | Silent failure, battle never starts |
| 21 | Active party member is dead | BattleLoop skips dead members; `SelectAttack` validates `ActiveUnit.IsAlive` | Dead unit executes attack |
| 22 | Parallel array desync (units vs AIs) | `InitBattle` validates `_enemyUnits.Count == _enemyAIs.Count` | Index out of bounds in intent phase |
| 23 | `_battleCoroutine` handle stale after yield break | Set to `null` before `yield break` in BattleLoop | `AbortBattle` tries to stop finished coroutine |
| 24 | State change without event | All state changes go through `SetState()` | UI doesn't know phase changed |
| 25 | Stance change without event | All stance changes go through `SetPlayerStance()` | UI doesn't update defense indicators |
| 26 | Per-unit stance lookup during enemy phase | `ResolveDefense` uses `GetUnitStance(target)` from `_partyStances` | Wrong member's stance applied to target |
| 27 | Element effectiveness not applied | `CalculateRawDamage` calls `GetElementMultiplier` | All attacks deal neutral damage |
| 28 | Party member acts out of order or twice | `_activePartyIndex` for-loop in BattleLoop iterates once per living member | Turn order incorrect |

---

## 8. What NOT To Build

Do NOT implement any of these. Leave `// TODO` comments where noted in the spec:

- ❌ UI / Canvas / Text / Images / LineRenderers / Glow shaders
- ❌ Skill definitions, skill trees, or skill effect logic
- ❌ Item definitions, inventory, or item effect logic
- ❌ Status effects / buffs / debuffs system
- ❌ Audio / SFX / Music
- ❌ Animation triggers or Animator Controller references
- ❌ Save/Load
- ❌ XP / leveling / rewards
- ❌ Scene management or transitions
- ❌ Singleton or static instance pattern
- ❌ Unit tests
- ❌ MonoBehaviour implementations of IBattleUnit or IEnemyAI
- ❌ Unnecessary using statements (`UnityEngine.UI`, `TMPro`, `InputSystem`, etc.)
- ❌ `Update()`, `FixedUpdate()`, `LateUpdate()`, or `OnGUI()`

---

## 9. Final Validation Checklist

After writing all 4 files, verify **EVERY** item. If any item fails, fix it before finishing:

**File structure:**
- [ ] Exactly 4 files at the exact paths listed in Section 1
- [ ] All files use `namespace Battle { }` (braces, not file-scoped)
- [ ] No extra files were created

**IBattleUnit.cs:**
- [ ] Interface with exactly 14 members: UnitName, UnitType, Element, CurrentHP (get+set), MaxHP, AttackPower, Defense, IsAlive, DodgeBonus, HasShield, CurrentResource (get+set), MaxResource, ResourceLabel, TakeDamage(int)
- [ ] Only `using System;` at top
- [ ] XML comments on interface and on DodgeBonus, HasShield, TakeDamage

**IEnemyAI.cs:**
- [ ] Interface with `DecideAction(IReadOnlyList<IBattleUnit>)` returning `EnemyIntent`
- [ ] `EnemyIntent` is a `readonly struct` with `readonly` fields and constructor
- [ ] Only `using System.Collections.Generic;` at top

**BattleEnums.cs:**
- [ ] Five enums: `BattleState` (8 values), `PlayerAction` (6 values), `DefensiveStance` (3 values), `UnitType` (3 values), `ElementType` (7 values)
- [ ] Each starts with `None` as first value
- [ ] No using statements needed

**BattleController.cs — Structure:**
- [ ] Inherits `MonoBehaviour`
- [ ] Only these usings: `System`, `System.Collections`, `System.Collections.Generic`, `UnityEngine`
- [ ] No `Update()`, `FixedUpdate()`, `LateUpdate()`, or `OnGUI()` methods
- [ ] No `static` members
- [ ] No `public` fields — only `[SerializeField] private` fields + public properties/methods/events
- [ ] Uses `#region` blocks as labeled in each section

**BattleController.cs — State Management:**
- [ ] `SetState()` method exists and ALL state changes go through it
- [ ] `_currentState` is NEVER directly assigned outside `SetState()`
- [ ] `SetPlayerStance()` method exists, records stance in `_partyStances`, fires event
- [ ] `GetUnitStance()` method exists and is used by `ResolveDefense` for per-unit lookup
- [ ] `_playerStance` is NEVER directly assigned outside `SetPlayerStance()`
- [ ] `OnStateChanged` fires on every state transition with (previous, new)
- [ ] `OnDefensiveStanceChanged` fires on every stance change
- [ ] `ActiveUnit` property returns current party member or null outside player phase

**BattleController.cs — Configuration:**
- [ ] All timing values are `[SerializeField]` private floats with defaults
- [ ] All defensive values: block 0.99, shield 1.0, block damage 0.01, shield damage 0.0, dodge 0.75, cap 0.95
- [ ] `_maxActionPoints` defaults to 2
- [ ] Combatant lists use `List<MonoBehaviour>` for Inspector compatibility

**BattleController.cs — Data Structures:**
- [ ] `_declaredIntents` is `List<(IBattleUnit, EnemyIntent)>` NOT `Dictionary`
- [ ] `_enemyUnits` and `_enemyAIs` are parallel lists with validated matching counts
- [ ] `EnemyIntent` is `readonly struct` with `readonly` fields

**BattleController.cs — Action Point Rules:**
- [ ] `SelectAttack` — forces AP to 0, ends member's sub-turn ALWAYS, uses `ActiveUnit` for attacker
- [ ] `SelectSkill` — costs 1 AP, does NOT end turn unless AP hits 0
- [ ] `SelectItem` — costs 1 AP, does NOT end turn unless AP hits 0
- [ ] `SelectBlock` — sets Blocking stance via `SetPlayerStance`, sets AP to 0, ends turn
- [ ] `SelectDodge` — sets Dodging stance via `SetPlayerStance`, sets AP to 0, ends turn
- [ ] Block + Dodge in same turn rejected by `_playerStance != None` check

**BattleController.cs — Defensive Mechanics:**
- [ ] Block: 99% base → 100% with shield
- [ ] Block: reduced damage via `CeilToInt(rawDamage * multiplier)` → 0% with shield
- [ ] Block: bypassed by unblockable (fires `OnBlockBypassed`, NOT `OnBlockSuccess`)
- [ ] Dodge: 75% base + `DodgeBonus`, clamped to `_maxDodgeChance` (0.95)
- [ ] Dodge: CAN avoid unblockable attacks
- [ ] Resolution order: Dodge → Block → Full damage (NEVER reversed)
- [ ] Uses `UnityEngine.Random.Range(0f, 1f)` with `roll < chance` comparison
- [ ] Comment explaining `[0, 1)` range behavior is present

**BattleController.cs — Battle Flow:**
- [ ] Coroutine-based loop, no Update polling
- [ ] Party members act 1-by-1 in a `for` loop over `_playerParty`; dead members skipped
- [ ] Each member gets fresh AP; `SetPlayerStance(None)` at START of each member's sub-turn
- [ ] `_partyStances` accumulates each member's stance choice across the party round
- [ ] `_partyStances` cleared AFTER all enemy attacks complete (not before)
- [ ] `_activePartyIndex` set to `-1` after party loop completes
- [ ] `CheckBattleEnd()` called after EACH party member's sub-turn AND after enemy turn
- [ ] `_battleCoroutine = null` then `yield break` when `CheckBattleEnd` returns true
- [ ] Dead enemies skipped in `ExecuteEnemyAttacks`
- [ ] Dead targets retargeted in `ExecuteEnemyAttacks`
- [ ] `ExecuteEnemyAttacks` uses indexed `for` loop over `_declaredIntents` list
- [ ] `OnEnemyIntentDeclared` fires for each living enemy with 4 args
- [ ] `OnTurnEnd` fires at end of turn cycle with turn number
- [ ] All events use `?.Invoke()` null-conditional

**BattleController.cs — UI Query Helpers:**
- [ ] `CanPerformAction(PlayerAction)` exists and checks input, state, AP, stance
- [ ] `GetActionPointCost(PlayerAction)` exists and returns correct costs
- [ ] `GetAvailableActions()` exists and returns filtered list
- [ ] `MaxActionPoints` public property exists

**BattleController.cs — Element Effectiveness:**
- [ ] `GetElementMultiplier()` method exists with correct classical cycle and arcane duality
- [ ] `CalculateRawDamage` applies element multiplier before flooring to minimum 1
- [ ] `_elementStrongMultiplier` (1.5) and `_elementWeakMultiplier` (0.5) are `[SerializeField]`
- [ ] Ignis > Ventus > Terra > Aqua > Ignis cycle encoded correctly
- [ ] Lux ↔ Umbra mutual strong encoded correctly
- [ ] None element = always neutral (1.0x)
- [ ] Same element = neutral (1.0x)
- [ ] Cross-family (e.g. Ignis vs Lux) = neutral (1.0x)

**BattleController.cs — Safety:**
- [ ] `InitBattle` checks `gameObject.activeInHierarchy`
- [ ] `InitBattle` validates `_enemyUnits.Count == _enemyAIs.Count`
- [ ] `InitBattle` null-checks all MonoBehaviour entries
- [ ] `InitBattle` stops existing `_battleCoroutine` before starting new one
- [ ] `AbortBattle` uses `SetState` and `SetPlayerStance`, not direct assignment

**BattleController.cs — Documentation:**
- [ ] XML doc comments on EVERY public and protected method
- [ ] XML doc comments on EVERY public event
- [ ] `[Tooltip()]` on every `[SerializeField]`
- [ ] TODO comments for: skill system, item system, status effects

---

## 10. File Structure When Done

```
Assets/
└── Scripts/
    └── Battle/
        ├── Interfaces/
        │   ├── IBattleUnit.cs
        │   └── IEnemyAI.cs
        ├── BattleEnums.cs
        └── BattleController.cs
```

**Create these 4 files and nothing else. Follow this spec exactly.**
