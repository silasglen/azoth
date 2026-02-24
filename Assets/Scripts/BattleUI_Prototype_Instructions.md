# BattleUI Prototype — Claude Code Agent Build Spec

> **Agent directive:** This is a simple prototype UI for testing `BattleController.cs`. Read the BattleController spec first if you haven't. This UI must be functional but ugly — we are testing logic, not shipping a product. Prioritize working buttons and readable text over visual polish.

---

## 0. Pre-Flight

1. The **BattleController** and its dependencies (IBattleUnit, IEnemyAI, BattleEnums) must already exist in `Assets/Scripts/Battle/`. If they don't, build them first using the BattleController spec.
2. You will create exactly **4 files** (listed in Section 1).
3. All new files use namespace `Battle` (UI files use `Battle.UI`).
4. Target: Unity 2022.3+ / C# 9. Uses Unity UI (uGUI), NOT UI Toolkit.
5. After writing all files, verify zero syntax errors.

---

## 1. Files To Create

| # | Path | Purpose |
|---|------|---------|
| 1 | `Assets/Scripts/Battle/TestBattleUnit.cs` | Simple IBattleUnit + IEnemyAI implementation for testing |
| 2 | `Assets/Scripts/Battle/UI/BattleUIController.cs` | Subscribes to BattleController events, updates UI |
| 3 | `Assets/Scripts/Battle/UI/EnemyButtonHandler.cs` | Clickable enemy target — attached to each enemy's UI element |
| 4 | `Assets/Scripts/Battle/BattleSceneBootstrap.cs` | Wires everything together and starts the battle |

---

## 2. File #1 — TestBattleUnit.cs

A single MonoBehaviour that implements both `IBattleUnit` and `IEnemyAI`. This lets one script work for both player and enemy units during testing.

Each unit has a `UnitType` that determines which skill categories and resource system it uses:
- **Alchemist** — uses Alchemy skills with classical elements: Ignis (fire), Ventus (wind), Terra (earth), Aqua (water). Skills cost **Charges** — a limited resource determined by the Catalyst equipped. Different Catalysts grant different max charges.
- **Magus** — uses Spells with arcane elements: Lux (light), Umbra (dark). Spells cost **MP** (Mana Points) — a regenerating resource pool.
- **Knight** — uses Melee skills (no element). Knights are mercenaries employed by the Magus faction. Melee skills have no resource cost (cooldown-based, future system).

**Skill resource systems:**
- **Charges (Alchemist):** Finite per battle. Set by equipped Catalyst (e.g. "Flask of Ignis" = 5 charges). When charges hit 0, the Alchemist cannot use skills until resupplied.
- **MP (Magus):** Pool with max value. Spells cost MP. MP may regenerate per turn (future system). When MP is insufficient, the spell cannot be cast.

