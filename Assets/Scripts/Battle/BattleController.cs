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
        #region Configuration

        [Header("=== Timing ===")]
        [SerializeField, Tooltip("Seconds to display enemy intents before they act")]
        private float _intentDisplayDuration = 2.5f;

        [SerializeField, Tooltip("Delay before each enemy acts (announcement pause)")]
        private float _preEnemyAttackDelay = 0.6f;

        [SerializeField, Tooltip("Delay after each enemy's attack resolves")]
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

        [Header("=== Critical Hits ===")]
        [SerializeField, Tooltip("Damage multiplier when a critical hit lands")]
        private float _critDamageMultiplier = 1.5f;

        [Header("=== Retreat ===")]
        [SerializeField, Tooltip("Optional retreat handler for flee mechanics")]
        private RetreatHandler _retreatHandler;

        [Header("=== Combatants (assign in Inspector or set via InitBattle) ===")]
        [SerializeField, Tooltip("Player party units — assign MonoBehaviours that implement IBattleUnit")]
        private List<MonoBehaviour> _playerPartyRaw = new List<MonoBehaviour>();

        [SerializeField, Tooltip("Enemy units — assign MonoBehaviours that implement IBattleUnit AND IEnemyAI")]
        private List<MonoBehaviour> _enemyUnitsRaw = new List<MonoBehaviour>();

        #endregion

        #region Runtime State

        private BattleState _currentState = BattleState.None;
        private int _currentActionPoints;
        private DefensiveStance _playerStance = DefensiveStance.None;
        private bool _isPlayerInputEnabled;
        private int _currentTurnNumber;

        // Shared party inventory for consumable items
        private List<ItemSlot> _battleInventory = new List<ItemSlot>();

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

        // Status effects owned by the controller, keyed per unit
        private readonly Dictionary<IBattleUnit, List<StatusEffect>> _statusEffects =
            new Dictionary<IBattleUnit, List<StatusEffect>>();

        #endregion

        #region Events

        // --- Battle lifecycle ---
        /// <summary>Fired once at battle start after combatants are initialized.</summary>
        public event Action OnBattleStart;

        /// <summary>Fired when the player wins (all enemies dead).</summary>
        public event Action OnBattleWon;

        /// <summary>Fired when the player loses (all party members dead).</summary>
        public event Action OnBattleLost;

        /// <summary>Fired when the player retreats/flees from battle.</summary>
        public event Action OnBattleRetreated;

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
        /// Args: (enemy unit, target unit, is attack unblockable, estimated damage, skill name or null).
        /// </summary>
        public event Action<IBattleUnit, IBattleUnit, bool, int, string> OnEnemyIntentDeclared;

        /// <summary>Fired when intent display period ends. UI should remove targeting lines and glows.</summary>
        public event Action OnEnemyIntentsCleared;

        /// <summary>Fired when enemy execution phase begins.</summary>
        public event Action OnEnemyTurnStart;

        /// <summary>
        /// Fired before each individual enemy acts during the execution phase.
        /// Args: (attacking enemy, target unit).
        /// UI can use this to announce/highlight the acting enemy.
        /// </summary>
        public event Action<IBattleUnit, IBattleUnit> OnEnemyActionStart;

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

        // --- Skill/Item feedback ---
        /// <summary>Fired when a unit is healed. Args: (healed unit, heal amount).</summary>
        public event Action<IBattleUnit, int> OnHealApplied;

        /// <summary>Fired when a unit's resource is restored. Args: (unit, amount restored).</summary>
        public event Action<IBattleUnit, int> OnResourceRestored;

        // --- Status Effects ---
        /// <summary>Fired when a status effect is applied. Args: (unit, type, duration, value).</summary>
        public event Action<IBattleUnit, StatusEffectType, int, float> OnStatusEffectApplied;

        /// <summary>Fired when a DoT effect ticks. Args: (unit, type, damage).</summary>
        public event Action<IBattleUnit, StatusEffectType, int> OnStatusEffectTick;

        /// <summary>Fired when a status effect expires. Args: (unit, type).</summary>
        public event Action<IBattleUnit, StatusEffectType> OnStatusEffectExpired;

        /// <summary>Fired when a stunned unit's turn is skipped. Args: (unit).</summary>
        public event Action<IBattleUnit> OnStunSkipped;

        // --- Critical Hits ---
        /// <summary>Fired when a critical hit lands. Args: (attacker, target).</summary>
        public event Action<IBattleUnit, IBattleUnit> OnCriticalHit;

        // --- Revive ---
        /// <summary>Fired when a unit is revived. Args: (unit, hp restored to).</summary>
        public event Action<IBattleUnit, int> OnUnitRevived;

        // --- Scan ---
        /// <summary>Fired when a scan completes. Args: (scanner, scanned target).</summary>
        public event Action<IBattleUnit, IBattleUnit> OnScanCompleted;

        // --- B1 AI Pattern Events ---
        /// <summary>Fired when a Saboteur skill burns a target's resource. Args: (target, amount burned).</summary>
        public event Action<IBattleUnit, int> OnResourceBurned;

        /// <summary>Fired when a Saboteur skill destroys a party item. Args: (item name).</summary>
        public event Action<string> OnItemDestroyed;

        /// <summary>Fired when an attack is redirected by a Bodyguard. Args: (attacker, originalTarget, newTarget).</summary>
        public event Action<IBattleUnit, IBattleUnit, IBattleUnit> OnAttackRedirected;

        #endregion

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

        /// <summary>The shared party inventory of consumable items.</summary>
        public IReadOnlyList<ItemSlot> BattleInventory => _battleInventory;

        /// <summary>Returns the skill list for the currently active party member's UnitType.</summary>
        public List<SkillDefinition> ActiveUnitSkills =>
            ActiveUnit != null ? SkillAndItemData.GetSkills(ActiveUnit.UnitType) : new List<SkillDefinition>();

        /// <summary>Last skill used by a player (for Mimic AI pattern).</summary>
        public SkillDefinition LastPlayerSkill { get; private set; }

        /// <summary>Element of last player attack/skill (for Elemental Shifter AI pattern).</summary>
        public ElementType LastPlayerAttackElement { get; private set; }

        /// <summary>
        /// Optional redirect handler for Bodyguard AI pattern.
        /// When set, called during attack/skill resolution to redirect attacks.
        /// Args: (attacker, originalTarget) → redirected target (or originalTarget if no redirect).
        /// </summary>
        public Func<IBattleUnit, IBattleUnit, IBattleUnit> AttackRedirectHandler { get; set; }

        #endregion

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
            _statusEffects.Clear();
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
            LastPlayerSkill = null;
            LastPlayerAttackElement = ElementType.None;
            AttackRedirectHandler = null;

            // --- Stop any existing battle coroutine ---
            if (_battleCoroutine != null)
            {
                StopCoroutine(_battleCoroutine);
                _battleCoroutine = null;
            }

            // Initialize shared party inventory
            _battleInventory = SkillAndItemData.GetStartingInventory();

            SetState(BattleState.None);
            _battleCoroutine = StartCoroutine(BattleLoop());
        }

        #endregion

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
                OnTurnStart?.Invoke(_currentTurnNumber);

                // --- ENEMY INTENT PHASE (shown BEFORE player acts so they can react) ---
                SetState(BattleState.EnemyIntent);
                _declaredIntents.Clear();
                OnEnemyIntentPhaseStart?.Invoke();

                DeclareAllEnemyIntents();

                // Hold so the player can read the intents / targeting lines / red glows
                yield return new WaitForSeconds(_intentDisplayDuration);

                OnEnemyIntentsCleared?.Invoke();

                // --- PLAYER PHASE: each living party member acts in order ---
                SetState(BattleState.PlayerTurn);
                _partyStances.Clear();

                for (_activePartyIndex = 0; _activePartyIndex < _playerParty.Count; _activePartyIndex++)
                {
                    IBattleUnit member = _playerParty[_activePartyIndex];

                    // Skip dead party members
                    if (!member.IsAlive) continue;

                    // Skip stunned party members
                    if (IsStunned(member))
                    {
                        OnStunSkipped?.Invoke(member);
                        continue;
                    }

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

                // --- ENEMY TURN (execute pre-declared intents) ---
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

                // Tick status effects (DoT damage, duration countdown, expiry)
                TickStatusEffects();

                // Check for DoT kills
                if (CheckBattleEnd())
                {
                    _battleCoroutine = null;
                    yield break;
                }

                OnTurnEnd?.Invoke(_currentTurnNumber);
                yield return new WaitForSeconds(_postActionDelay);
            }
        }

        #endregion

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
                EnemyIntent intent = ai.DecideAction(livingPlayers.AsReadOnly(), _enemyUnits.AsReadOnly());

                // Safety: if AI returned a dead/null target, default to first living player
                IBattleUnit validTarget = intent.Target;
                if (validTarget == null || !validTarget.IsAlive)
                {
                    Debug.LogWarning($"[BattleController] Enemy '{enemy.UnitName}' AI returned invalid target. Defaulting to first living player.");
                    validTarget = livingPlayers[0];
                    intent = new EnemyIntent(validTarget, intent.IsUnblockable, intent.EstimatedDamage, intent.Skill);
                }

                _declaredIntents.Add((enemy, intent));

                string skillName = intent.Skill != null ? intent.Skill.Name : null;
                OnEnemyIntentDeclared?.Invoke(enemy, validTarget, intent.IsUnblockable, intent.EstimatedDamage, skillName);
            }
        }

        #endregion

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

                // Skip stunned enemies
                if (IsStunned(attacker))
                {
                    OnStunSkipped?.Invoke(attacker);
                    yield return new WaitForSeconds(_delayBetweenEnemyAttacks);
                    continue;
                }

                // Branch: skill or basic attack
                if (intent.Skill != null)
                {
                    SkillDefinition skill = intent.Skill;

                    // For ally-targeting skills (heal/buff), if target died skip instead of retargeting to player
                    if (skill.TargetType == SkillTargetType.Ally)
                    {
                        if (!target.IsAlive)
                        {
                            // Target ally died — skip this action
                            yield return new WaitForSeconds(_delayBetweenEnemyAttacks);
                            continue;
                        }
                    }
                    else
                    {
                        // Retarget offensive skills if original target died
                        if (!target.IsAlive)
                        {
                            IBattleUnit newTarget = GetFirstLivingUnit(_playerParty);
                            if (newTarget == null) yield break;
                            target = newTarget;
                        }
                    }

                    OnEnemyActionStart?.Invoke(attacker, target);
                    yield return new WaitForSeconds(_preEnemyAttackDelay);

                    ExecuteSkillEffect(skill, attacker, target);
                }
                else
                {
                    // Basic attack — retarget if original target died
                    if (!target.IsAlive)
                    {
                        IBattleUnit newTarget = GetFirstLivingUnit(_playerParty);
                        if (newTarget == null) yield break;
                        target = newTarget;
                    }

                    OnEnemyActionStart?.Invoke(attacker, target);
                    yield return new WaitForSeconds(_preEnemyAttackDelay);

                    int rawDamage = CalculateRawDamage(attacker, target);
                    rawDamage = ApplyCriticalHit(rawDamage, attacker, target);
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
                }

                yield return new WaitForSeconds(_delayBetweenEnemyAttacks);
            }
        }

        #endregion

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

        #region Damage Calculation

        [Header("=== Element Effectiveness ===")]
        [SerializeField, Tooltip("Damage multiplier when attacker's element is strong vs target's element")]
        private float _elementStrongMultiplier = 1.5f;

        [SerializeField, Tooltip("Damage multiplier when attacker's element is weak vs target's element")]
        private float _elementWeakMultiplier = 0.5f;

        /// <summary>
        /// Calculates base damage before defensive modifiers, including element effectiveness.
        /// Formula: (effectiveAttack - effectiveDefense) * elementMultiplier, minimum 1.
        /// Guaranteed to return at least 1 so no attack is completely wasted.
        /// </summary>
        private int CalculateRawDamage(IBattleUnit attacker, IBattleUnit target)
        {
            int effectiveAtk = Mathf.RoundToInt(GetEffectiveAttack(attacker));
            int effectiveDef = Mathf.RoundToInt(GetEffectiveDefense(target));
            int baseDamage = effectiveAtk - effectiveDef;
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

        /// <summary>
        /// Rolls for critical hit and applies multiplier if successful.
        /// </summary>
        private int ApplyCriticalHit(int damage, IBattleUnit attacker, IBattleUnit target)
        {
            if (UnityEngine.Random.value < attacker.CritChance)
            {
                OnCriticalHit?.Invoke(attacker, target);
                return Mathf.RoundToInt(damage * _critDamageMultiplier);
            }
            return damage;
        }

        #endregion

        #region Status Effects

        /// <summary>
        /// Applies a status effect to a unit. Stacks are replaced if same type exists.
        /// </summary>
        public void ApplyStatusEffect(IBattleUnit unit, StatusEffectType type, int duration, float value)
        {
            if (type == StatusEffectType.None) return;

            if (!_statusEffects.ContainsKey(unit))
                _statusEffects[unit] = new List<StatusEffect>();

            // Replace existing effect of same type (refresh duration)
            var list = _statusEffects[unit];
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].Type == type)
                    list.RemoveAt(i);
            }

            list.Add(new StatusEffect(type, duration, value));
            OnStatusEffectApplied?.Invoke(unit, type, duration, value);
        }

        /// <summary>
        /// Removes all instances of a specific status effect type from a unit.
        /// </summary>
        public void CureStatusEffect(IBattleUnit unit, StatusEffectType type)
        {
            if (!_statusEffects.ContainsKey(unit)) return;
            var list = _statusEffects[unit];
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].Type == type)
                {
                    list.RemoveAt(i);
                    OnStatusEffectExpired?.Invoke(unit, type);
                }
            }
        }

        /// <summary>
        /// Returns true if the unit has a Stun effect active.
        /// </summary>
        public bool IsStunned(IBattleUnit unit)
        {
            if (!_statusEffects.TryGetValue(unit, out var list)) return false;
            foreach (var eff in list)
            {
                if (eff.Type == StatusEffectType.Stun && eff.RemainingTurns > 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns effective attack power after buff/debuff modifiers.
        /// </summary>
        public float GetEffectiveAttack(IBattleUnit unit)
        {
            float atk = unit.AttackPower;
            if (!_statusEffects.TryGetValue(unit, out var list)) return atk;
            foreach (var eff in list)
            {
                if (eff.Type == StatusEffectType.AtkUp || eff.Type == StatusEffectType.AtkDown)
                    atk *= eff.Value;
            }
            return atk;
        }

        /// <summary>
        /// Returns effective defense after buff/debuff modifiers.
        /// </summary>
        public float GetEffectiveDefense(IBattleUnit unit)
        {
            float def = unit.Defense;
            if (!_statusEffects.TryGetValue(unit, out var list)) return def;
            foreach (var eff in list)
            {
                if (eff.Type == StatusEffectType.DefUp || eff.Type == StatusEffectType.DefDown)
                    def *= eff.Value;
            }
            return def;
        }

        /// <summary>
        /// Ticks all status effects: applies DoT damage, decrements durations, removes expired.
        /// Called at TurnEnd.
        /// </summary>
        private void TickStatusEffects()
        {
            // Iterate over a snapshot of keys to avoid modification during iteration
            var units = new List<IBattleUnit>(_statusEffects.Keys);
            foreach (var unit in units)
            {
                if (!unit.IsAlive) continue;
                if (!_statusEffects.TryGetValue(unit, out var list)) continue;

                for (int i = list.Count - 1; i >= 0; i--)
                {
                    StatusEffect eff = list[i];

                    // Tick DoT damage
                    if (eff.Type == StatusEffectType.Poison || eff.Type == StatusEffectType.Burn)
                    {
                        int dotDamage = Mathf.Max(1, Mathf.RoundToInt(eff.Value));
                        unit.TakeDamage(dotDamage);
                        OnStatusEffectTick?.Invoke(unit, eff.Type, dotDamage);

                        if (!unit.IsAlive)
                        {
                            OnUnitDied?.Invoke(unit, unit); // self-inflicted DoT kill
                            list.Clear();
                            break;
                        }
                    }

                    // Decrement duration
                    eff.RemainingTurns--;
                    if (eff.RemainingTurns <= 0)
                    {
                        StatusEffectType expiredType = eff.Type;
                        list.RemoveAt(i);
                        OnStatusEffectExpired?.Invoke(unit, expiredType);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the list of status effects on a unit. For UI display.
        /// </summary>
        public IReadOnlyList<StatusEffect> GetStatusEffects(IBattleUnit unit)
        {
            if (_statusEffects.TryGetValue(unit, out var list))
                return list.AsReadOnly();
            return new List<StatusEffect>().AsReadOnly();
        }

        #endregion

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

            // Track element for Elemental Shifter AI
            LastPlayerAttackElement = attacker.Element;

            // Bodyguard redirect check
            if (AttackRedirectHandler != null)
            {
                IBattleUnit originalTarget = target;
                IBattleUnit redirected = AttackRedirectHandler(attacker, target);
                if (redirected != null && redirected.IsAlive && redirected != target)
                {
                    target = redirected;
                    OnAttackRedirected?.Invoke(attacker, originalTarget, target);
                }
            }

            int rawDamage = CalculateRawDamage(attacker, target);
            rawDamage = ApplyCriticalHit(rawDamage, attacker, target);
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

            IBattleUnit caster = ActiveUnit;
            if (caster == null || !caster.IsAlive) return;

            var skills = SkillAndItemData.GetSkills(caster.UnitType);
            if (skillIndex < 0 || skillIndex >= skills.Count)
            {
                Debug.LogWarning($"[BattleController] Invalid skill index {skillIndex}.");
                return;
            }

            SkillDefinition skill = skills[skillIndex];

            // Validate resource cost
            if (skill.ResourceCost > 0 && caster.CurrentResource < skill.ResourceCost)
            {
                Debug.LogWarning($"[BattleController] Not enough resource for {skill.Name} (need {skill.ResourceCost}, have {caster.CurrentResource}).");
                return;
            }

            // Track for Mimic and Elemental Shifter AI patterns
            LastPlayerSkill = skill;
            LastPlayerAttackElement = skill.Element;

            // Resolve target for self-targeting skills
            IBattleUnit resolvedTarget = ResolveSkillTarget(skill, caster, target);

            // Bodyguard redirect check (only for enemy-targeting skills)
            if (skill.TargetType == SkillTargetType.Enemy && AttackRedirectHandler != null)
            {
                IBattleUnit originalTarget = resolvedTarget;
                IBattleUnit redirected = AttackRedirectHandler(caster, resolvedTarget);
                if (redirected != null && redirected.IsAlive && redirected != resolvedTarget)
                {
                    resolvedTarget = redirected;
                    OnAttackRedirected?.Invoke(caster, originalTarget, resolvedTarget);
                }
            }

            OnPlayerActionSelected?.Invoke(PlayerAction.Skill, resolvedTarget);

            // Deduct resource
            if (skill.ResourceCost > 0)
            {
                caster.CurrentResource -= skill.ResourceCost;
            }

            ExecuteSkillEffect(skill, caster, resolvedTarget);
            ConsumeActionPoints(1);
        }

        /// <summary>
        /// Player uses an item. Costs 1 AP. Does NOT end the turn.
        /// Player can still take another action if AP remains.
        /// </summary>
        public void SelectItem(int itemIndex, IBattleUnit target)
        {
            if (!ValidateInput(PlayerAction.Item, 1)) return;

            if (itemIndex < 0 || itemIndex >= _battleInventory.Count)
            {
                Debug.LogWarning($"[BattleController] Invalid item index {itemIndex}.");
                return;
            }

            ItemSlot slot = _battleInventory[itemIndex];
            if (slot.Quantity <= 0)
            {
                Debug.LogWarning($"[BattleController] No {slot.Item.Name} remaining.");
                return;
            }

            IBattleUnit user = ActiveUnit;
            if (user == null || !user.IsAlive) return;

            // Resolve target
            IBattleUnit resolvedTarget = ResolveItemTarget(slot.Item, user, target);

            OnPlayerActionSelected?.Invoke(PlayerAction.Item, resolvedTarget);

            // Consume item
            slot.Quantity--;

            ExecuteItemEffect(slot.Item, user, resolvedTarget);
            ConsumeActionPoints(1);
        }

        /// <summary>
        /// Player scans an enemy target, recording its data in the bestiary.
        /// Costs 1 AP. Does NOT end the turn.
        /// </summary>
        public void SelectScan(IBattleUnit target)
        {
            if (!ValidateInput(PlayerAction.Scan, 1)) return;
            if (target == null || !target.IsAlive)
            {
                Debug.LogWarning("[BattleController] SelectScan: invalid or dead target.");
                return;
            }

            OnPlayerActionSelected?.Invoke(PlayerAction.Scan, target);
            BestiaryData.RecordScan(target);
            OnScanCompleted?.Invoke(ActiveUnit, target);
            ConsumeActionPoints(1);
        }

        /// <summary>
        /// Player attempts to flee from battle.
        /// On success: battle aborts, OnBattleRetreated fires.
        /// On failure: all remaining AP consumed, turn ends.
        /// </summary>
        public void SelectFlee()
        {
            if (!ValidateInput(PlayerAction.Flee, 1)) return;

            OnPlayerActionSelected?.Invoke(PlayerAction.Flee, null);

            if (_retreatHandler == null)
            {
                Debug.LogWarning("[BattleController] SelectFlee: no RetreatHandler assigned.");
                return;
            }

            bool succeeded = _retreatHandler.TryRetreat();

            if (!succeeded)
            {
                // Failed flee costs all remaining AP
                _currentActionPoints = 0;
                OnActionPointsChanged?.Invoke(0);
                EndPlayerInput();
            }
        }

        /// <summary>
        /// Fires the OnBattleRetreated event. Called by RetreatHandler after successful retreat.
        /// </summary>
        public void NotifyRetreat()
        {
            OnBattleRetreated?.Invoke();
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

            // Flee requires a retreat handler that permits retreat
            if (action == PlayerAction.Flee)
            {
                if (_retreatHandler == null || !_retreatHandler.CanRetreat) return false;
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
                case PlayerAction.Scan:   return 1;
                case PlayerAction.Flee:  return _currentActionPoints; // All remaining
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

        /// <summary>
        /// Returns true if any player party member is dead (for revive item availability).
        /// </summary>
        public bool HasDeadAlly()
        {
            foreach (var unit in _playerParty)
            {
                if (!unit.IsAlive) return true;
            }
            return false;
        }

        #endregion

        #region Skill & Item Execution

        private void ExecuteSkillEffect(SkillDefinition skill, IBattleUnit caster, IBattleUnit target)
        {
            // Heal
            if (skill.HealAmount > 0 && target != null && target.IsAlive)
            {
                int before = target.CurrentHP;
                target.CurrentHP = Mathf.Min(target.CurrentHP + skill.HealAmount, target.MaxHP);
                int healed = target.CurrentHP - before;
                if (healed > 0)
                    OnHealApplied?.Invoke(target, healed);
            }

            // Damage — AoE or single target
            if (skill.DamageMultiplier > 0f)
            {
                if (skill.IsAoE)
                {
                    // AoE: damage all living opposing units
                    List<IBattleUnit> opposingUnits;
                    if (_playerParty.Contains(caster))
                        opposingUnits = GetLivingUnits(_enemyUnits);
                    else
                        opposingUnits = GetLivingUnits(_playerParty);

                    foreach (var aoeTarget in opposingUnits)
                    {
                        ApplySkillDamage(skill, caster, aoeTarget);
                    }
                }
                else if (target != null && target.IsAlive)
                {
                    ApplySkillDamage(skill, caster, target);
                }
            }

            // Resource burn (Saboteur)
            if (skill.ResourceBurnAmount > 0 && target != null && target.IsAlive && target.MaxResource > 0)
            {
                int burnAmount = Mathf.Min(skill.ResourceBurnAmount, target.CurrentResource);
                if (burnAmount > 0)
                {
                    target.CurrentResource -= burnAmount;
                    OnResourceBurned?.Invoke(target, burnAmount);
                }
            }

            // Item destroy (Saboteur)
            if (skill.DestroysItem && _battleInventory.Count > 0)
            {
                // Find non-empty slots and destroy a random one
                var nonEmpty = new List<int>();
                for (int i = 0; i < _battleInventory.Count; i++)
                {
                    if (_battleInventory[i].Quantity > 0)
                        nonEmpty.Add(i);
                }
                if (nonEmpty.Count > 0)
                {
                    int idx = nonEmpty[UnityEngine.Random.Range(0, nonEmpty.Count)];
                    string itemName = _battleInventory[idx].Item.Name;
                    _battleInventory[idx].Quantity--;
                    OnItemDestroyed?.Invoke(itemName);
                }
            }

            // Apply status effect
            if (skill.AppliesEffect != StatusEffectType.None && target != null && target.IsAlive)
            {
                if (UnityEngine.Random.value < skill.EffectChance)
                {
                    ApplyStatusEffect(target, skill.AppliesEffect, skill.EffectDuration, skill.EffectValue);
                }
            }
        }

        /// <summary>
        /// Applies damage from a skill to a single target (extracted for AoE reuse).
        /// </summary>
        private void ApplySkillDamage(SkillDefinition skill, IBattleUnit caster, IBattleUnit target)
        {
            int effectiveAtk = Mathf.RoundToInt(GetEffectiveAttack(caster));
            int effectiveDef = Mathf.RoundToInt(GetEffectiveDefense(target));
            int baseDamage = Mathf.Max(0, effectiveAtk - effectiveDef);
            int rawDamage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * skill.DamageMultiplier));

            float elemMult = GetElementMultiplier(skill.Element, target.Element);
            rawDamage = Mathf.Max(1, Mathf.RoundToInt(rawDamage * elemMult));

            rawDamage = ApplyCriticalHit(rawDamage, caster, target);

            OnAttackExecuted?.Invoke(caster, target, rawDamage);
            target.TakeDamage(rawDamage);
            OnDamageTaken?.Invoke(target, caster, rawDamage);

            // Lifesteal
            if (skill.LifestealRatio > 0f && caster.IsAlive)
            {
                int healAmt = Mathf.Max(1, Mathf.RoundToInt(rawDamage * skill.LifestealRatio));
                int before = caster.CurrentHP;
                caster.CurrentHP = Mathf.Min(caster.CurrentHP + healAmt, caster.MaxHP);
                int healed = caster.CurrentHP - before;
                if (healed > 0)
                    OnHealApplied?.Invoke(caster, healed);
            }

            if (!target.IsAlive)
                OnUnitDied?.Invoke(target, caster);
        }

        private void ExecuteItemEffect(ItemDefinition item, IBattleUnit user, IBattleUnit target)
        {
            // Escape item — guaranteed retreat
            if (item.IsEscapeItem && _retreatHandler != null)
            {
                _retreatHandler.TryRetreat(true);
                return;
            }

            // Revive
            if (item.IsRevive && target != null && !target.IsAlive)
            {
                target.Revive(item.HealHP);
                OnUnitRevived?.Invoke(target, target.CurrentHP);
                return; // Revive is the only effect
            }

            // Cure status effect
            if (item.CuresEffect != StatusEffectType.None && target != null && target.IsAlive)
            {
                CureStatusEffect(target, item.CuresEffect);
            }

            // Heal HP
            if (item.HealHP > 0 && target != null && target.IsAlive)
            {
                int before = target.CurrentHP;
                target.CurrentHP = Mathf.Min(target.CurrentHP + item.HealHP, target.MaxHP);
                int healed = target.CurrentHP - before;
                if (healed > 0)
                    OnHealApplied?.Invoke(target, healed);
            }

            // Restore resource
            if (item.RestoreResource > 0 && target != null && target.IsAlive && target.MaxResource > 0)
            {
                int before = target.CurrentResource;
                target.CurrentResource = Mathf.Min(target.CurrentResource + item.RestoreResource, target.MaxResource);
                int restored = target.CurrentResource - before;
                if (restored > 0)
                    OnResourceRestored?.Invoke(target, restored);
            }

            // Damage
            if (item.Damage > 0 && target != null && target.IsAlive)
            {
                int rawDamage = item.Damage;

                // Apply element effectiveness
                float elemMult = GetElementMultiplier(item.DamageElement, target.Element);
                rawDamage = Mathf.Max(1, Mathf.RoundToInt(rawDamage * elemMult));

                // Apply critical hit
                rawDamage = ApplyCriticalHit(rawDamage, user, target);

                OnAttackExecuted?.Invoke(user, target, rawDamage);
                target.TakeDamage(rawDamage);
                OnDamageTaken?.Invoke(target, user, rawDamage);

                if (!target.IsAlive)
                    OnUnitDied?.Invoke(target, user);
            }
        }

        private IBattleUnit ResolveSkillTarget(SkillDefinition skill, IBattleUnit caster, IBattleUnit providedTarget)
        {
            switch (skill.TargetType)
            {
                case SkillTargetType.Self:
                    return caster;
                case SkillTargetType.Ally:
                    return providedTarget != null && providedTarget.IsAlive ? providedTarget : GetLowestHPAlly();
                case SkillTargetType.Enemy:
                default:
                    return providedTarget;
            }
        }

        private IBattleUnit ResolveItemTarget(ItemDefinition item, IBattleUnit user, IBattleUnit providedTarget)
        {
            // Revive items auto-target first dead ally
            if (item.IsRevive)
            {
                return providedTarget ?? GetFirstDeadAlly();
            }

            switch (item.TargetType)
            {
                case SkillTargetType.Self:
                    return user;
                case SkillTargetType.Ally:
                    return providedTarget != null && providedTarget.IsAlive ? providedTarget : GetLowestHPAlly();
                case SkillTargetType.Enemy:
                default:
                    return providedTarget;
            }
        }

        private IBattleUnit GetLowestHPAlly()
        {
            IBattleUnit lowest = null;
            foreach (var unit in _playerParty)
            {
                if (!unit.IsAlive) continue;
                if (lowest == null || unit.CurrentHP < lowest.CurrentHP)
                    lowest = unit;
            }
            return lowest;
        }

        private IBattleUnit GetFirstDeadAlly()
        {
            foreach (var unit in _playerParty)
            {
                if (!unit.IsAlive) return unit;
            }
            return null;
        }

        /// <summary>
        /// Returns true if the active unit has enough resource to use the given skill.
        /// UI uses this to gray out unaffordable skills.
        /// </summary>
        public bool CanAffordSkill(int skillIndex)
        {
            IBattleUnit active = ActiveUnit;
            if (active == null) return false;
            var skills = SkillAndItemData.GetSkills(active.UnitType);
            if (skillIndex < 0 || skillIndex >= skills.Count) return false;
            return active.CurrentResource >= skills[skillIndex].ResourceCost;
        }

        #endregion

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
            _statusEffects.Clear();
            OnPlayerInputStateChanged?.Invoke(false);
            SetState(BattleState.None);
        }

        #endregion
    }
}