**Element weakness chain:**
- Classical cycle (Alchemy): Ignis > Ventus > Terra > Aqua > Ignis
- Arcane duality (Magus): Lux ↔ Umbra (mutually effective and weak against each other)
- Melee: element-neutral — no weakness/resistance interactions

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    // NOTE: UnitType and ElementType enums are defined in BattleEnums.cs.
    // Do NOT duplicate them here — just use them via the Battle namespace.

    /// <summary>
    /// Barebones test combatant. Implements both IBattleUnit and IEnemyAI
    /// so it can be used as either a player unit or enemy in the Inspector.
    /// </summary>
    public class TestBattleUnit : MonoBehaviour, IBattleUnit, IEnemyAI
    {
        [Header("=== Identity ===")]
        [SerializeField] private string _unitName = "Unit";
        [SerializeField] private UnitType _unitType = UnitType.Alchemist;
        [SerializeField] private ElementType _element = ElementType.None;

        [Header("=== Stats ===")]
        [SerializeField] private int _maxHP = 100;
        [SerializeField] private int _attackPower = 20;
        [SerializeField] private int _defense = 5;

        [Header("=== Skill Resource (Magus: MP, Alchemist: Charges) ===")]
        [SerializeField] private int _maxResource = 0;
        [SerializeField] private string _catalystName = "";

        [Header("=== Gear ===")]
        [SerializeField] private float _dodgeBonus = 0f;
        [SerializeField] private bool _hasShield = false;

        [Header("=== Enemy AI Settings ===")]
        [SerializeField, Range(0f, 1f)]
        private float _unblockableChance = 0.2f;

        private int _currentHP;
        private int _currentResource;
        private bool _isAlive = true;

        // --- IBattleUnit ---
        public string UnitName => _unitName;
        public UnitType UnitType => _unitType;
        public ElementType Element => _element;
        public int CurrentHP
        {
            get => _currentHP;
            set => _currentHP = value;
        }
        public int MaxHP => _maxHP;
        public int AttackPower => _attackPower;
        public int Defense => _defense;
        public bool IsAlive => _isAlive;
        public float DodgeBonus => _dodgeBonus;
        public bool HasShield => _hasShield;

        /// <summary>
        /// Current skill resource: MP for Magus, Charges for Alchemist, 0 for Knight.
        /// </summary>
        public int CurrentResource
        {
            get => _currentResource;
            set => _currentResource = value;
        }
        public int MaxResource => _maxResource;

        /// <summary>
        /// Display label for the resource bar: "MP" for Magus, Catalyst name for Alchemist, empty for Knight.
        /// </summary>
        public string ResourceLabel => _unitType switch
        {
            UnitType.Magus => "MP",
            UnitType.Alchemist => string.IsNullOrEmpty(_catalystName) ? "Charges" : _catalystName,
            _ => ""
        };

        private void Awake()
        {
            _currentHP = _maxHP;
            _currentResource = _maxResource;
        }

        public void TakeDamage(int amount)
        {
            _currentHP = Mathf.Max(0, _currentHP - amount);
            if (_currentHP <= 0)
            {
                _isAlive = false;
            }
        }

        // --- IEnemyAI ---
        public EnemyIntent DecideAction(IReadOnlyList<IBattleUnit> playerParty)
        {
            // Simple AI: pick random living target, random chance of unblockable
            IBattleUnit target = playerParty[Random.Range(0, playerParty.Count)];
            bool unblockable = Random.value < _unblockableChance;
            int estimatedDamage = Mathf.Max(1, _attackPower - target.Defense);
            return new EnemyIntent(target, unblockable, estimatedDamage);
        }
    }
}
```

---

## 3. File #2 — BattleUIController.cs

This is the main UI script. It subscribes to BattleController events and updates Unity UI elements.

**Design rules:**
- All UI references are `[SerializeField]` — assigned in Inspector.
- Uses `UnityEngine.UI` (Button, ScrollRect, Image) + `TMPro` (TMP_Text) for all text display.
- The log area is a scrolling TMP_Text field that appends combat messages.
- Action buttons are enabled/disabled based on `CanPerformAction()`.

```csharp
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Battle;

namespace Battle.UI
{
    /// <summary>
    /// Prototype battle UI. Subscribes to BattleController events and
    /// updates basic Unity UI elements. Attach to a Canvas GameObject.
    /// The player party acts one unit at a time — the UI shows whose turn
    /// it is and cycles through each living party member in order.
    /// </summary>
    public class BattleUIController : MonoBehaviour
    {
        [Header("=== Controller Reference ===")]
        [SerializeField] private BattleController _battleController;

        [Header("=== Info Display ===")]
        [SerializeField] private TMP_Text _turnText;
        [SerializeField] private TMP_Text _stateText;
        [SerializeField] private TMP_Text _apText;
        [SerializeField] private TMP_Text _stanceText;

        [Header("=== Active Unit Indicator ===")]
        [SerializeField] private TMP_Text _activeUnitText;

        [Header("=== Player Party HP (one Text per party member, assign in order) ===")]
        [SerializeField] private TMP_Text[] _playerHPTexts;

        [Header("=== Enemy HP (one Text per enemy, assign in order) ===")]
        [SerializeField] private TMP_Text[] _enemyHPTexts;

        [Header("=== Action Buttons ===")]
        [SerializeField] private Button _attackButton;
        [SerializeField] private Button _skillButton;
        [SerializeField] private Button _itemButton;
        [SerializeField] private Button _blockButton;
        [SerializeField] private Button _dodgeButton;

        [Header("=== Combat Log ===")]
        [SerializeField] private TMP_Text _logText;
        [SerializeField] private ScrollRect _logScrollRect;

        [Header("=== Enemy Target Buttons (one per enemy, assign in order) ===")]
        [SerializeField] private EnemyButtonHandler[] _enemyButtons;

        private StringBuilder _logBuilder = new StringBuilder();
        private bool _waitingForTarget = false;
        private PlayerAction _pendingAction = PlayerAction.None;
        private Action[] _enemyButtonDelegates;

        private void OnEnable()
        {
            if (_battleController == null)
            {
                Debug.LogError("[BattleUIController] BattleController reference not assigned!");
                return;
            }

            // Subscribe to events
            _battleController.OnBattleStart += HandleBattleStart;
            _battleController.OnBattleWon += HandleBattleWon;
            _battleController.OnBattleLost += HandleBattleLost;
            _battleController.OnStateChanged += HandleStateChanged;
            _battleController.OnTurnStart += HandleTurnStart;
            _battleController.OnPlayerTurnStart += HandlePlayerTurnStart;
            _battleController.OnActionPointsChanged += HandleAPChanged;
            _battleController.OnPlayerInputStateChanged += HandleInputStateChanged;
            _battleController.OnDefensiveStanceChanged += HandleStanceChanged;
            _battleController.OnEnemyIntentDeclared += HandleEnemyIntent;
            _battleController.OnEnemyIntentsCleared += HandleIntentsCleared;
            _battleController.OnAttackExecuted += HandleAttackExecuted;
            _battleController.OnDodgeSuccess += HandleDodgeSuccess;
            _battleController.OnDodgeFailed += HandleDodgeFailed;
            _battleController.OnBlockSuccess += HandleBlockSuccess;
            _battleController.OnBlockBypassed += HandleBlockBypassed;
            _battleController.OnDamageTaken += HandleDamageTaken;
            _battleController.OnUnitDied += HandleUnitDied;
            _battleController.OnPlayerTurnEnd += HandlePlayerTurnEnd;
            _battleController.OnEnemyIntentPhaseStart += HandleEnemyIntentPhaseStart;
            _battleController.OnEnemyTurnStart += HandleEnemyTurnStart;
            _battleController.OnEnemyTurnEnd += HandleEnemyTurnEnd;
            _battleController.OnTurnEnd += HandleTurnEnd;
            _battleController.OnPlayerActionSelected += HandlePlayerActionSelected;

            // Wire up action buttons
            _attackButton.onClick.AddListener(OnAttackPressed);
            _skillButton.onClick.AddListener(OnSkillPressed);
            _itemButton.onClick.AddListener(OnItemPressed);
            _blockButton.onClick.AddListener(OnBlockPressed);
            _dodgeButton.onClick.AddListener(OnDodgePressed);

            // Wire up enemy target buttons (cache delegates for cleanup in OnDisable)
            _enemyButtonDelegates = new Action[_enemyButtons.Length];
            for (int i = 0; i < _enemyButtons.Length; i++)
            {
                int index = i; // Capture for closure
                _enemyButtonDelegates[i] = () => OnEnemyTargetSelected(index);
                _enemyButtons[i].OnClicked += _enemyButtonDelegates[i];
            }
        }

        private void OnDisable()
        {
            if (_battleController == null) return;

            _battleController.OnBattleStart -= HandleBattleStart;
            _battleController.OnBattleWon -= HandleBattleWon;
            _battleController.OnBattleLost -= HandleBattleLost;
            _battleController.OnStateChanged -= HandleStateChanged;
            _battleController.OnTurnStart -= HandleTurnStart;
            _battleController.OnPlayerTurnStart -= HandlePlayerTurnStart;
            _battleController.OnActionPointsChanged -= HandleAPChanged;
            _battleController.OnPlayerInputStateChanged -= HandleInputStateChanged;
            _battleController.OnDefensiveStanceChanged -= HandleStanceChanged;
            _battleController.OnEnemyIntentDeclared -= HandleEnemyIntent;
            _battleController.OnEnemyIntentsCleared -= HandleIntentsCleared;
            _battleController.OnAttackExecuted -= HandleAttackExecuted;
            _battleController.OnDodgeSuccess -= HandleDodgeSuccess;
            _battleController.OnDodgeFailed -= HandleDodgeFailed;
            _battleController.OnBlockSuccess -= HandleBlockSuccess;
            _battleController.OnBlockBypassed -= HandleBlockBypassed;
            _battleController.OnDamageTaken -= HandleDamageTaken;
            _battleController.OnUnitDied -= HandleUnitDied;
            _battleController.OnPlayerTurnEnd -= HandlePlayerTurnEnd;
            _battleController.OnEnemyIntentPhaseStart -= HandleEnemyIntentPhaseStart;
            _battleController.OnEnemyTurnStart -= HandleEnemyTurnStart;
            _battleController.OnEnemyTurnEnd -= HandleEnemyTurnEnd;
            _battleController.OnTurnEnd -= HandleTurnEnd;
            _battleController.OnPlayerActionSelected -= HandlePlayerActionSelected;

            _attackButton.onClick.RemoveListener(OnAttackPressed);
            _skillButton.onClick.RemoveListener(OnSkillPressed);
            _itemButton.onClick.RemoveListener(OnItemPressed);
            _blockButton.onClick.RemoveListener(OnBlockPressed);
            _dodgeButton.onClick.RemoveListener(OnDodgePressed);

            // Unsubscribe cached enemy button delegates
            if (_enemyButtonDelegates != null)
            {
                for (int i = 0; i < _enemyButtons.Length && i < _enemyButtonDelegates.Length; i++)
                {
                    _enemyButtons[i].OnClicked -= _enemyButtonDelegates[i];
                }
                _enemyButtonDelegates = null;
            }
        }

        // ==========================================
        // EVENT HANDLERS
        // ==========================================

        private void HandleBattleStart()
        {
            AppendLog("<b>--- BATTLE START ---</b>");
            RefreshAllHP();
        }

        private void HandleBattleWon()
        {
            AppendLog("<color=green><b>=== VICTORY! ===</b></color>");
            DisableAllButtons();
        }

        private void HandleBattleLost()
        {
            AppendLog("<color=red><b>=== DEFEAT... ===</b></color>");
            DisableAllButtons();
        }

        private void HandleStateChanged(BattleState previous, BattleState current)
        {
            if (_stateText != null)
                _stateText.text = $"Phase: {current}";
        }

        private void HandleTurnStart(int turn)
        {
            if (_turnText != null)
                _turnText.text = $"Turn {turn}";
            AppendLog($"\n<b>--- Turn {turn} ---</b>");
        }

        private void HandlePlayerTurnStart(int ap)
        {
            // Show which party member is currently acting
            IBattleUnit active = _battleController.ActiveUnit;
            string who = active != null ? active.UnitName : "???";
            if (_activeUnitText != null)
                _activeUnitText.text = $">> {who}'s Turn <<";
            AppendLog($"{who}'s turn! Choose an action.");
            RefreshActionButtons();
            CancelTargetSelection();
        }

        private void HandleAPChanged(int remaining)
        {
            if (_apText != null)
                _apText.text = $"AP: {remaining}/{_battleController.MaxActionPoints}";
            RefreshActionButtons();
        }

        private void HandleInputStateChanged(bool enabled)
        {
            if (!enabled)
            {
                DisableAllButtons();
                CancelTargetSelection();
            }
        }

        private void HandleStanceChanged(DefensiveStance stance)
        {
            if (_stanceText != null)
            {
                _stanceText.text = stance == DefensiveStance.None
                    ? ""
                    : $"Stance: {stance}";
            }
        }

        private void HandleEnemyIntent(IBattleUnit enemy, IBattleUnit target, bool unblockable, int damage)
        {
            string warning = unblockable ? " <color=red>[UNBLOCKABLE!]</color>" : "";
            AppendLog($"  {enemy.UnitName} → {target.UnitName} (~{damage} dmg){warning}");

            // Highlight the enemy button red if unblockable
            for (int i = 0; i < _battleController.EnemyUnits.Count && i < _enemyButtons.Length; i++)
            {
                if (_battleController.EnemyUnits[i] == enemy && unblockable)
                {
                    _enemyButtons[i].SetGlow(true);
                }
            }
        }

        private void HandleIntentsCleared()
        {
            // Remove all glows
            foreach (var btn in _enemyButtons)
            {
                btn.SetGlow(false);
            }
        }

        private void HandleAttackExecuted(IBattleUnit attacker, IBattleUnit target, int raw)
        {
            AppendLog($"  {attacker.UnitName} attacks {target.UnitName}!");
        }

        private void HandleDodgeSuccess(IBattleUnit dodger, IBattleUnit attacker)
        {
            AppendLog($"  <color=cyan>{dodger.UnitName} DODGED {attacker.UnitName}'s attack!</color>");
        }

        private void HandleDodgeFailed(IBattleUnit dodger, IBattleUnit attacker)
        {
            AppendLog($"  <color=yellow>{dodger.UnitName} failed to dodge!</color>");
        }

        private void HandleBlockSuccess(IBattleUnit blocker, IBattleUnit attacker, int reduced)
        {
            AppendLog($"  <color=cyan>{blocker.UnitName} BLOCKED! ({reduced} dmg leaked)</color>");
        }

        private void HandleBlockBypassed(IBattleUnit blocker, IBattleUnit attacker)
        {
            AppendLog($"  <color=red>{attacker.UnitName}'s attack is UNBLOCKABLE! Block bypassed!</color>");
        }

        private void HandleDamageTaken(IBattleUnit target, IBattleUnit attacker, int damage)
        {
            AppendLog($"  {target.UnitName} takes <color=red>{damage}</color> damage! ({target.CurrentHP}/{target.MaxHP} HP)");
            RefreshAllHP();
        }

        private void HandleUnitDied(IBattleUnit dead, IBattleUnit killer)
        {
            AppendLog($"  <color=red><b>{dead.UnitName} has been slain by {killer.UnitName}!</b></color>");
            RefreshAllHP();
        }

        private void HandlePlayerTurnEnd()
        {
            if (_activeUnitText != null)
                _activeUnitText.text = "";
            AppendLog("  Party phase complete.");
        }

        private void HandleEnemyIntentPhaseStart()
        {
            AppendLog("\n<b>--- Enemy Intent Phase ---</b>");
        }

        private void HandleEnemyTurnStart()
        {
            AppendLog("\n<b>--- Enemy Attacks ---</b>");
        }

        private void HandleEnemyTurnEnd()
        {
            AppendLog("  All enemies have acted.");
            if (_stanceText != null)
                _stanceText.text = "";
        }

        private void HandleTurnEnd(int turn)
        {
            AppendLog($"<i>--- End of Turn {turn} ---</i>\n");
            RefreshAllHP();
        }

        private void HandlePlayerActionSelected(PlayerAction action, IBattleUnit target)
        {
            string targetName = target != null ? target.UnitName : "";
            IBattleUnit active = _battleController.ActiveUnit;
            string who = active != null ? active.UnitName : "???";

            switch (action)
            {
                case PlayerAction.Attack:
                    AppendLog($"  {who} chooses <b>Attack</b> → {targetName}");
                    break;
                case PlayerAction.Skill:
                    AppendLog($"  {who} chooses <b>Skill</b> → {targetName}");
                    break;
                case PlayerAction.Item:
                    AppendLog($"  {who} chooses <b>Item</b> → {targetName}");
                    break;
                case PlayerAction.Block:
                    AppendLog($"  {who} chooses <b>Block</b>");
                    break;
                case PlayerAction.Dodge:
                    AppendLog($"  {who} chooses <b>Dodge</b>");
                    break;
            }
        }

        // ==========================================
        // BUTTON CALLBACKS
        // ==========================================

        private void OnAttackPressed()
        {
            // Attack needs a target — enter target selection mode
            _pendingAction = PlayerAction.Attack;
            _waitingForTarget = true;
            AppendLog("  Select an enemy to attack...");
            HighlightEnemyButtons(true);
        }

        private void OnSkillPressed()
        {
            // TODO: Show skill selection sub-menu. For now, use skill index 0 on first enemy.
            IBattleUnit target = GetFirstLivingEnemy();
            if (target != null)
                _battleController.SelectSkill(0, target);
        }

        private void OnItemPressed()
        {
            // TODO: Show item selection sub-menu. For now, use item index 0 on player.
            IBattleUnit target = GetFirstLivingPlayer();
            if (target != null)
                _battleController.SelectItem(0, target);
        }

        private void OnBlockPressed()
        {
            _battleController.SelectBlock();
        }

        private void OnDodgePressed()
        {
            _battleController.SelectDodge();
        }

        private void OnEnemyTargetSelected(int enemyIndex)
        {
            if (!_waitingForTarget) return;
            if (enemyIndex < 0 || enemyIndex >= _battleController.EnemyUnits.Count) return;

            IBattleUnit target = _battleController.EnemyUnits[enemyIndex];
            if (!target.IsAlive) return;

            CancelTargetSelection();

            if (_pendingAction == PlayerAction.Attack)
            {
                _battleController.SelectAttack(target);
            }
            // TODO: Handle Skill/Item targeting here when those systems exist
        }

        // ==========================================
        // HELPERS
        // ==========================================

        private void RefreshActionButtons()
        {
            if (_battleController == null) return;

            _attackButton.interactable = _battleController.CanPerformAction(PlayerAction.Attack);
            _skillButton.interactable = _battleController.CanPerformAction(PlayerAction.Skill);
            _itemButton.interactable = _battleController.CanPerformAction(PlayerAction.Item);
            _blockButton.interactable = _battleController.CanPerformAction(PlayerAction.Block);
            _dodgeButton.interactable = _battleController.CanPerformAction(PlayerAction.Dodge);
        }

        private void DisableAllButtons()
        {
            _attackButton.interactable = false;
            _skillButton.interactable = false;
            _itemButton.interactable = false;
            _blockButton.interactable = false;
            _dodgeButton.interactable = false;
        }

        private void RefreshAllHP()
        {
            // Player party HP + resource (one text per party member)
            for (int i = 0; i < _battleController.PlayerParty.Count && i < _playerHPTexts.Length; i++)
            {
                IBattleUnit player = _battleController.PlayerParty[i];
                if (_playerHPTexts[i] != null)
                {
                    if (!player.IsAlive)
                    {
                        _playerHPTexts[i].text = $"{player.UnitName}: DEAD";
                    }
                    else
                    {
                        string hp = $"{player.UnitName}: {player.CurrentHP}/{player.MaxHP}";
                        // Show resource (MP/Charges) if the unit has one
                        if (player.MaxResource > 0)
                            hp += $" | {player.ResourceLabel}: {player.CurrentResource}/{player.MaxResource}";
                        _playerHPTexts[i].text = hp;
                    }
                }
            }

            // Enemy HP
            for (int i = 0; i < _battleController.EnemyUnits.Count && i < _enemyHPTexts.Length; i++)
            {
                IBattleUnit enemy = _battleController.EnemyUnits[i];
                if (_enemyHPTexts[i] != null)
                {
                    _enemyHPTexts[i].text = enemy.IsAlive
                        ? $"{enemy.UnitName}: {enemy.CurrentHP}/{enemy.MaxHP}"
                        : $"{enemy.UnitName}: DEAD";
                }
            }
        }

        private void HighlightEnemyButtons(bool highlight)
        {
            for (int i = 0; i < _enemyButtons.Length; i++)
            {
                if (i < _battleController.EnemyUnits.Count && _battleController.EnemyUnits[i].IsAlive)
                    _enemyButtons[i].SetTargetable(highlight);
                else
                    _enemyButtons[i].SetTargetable(false);
            }
        }

        private void CancelTargetSelection()
        {
            _waitingForTarget = false;
            _pendingAction = PlayerAction.None;
            HighlightEnemyButtons(false);
        }

        private void AppendLog(string message)
        {
            _logBuilder.AppendLine(message);
            if (_logText != null)
            {
                _logText.text = _logBuilder.ToString();
            }
            // Auto-scroll to bottom
            if (_logScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                _logScrollRect.verticalNormalizedPosition = 0f;
            }
        }

        private IBattleUnit GetFirstLivingEnemy()
        {
            foreach (var e in _battleController.EnemyUnits)
            {
                if (e.IsAlive) return e;
            }
            return null;
        }

        private IBattleUnit GetFirstLivingPlayer()
        {
            foreach (var p in _battleController.PlayerParty)
            {
                if (p.IsAlive) return p;
            }
            return null;
        }
    }
}
```

---

## 4. File #3 — EnemyButtonHandler.cs

Small component attached to each enemy's clickable area. Handles visual states (targetable highlight, unblockable glow).

```csharp
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.UI
{
    /// <summary>
    /// Attach to each enemy's UI button/panel. Handles click events
    /// and visual states for targeting and unblockable glow.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class EnemyButtonHandler : MonoBehaviour
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _targetableColor = Color.yellow;
        [SerializeField] private Color _glowColor = Color.red;

        public event Action OnClicked;

        private Button _button;
        private bool _isGlowing;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() => OnClicked?.Invoke());
        }

        /// <summary>
        /// Highlight this enemy as a valid attack target.
        /// </summary>
        public void SetTargetable(bool targetable)
        {
            if (_isGlowing) return; // Don't override glow

            if (_backgroundImage != null)
                _backgroundImage.color = targetable ? _targetableColor : _normalColor;

            _button.interactable = targetable;
        }

        /// <summary>
        /// Apply red glow for unblockable attack intent.
        /// Overrides targetable highlight.
        /// </summary>
        public void SetGlow(bool glow)
        {
            _isGlowing = glow;
            if (_backgroundImage != null)
                _backgroundImage.color = glow ? _glowColor : _normalColor;
        }
    }
}
```

---

## 5. File #4 — BattleSceneBootstrap.cs

Wires everything together for a quick play-mode test. Attach to an empty GameObject in your test scene.

```csharp
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Drop onto an empty GameObject in a test scene.
    /// Starts the battle automatically on scene load using
    /// the Inspector-assigned combatants on BattleController.
    /// </summary>
    public class BattleSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private BattleController _battleController;

        private void Start()
        {
            if (_battleController == null)
            {
                Debug.LogError("[BattleSceneBootstrap] BattleController not assigned!");
                return;
            }

            _battleController.StartBattle();
        }
    }
}
```

---

## 6. Scene Setup Instructions (Manual — Do NOT Automate)

After creating the 4 files, provide the user with these instructions to set up the test scene manually in the Unity Editor. Do NOT try to create GameObjects or scenes via code.

### Step-by-step:

1. **Create a new scene** (File → New Scene → Basic).

2. **Create the BattleController GameObject:**
   - Empty GameObject → name it `BattleController`
   - Add `BattleController` component
   - Add `BattleSceneBootstrap` component → drag `BattleController` into its `_battleController` slot

3. **Create player party (multiple units — each acts 1-by-1 during the player phase):**
   - Empty GameObject → name it `PlayerUnit1`
   - Add `TestBattleUnit` component
   - Set: Name = "Mercury" (protagonist, older Alchemist), UnitType = Alchemist, Element = Ignis, MaxHP = 150, AttackPower = 25, Defense = 8, MaxResource = 5, CatalystName = "Flask of Ignis"
   - Empty GameObject → name it `PlayerUnit2`
   - Add `TestBattleUnit` component
   - Set: Name = "Sulfur" (female Magus defector, fights against other Magi), UnitType = Magus, Element = Lux, MaxHP = 100, AttackPower = 30, Defense = 5, MaxResource = 40
   - *(More party members are added over the course of the story — the system supports any number.)*

4. **Create 2-3 enemy units (mix of Magus and Knight types):**
   - Empty GameObjects → name them `Enemy1`, `Enemy2`, `Enemy3`
   - Add `TestBattleUnit` to each
   - Enemy 1: Name = "Magus Grunt", UnitType = Magus, Element = Umbra, MaxHP = 80, AttackPower = 15, Defense = 3, Unblockable Chance = 0.1
   - Enemy 2: Name = "Magus Mage", UnitType = Magus, Element = Lux, MaxHP = 60, AttackPower = 30, Defense = 2, Unblockable Chance = 0.3
   - Enemy 3: Name = "Hired Knight", UnitType = Knight, Element = None, MaxHP = 100, AttackPower = 22, Defense = 10, HasShield = true, Unblockable Chance = 0.0

5. **Wire combatants into BattleController:**
   - On the `BattleController` component, add `PlayerUnit1`, `PlayerUnit2` to `Player Party Raw` list (order = turn order)
   - Add `Enemy1`, `Enemy2`, `Enemy3` to `Enemy Units Raw` list

6. **Create the Canvas:**
   - GameObject → UI → Canvas (Screen Space - Overlay)
   - Add a `BattleUIController` component to the Canvas
   - Drag `BattleController` into its `_battleController` slot

7. **Create UI elements on the Canvas:**

   All text elements use **TextMeshPro** (GameObject → UI → Text - TextMeshPro). If prompted to import TMP Essentials, click "Import".

   **Info panel (top):**
   - TextMeshPro: "Turn 1" → assign to `_turnText`
   - TextMeshPro: "Phase: None" → assign to `_stateText`
   - TextMeshPro: "AP: 2/2" → assign to `_apText`
   - TextMeshPro: "" → assign to `_stanceText`

   **Active unit indicator (top-center):**
   - TextMeshPro: ">> Mercury's Turn <<" → assign to `_activeUnitText`

   **Player Party HP + Resource (left — one TMP per party member):**
   - TextMeshPro: "Mercury: 150/150 | Flask of Ignis: 5/5" → assign to `_playerHPTexts[0]`
   - TextMeshPro: "Sulfur: 100/100 | MP: 40/40" → assign to `_playerHPTexts[1]`
   - *(Add more as the party grows.)*

   **Enemy panel (right) — one panel per enemy:**
   - For each enemy, create a Button with an Image child and a TextMeshPro child
   - Add `EnemyButtonHandler` component to each Button
   - Drag the Button's Image into `_backgroundImage`
   - Write the enemy name on the TextMeshPro
   - Assign each enemy's HP TextMeshPro to `_enemyHPTexts` array (in order)
   - Assign each `EnemyButtonHandler` to `_enemyButtons` array (in order)

   **Action buttons (bottom):**
   - 5 Buttons labeled: "Attack", "Skill", "Item", "Block", "Dodge"
   - Assign to `_attackButton`, `_skillButton`, `_itemButton`, `_blockButton`, `_dodgeButton`

   **Combat log (center/bottom):**
   - Create a Scroll View (GameObject → UI → Scroll View)
   - Inside Content, add a TextMeshPro (UI) with: Overflow = Overflow, Font Size = 14, Rich Text = true
   - Assign the TMP_Text to `_logText`
   - Assign the ScrollRect to `_logScrollRect`

8. **Press Play** and test!

---

## 7. What To Test

After setup, verify these scenarios in play mode:

| # | Test | Expected Result |
|---|------|----------------|
| 1 | Battle starts | UI shows ">> Mercury's Turn <<". Both party members' HP + resource displayed on left. Mercury shows "Flask of Ignis: 5/5", Sulfur shows "MP: 40/40". |
| 2 | Mercury: Press Attack → click an enemy | Enemy takes damage. Turn advances to Sulfur (next party member). |
| 3 | Sulfur: Press Attack → click an enemy | Sulfur attacks. After all party members act, enemy phase begins. |
| 4 | Press Skill → then Attack | Skill logs "not implemented", then attack deals damage. Turn advances to next party member. |
| 5 | Press Item → then Dodge | Item logs "not implemented", then "Stance: Dodging" appears. Turn advances to next party member. |
| 6 | Press Block | "Stance: Blocking" appears. Turn advances to next party member. AP goes to 0. |
| 7 | Press Block → try Block again | Second block should be impossible (button disabled). |
| 8 | Press Dodge → try Block | Block button disabled after dodge chosen. |
| 9 | Wait for enemy intent phase | Log shows which enemies target which party members. Unblockable attacks show red text. |
| 10 | Block against unblockable | Log shows "UNBLOCKABLE! Block bypassed!" — full damage taken. |
| 11 | Dodge against unblockable | Dodge still has 75% chance to work. |
| 12 | Kill all enemies | "VICTORY!" message in log. All buttons disabled. |
| 13 | Let enemies kill all party members | "DEFEAT..." message. All buttons disabled. |
| 14 | One party member dies mid-battle | Dead member is skipped in turn order. Remaining members continue acting. |

---

## 8. File Structure When Done

```
Assets/
└── Scripts/
    └── Battle/
        ├── Interfaces/
        │   ├── IBattleUnit.cs
        │   └── IEnemyAI.cs
        ├── UI/
        │   ├── BattleUIController.cs
        │   └── EnemyButtonHandler.cs
        ├── BattleEnums.cs
        ├── BattleController.cs
        ├── TestBattleUnit.cs
        └── BattleSceneBootstrap.cs
```

**Create these 4 files. The BattleController files (4 files) should already exist. Total: 8 files.**
