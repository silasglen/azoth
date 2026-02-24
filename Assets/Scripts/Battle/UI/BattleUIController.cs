using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Battle;

namespace Battle.UI
{
    public enum SubMenuMode
    {
        None,
        Skill,
        Item
    }

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

        [Header("=== Action Buttons Panel (parent of the 5 action buttons) ===")]
        [SerializeField] private GameObject _actionButtonsPanel;

        [Header("=== Sub-Menu (Skill/Item selection) ===")]
        [SerializeField] private GameObject _subMenuPanel;
        [SerializeField] private Button[] _subMenuButtons;
        [SerializeField] private TMP_Text[] _subMenuButtonLabels;
        [SerializeField] private Button _subMenuBackButton;

        [Header("=== Combat Log ===")]
        [SerializeField] private TMP_Text _logText;
        [SerializeField] private ScrollRect _logScrollRect;

        [Header("=== Enemy Target Buttons (one per enemy, assign in order) ===")]
        [SerializeField] private EnemyButtonHandler[] _enemyButtons;

        [Header("=== F4: Enemy HP Bars (one per enemy, assign in order) ===")]
        [SerializeField] private EnemyHPBar[] _enemyHPBars;

        [Header("=== F4: Status Effect Displays ===")]
        [SerializeField] private StatusEffectDisplay[] _playerStatusDisplays;
        [SerializeField] private StatusEffectDisplay[] _enemyStatusDisplays;

        [Header("=== Flee Button ===")]
        [SerializeField] private Button _fleeButton;

        [Header("=== F4: Scan ===")]
        [SerializeField] private Button _scanButton;
        [SerializeField] private ScanResultPanel _scanResultPanel;

        [Header("=== F4: Tooltip ===")]
        [SerializeField] private TooltipPanel _tooltipPanel;

        [Header("=== F4: Hotbar ===")]
        [SerializeField] private BattleHotbar _hotbar;

        private StringBuilder _logBuilder = new StringBuilder();
        private bool _waitingForTarget = false;
        private PlayerAction _pendingAction = PlayerAction.None;
        private Action[] _enemyButtonDelegates;

        // Sub-menu state
        private SubMenuMode _subMenuMode = SubMenuMode.None;
        private int _selectedSubIndex = -1;

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
            _battleController.OnEnemyActionStart += HandleEnemyActionStart;
            _battleController.OnEnemyTurnStart += HandleEnemyTurnStart;
            _battleController.OnEnemyTurnEnd += HandleEnemyTurnEnd;
            _battleController.OnTurnEnd += HandleTurnEnd;
            _battleController.OnPlayerActionSelected += HandlePlayerActionSelected;
            _battleController.OnHealApplied += HandleHealApplied;
            _battleController.OnResourceRestored += HandleResourceRestored;

            // New events
            _battleController.OnStatusEffectApplied += HandleStatusEffectApplied;
            _battleController.OnStatusEffectTick += HandleStatusEffectTick;
            _battleController.OnStatusEffectExpired += HandleStatusEffectExpired;
            _battleController.OnStunSkipped += HandleStunSkipped;
            _battleController.OnCriticalHit += HandleCriticalHit;
            _battleController.OnUnitRevived += HandleUnitRevived;
            _battleController.OnScanCompleted += HandleScanCompleted;

            // B1 AI pattern events
            _battleController.OnResourceBurned += HandleResourceBurned;
            _battleController.OnItemDestroyed += HandleItemDestroyed;
            _battleController.OnAttackRedirected += HandleAttackRedirected;

            // Wire up action buttons
            _attackButton.onClick.AddListener(OnAttackPressed);
            _skillButton.onClick.AddListener(OnSkillPressed);
            _itemButton.onClick.AddListener(OnItemPressed);
            _blockButton.onClick.AddListener(OnBlockPressed);
            _dodgeButton.onClick.AddListener(OnDodgePressed);

            if (_scanButton != null)
                _scanButton.onClick.AddListener(OnScanPressed);

            if (_fleeButton != null)
                _fleeButton.onClick.AddListener(OnFleePressed);

            // Wire up sub-menu buttons
            if (_subMenuButtons != null)
            {
                for (int i = 0; i < _subMenuButtons.Length; i++)
                {
                    int index = i;
                    _subMenuButtons[i].onClick.AddListener(() => OnSubMenuButtonSelected(index));
                }
            }
            if (_subMenuBackButton != null)
                _subMenuBackButton.onClick.AddListener(OnSubMenuBack);

            // Wire up enemy target buttons (cache delegates for cleanup in OnDisable)
            _enemyButtonDelegates = new Action[_enemyButtons.Length];
            for (int i = 0; i < _enemyButtons.Length; i++)
            {
                int index = i; // Capture for closure
                _enemyButtonDelegates[i] = () => OnEnemyTargetSelected(index);
                _enemyButtons[i].OnClicked += _enemyButtonDelegates[i];
            }

            // Ensure sub-menu starts hidden
            if (_subMenuPanel != null) _subMenuPanel.SetActive(false);
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
            _battleController.OnEnemyActionStart -= HandleEnemyActionStart;
            _battleController.OnEnemyTurnStart -= HandleEnemyTurnStart;
            _battleController.OnEnemyTurnEnd -= HandleEnemyTurnEnd;
            _battleController.OnTurnEnd -= HandleTurnEnd;
            _battleController.OnPlayerActionSelected -= HandlePlayerActionSelected;
            _battleController.OnHealApplied -= HandleHealApplied;
            _battleController.OnResourceRestored -= HandleResourceRestored;

            // New events
            _battleController.OnStatusEffectApplied -= HandleStatusEffectApplied;
            _battleController.OnStatusEffectTick -= HandleStatusEffectTick;
            _battleController.OnStatusEffectExpired -= HandleStatusEffectExpired;
            _battleController.OnStunSkipped -= HandleStunSkipped;
            _battleController.OnCriticalHit -= HandleCriticalHit;
            _battleController.OnUnitRevived -= HandleUnitRevived;
            _battleController.OnScanCompleted -= HandleScanCompleted;

            // B1 AI pattern events
            _battleController.OnResourceBurned -= HandleResourceBurned;
            _battleController.OnItemDestroyed -= HandleItemDestroyed;
            _battleController.OnAttackRedirected -= HandleAttackRedirected;

            _attackButton.onClick.RemoveListener(OnAttackPressed);
            _skillButton.onClick.RemoveListener(OnSkillPressed);
            _itemButton.onClick.RemoveListener(OnItemPressed);
            _blockButton.onClick.RemoveListener(OnBlockPressed);
            _dodgeButton.onClick.RemoveListener(OnDodgePressed);

            if (_scanButton != null)
                _scanButton.onClick.RemoveListener(OnScanPressed);

            if (_fleeButton != null)
                _fleeButton.onClick.RemoveListener(OnFleePressed);

            // Clean up sub-menu buttons
            if (_subMenuButtons != null)
            {
                foreach (var btn in _subMenuButtons)
                    btn.onClick.RemoveAllListeners();
            }
            if (_subMenuBackButton != null)
                _subMenuBackButton.onClick.RemoveListener(OnSubMenuBack);

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

            // Initialize enemy HP bars
            if (_enemyHPBars != null)
            {
                for (int i = 0; i < _battleController.EnemyUnits.Count && i < _enemyHPBars.Length; i++)
                {
                    if (_enemyHPBars[i] != null)
                    {
                        IBattleUnit enemy = _battleController.EnemyUnits[i];
                        bool alreadyScanned = BestiaryData.IsScanned(enemy.UnitName);
                        _enemyHPBars[i].Init(enemy, alreadyScanned);
                    }
                }
            }

            // Initialize status effect displays
            if (_playerStatusDisplays != null)
            {
                for (int i = 0; i < _battleController.PlayerParty.Count && i < _playerStatusDisplays.Length; i++)
                {
                    if (_playerStatusDisplays[i] != null)
                        _playerStatusDisplays[i].Init(_battleController.PlayerParty[i], _battleController);
                }
            }
            if (_enemyStatusDisplays != null)
            {
                for (int i = 0; i < _battleController.EnemyUnits.Count && i < _enemyStatusDisplays.Length; i++)
                {
                    if (_enemyStatusDisplays[i] != null)
                        _enemyStatusDisplays[i].Init(_battleController.EnemyUnits[i], _battleController);
                }
            }

            RefreshAllHP();
            // Clear stance text since the event won't fire for the initial None state
            if (_stanceText != null)
                _stanceText.text = "";
        }

        private void HandleBattleWon()
        {
            AppendLog("<color=green><b>=== VICTORY! ===</b></color>");
            DisableAllButtons();
            HideSubMenu();
        }

        private void HandleBattleLost()
        {
            AppendLog("<color=red><b>=== DEFEAT... ===</b></color>");
            DisableAllButtons();
            HideSubMenu();
        }

        private void HandleStateChanged(BattleState previous, BattleState current)
        {
            if (_stateText != null)
                _stateText.text = GetFriendlyStateName(current);
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
            if (_apText != null)
                _apText.text = $"AP: {ap}/{_battleController.MaxActionPoints}";
            AppendLog($"<b>{who}</b>'s turn — {ap} AP available.");
            RefreshActionButtons();
            CancelTargetSelection();
            HideSubMenu();
        }

        private void HandleAPChanged(int remaining)
        {
            if (_apText != null)
                _apText.text = $"AP: {remaining}/{_battleController.MaxActionPoints}";
            RefreshActionButtons();
            RefreshAllHP();
        }

        private void HandleInputStateChanged(bool enabled)
        {
            if (!enabled)
            {
                DisableAllButtons();
                CancelTargetSelection();
                HideSubMenu();
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

        private void HandleEnemyIntent(IBattleUnit enemy, IBattleUnit target, bool unblockable, int damage, string skillName)
        {
            string warning = unblockable ? " <color=red>[UNBLOCKABLE!]</color>" : "";
            string skillTag = skillName != null ? $" <color=yellow>[{skillName}]</color>" : "";
            AppendLog($"  {enemy.UnitName} → {target.UnitName}{skillTag} (~{damage} dmg){warning}");

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
            // Check if a Coward fled instead of dying
            if (dead is CowardAI coward && coward.HasFled)
            {
                AppendLog($"  <color=yellow><b>{dead.UnitName} fled from battle!</b></color>");
            }
            else
            {
                AppendLog($"  <color=red><b>{dead.UnitName} has been slain by {killer.UnitName}!</b></color>");
            }
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

        private void HandleEnemyActionStart(IBattleUnit enemy, IBattleUnit target)
        {
            AppendLog($"  <color=orange><b>{enemy.UnitName}</b> prepares to act against {target.UnitName}...</color>");
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
                    // Log is handled more specifically in sub-menu selection
                    break;
                case PlayerAction.Item:
                    // Log is handled more specifically in sub-menu selection
                    break;
                case PlayerAction.Block:
                    AppendLog($"  {who} chooses <b>Block</b>");
                    break;
                case PlayerAction.Dodge:
                    AppendLog($"  {who} chooses <b>Dodge</b>");
                    break;
                case PlayerAction.Scan:
                    AppendLog($"  {who} chooses <b>Scan</b> → {targetName}");
                    break;
                case PlayerAction.Flee:
                    AppendLog($"  {who} attempts to <b>Flee</b>!");
                    break;
            }
        }

        private void HandleHealApplied(IBattleUnit unit, int amount)
        {
            AppendLog($"  <color=green>{unit.UnitName} heals {amount} HP! ({unit.CurrentHP}/{unit.MaxHP})</color>");
            RefreshAllHP();
        }

        private void HandleResourceRestored(IBattleUnit unit, int amount)
        {
            AppendLog($"  <color=#88aaff>{unit.UnitName} restores {amount} {unit.ResourceLabel}! ({unit.CurrentResource}/{unit.MaxResource})</color>");
            RefreshAllHP();
        }

        // --- New event handlers ---

        private void HandleStatusEffectApplied(IBattleUnit unit, StatusEffectType type, int duration, float value)
        {
            string color = GetStatusEffectColor(type);
            AppendLog($"  <color={color}>{unit.UnitName} is afflicted with {type} ({duration} turns)!</color>");
            RefreshAllHP();
        }

        private void HandleStatusEffectTick(IBattleUnit unit, StatusEffectType type, int damage)
        {
            string color = GetStatusEffectColor(type);
            AppendLog($"  <color={color}>{unit.UnitName} takes {damage} {type} damage!</color>");
            RefreshAllHP();
        }

        private void HandleStatusEffectExpired(IBattleUnit unit, StatusEffectType type)
        {
            AppendLog($"  <color=#aaaaaa>{unit.UnitName}'s {type} wore off.</color>");
            RefreshAllHP();
        }

        private void HandleStunSkipped(IBattleUnit unit)
        {
            AppendLog($"  <color=yellow><b>{unit.UnitName} is STUNNED and cannot act!</b></color>");
        }

        private void HandleCriticalHit(IBattleUnit attacker, IBattleUnit target)
        {
            AppendLog($"  <color=#ff4444><b>CRITICAL HIT!</b></color>");
        }

        private void HandleUnitRevived(IBattleUnit unit, int hp)
        {
            AppendLog($"  <color=green><b>{unit.UnitName} has been revived with {hp} HP!</b></color>");
            RefreshAllHP();
        }

        // --- B1 AI Pattern event handlers ---

        private void HandleResourceBurned(IBattleUnit target, int amount)
        {
            string label = target.MaxResource > 0 ? target.ResourceLabel : "resource";
            AppendLog($"  <color=#cc44ff>{target.UnitName} lost {amount} {label} to resource burn! ({target.CurrentResource}/{target.MaxResource})</color>");
            RefreshAllHP();
        }

        private void HandleItemDestroyed(string itemName)
        {
            AppendLog($"  <color=#ff6644><b>Corrosive attack destroyed {itemName}!</b></color>");
        }

        private void HandleAttackRedirected(IBattleUnit attacker, IBattleUnit originalTarget, IBattleUnit newTarget)
        {
            AppendLog($"  <color=#44aaff><b>{newTarget.UnitName} intercepts the attack meant for {originalTarget.UnitName}!</b></color>");
        }

        private void HandleScanCompleted(IBattleUnit scanner, IBattleUnit target)
        {
            string who = scanner != null ? scanner.UnitName : "???";
            AppendLog($"  <color=#88ffcc>{who} scans <b>{target.UnitName}</b>!</color>");

            // Show scan result panel
            if (_scanResultPanel != null)
                _scanResultPanel.ShowScanResult(target);

            // Enable HP numbers on the scanned enemy's HP bar
            if (_enemyHPBars != null)
            {
                for (int i = 0; i < _battleController.EnemyUnits.Count && i < _enemyHPBars.Length; i++)
                {
                    if (_battleController.EnemyUnits[i] == target && _enemyHPBars[i] != null)
                    {
                        _enemyHPBars[i].SetShowNumbers(true);
                        break;
                    }
                }
            }

            RefreshAllHP();
        }

        // ==========================================
        // BUTTON CALLBACKS
        // ==========================================

        private void OnAttackPressed()
        {
            // Attack needs a target — enter target selection mode
            _pendingAction = PlayerAction.Attack;
            _waitingForTarget = true;
            AppendLog("  <i>Select a target...</i>");
            HighlightEnemyButtons(true);
        }

        private void OnSkillPressed()
        {
            ShowSkillSubMenu();
        }

        private void OnItemPressed()
        {
            ShowItemSubMenu();
        }

        private void OnBlockPressed()
        {
            _battleController.SelectBlock();
        }

        private void OnDodgePressed()
        {
            _battleController.SelectDodge();
        }

        private void OnScanPressed()
        {
            _pendingAction = PlayerAction.Scan;
            _waitingForTarget = true;
            AppendLog("  <i>Select a target to scan...</i>");
            HighlightEnemyButtons(true);
        }

        private void OnFleePressed()
        {
            _battleController.SelectFlee();
        }

        private void OnEnemyTargetSelected(int enemyIndex)
        {
            if (!_waitingForTarget) return;
            if (enemyIndex < 0 || enemyIndex >= _battleController.EnemyUnits.Count) return;

            IBattleUnit target = _battleController.EnemyUnits[enemyIndex];
            if (!target.IsAlive) return;

            PlayerAction action = _pendingAction;
            int subIndex = _selectedSubIndex;
            CancelTargetSelection();

            if (action == PlayerAction.Attack)
            {
                _battleController.SelectAttack(target);
            }
            else if (action == PlayerAction.Skill)
            {
                var skills = _battleController.ActiveUnitSkills;
                if (subIndex >= 0 && subIndex < skills.Count)
                {
                    IBattleUnit active = _battleController.ActiveUnit;
                    string who = active != null ? active.UnitName : "???";
                    AppendLog($"  {who} uses <b>{skills[subIndex].Name}</b> → {target.UnitName}");
                }
                _battleController.SelectSkill(subIndex, target);
                HideSubMenu();
            }
            else if (action == PlayerAction.Item)
            {
                var inv = _battleController.BattleInventory;
                if (subIndex >= 0 && subIndex < inv.Count)
                {
                    IBattleUnit active = _battleController.ActiveUnit;
                    string who = active != null ? active.UnitName : "???";
                    AppendLog($"  {who} uses <b>{inv[subIndex].Item.Name}</b> → {target.UnitName}");
                }
                _battleController.SelectItem(subIndex, target);
                HideSubMenu();
            }
            else if (action == PlayerAction.Scan)
            {
                _battleController.SelectScan(target);
            }
        }

        // ==========================================
        // SUB-MENU (Skill / Item)
        // ==========================================

        private void ShowSkillSubMenu()
        {
            _subMenuMode = SubMenuMode.Skill;
            var skills = _battleController.ActiveUnitSkills;

            if (_subMenuButtons == null || _subMenuButtonLabels == null) return;

            for (int i = 0; i < _subMenuButtons.Length; i++)
            {
                if (i < skills.Count)
                {
                    _subMenuButtons[i].gameObject.SetActive(true);
                    SkillDefinition sk = skills[i];
                    string costStr = sk.ResourceCost > 0
                        ? $" ({sk.ResourceCost} {(_battleController.ActiveUnit?.ResourceLabel ?? "")})"
                        : " (Free)";
                    if (_subMenuButtonLabels[i] != null)
                        _subMenuButtonLabels[i].text = sk.Name + costStr;

                    _subMenuButtons[i].interactable = _battleController.CanAffordSkill(i);

                    // Set tooltip data on sub-menu button
                    if (_tooltipPanel != null)
                    {
                        TooltipTrigger trigger = _subMenuButtons[i].GetComponent<TooltipTrigger>();
                        if (trigger == null)
                            trigger = _subMenuButtons[i].gameObject.AddComponent<TooltipTrigger>();
                        trigger.SetSkillData(sk, _battleController.ActiveUnit, _tooltipPanel);
                    }
                }
                else
                {
                    _subMenuButtons[i].gameObject.SetActive(false);
                }
            }

            if (_subMenuPanel != null) _subMenuPanel.SetActive(true);
            if (_actionButtonsPanel != null) _actionButtonsPanel.SetActive(false);
        }

        private void ShowItemSubMenu()
        {
            _subMenuMode = SubMenuMode.Item;
            var inv = _battleController.BattleInventory;

            if (_subMenuButtons == null || _subMenuButtonLabels == null) return;

            bool hasDeadAlly = _battleController.HasDeadAlly();

            int shown = 0;
            for (int i = 0; i < _subMenuButtons.Length; i++)
            {
                if (i < inv.Count)
                {
                    ItemSlot slot = inv[i];
                    if (slot.Quantity > 0)
                    {
                        _subMenuButtons[i].gameObject.SetActive(true);
                        string label = $"{slot.Item.Name} x{slot.Quantity}";
                        if (slot.Item.IsRevive)
                        {
                            label = $"<color=green>[REVIVE]</color> {label}";
                            // Revive items only interactable if there's a dead ally
                            _subMenuButtons[i].interactable = hasDeadAlly;
                        }
                        else
                        {
                            _subMenuButtons[i].interactable = true;
                        }
                        if (_subMenuButtonLabels[i] != null)
                            _subMenuButtonLabels[i].text = label;

                        // Set tooltip data on sub-menu button
                        if (_tooltipPanel != null)
                        {
                            TooltipTrigger trigger = _subMenuButtons[i].GetComponent<TooltipTrigger>();
                            if (trigger == null)
                                trigger = _subMenuButtons[i].gameObject.AddComponent<TooltipTrigger>();
                            trigger.SetItemData(slot.Item, _tooltipPanel);
                        }

                        shown++;
                    }
                    else
                    {
                        _subMenuButtons[i].gameObject.SetActive(false);
                    }
                }
                else
                {
                    _subMenuButtons[i].gameObject.SetActive(false);
                }
            }

            if (_subMenuPanel != null) _subMenuPanel.SetActive(true);
            if (_actionButtonsPanel != null) _actionButtonsPanel.SetActive(false);
        }

        private void OnSubMenuButtonSelected(int index)
        {
            _selectedSubIndex = index;

            if (_subMenuMode == SubMenuMode.Skill)
            {
                var skills = _battleController.ActiveUnitSkills;
                if (index < 0 || index >= skills.Count) return;

                SkillDefinition skill = skills[index];

                if (skill.TargetType == SkillTargetType.Enemy)
                {
                    // Need enemy target selection
                    _pendingAction = PlayerAction.Skill;
                    _waitingForTarget = true;
                    AppendLog("  <i>Select a target...</i>");
                    HighlightEnemyButtons(true);
                }
                else
                {
                    // Self or ally — execute immediately
                    IBattleUnit active = _battleController.ActiveUnit;
                    string who = active != null ? active.UnitName : "???";
                    string targetName = skill.TargetType == SkillTargetType.Self ? who : who;
                    AppendLog($"  {who} uses <b>{skill.Name}</b> → {targetName}");
                    _battleController.SelectSkill(index, null);
                    HideSubMenu();
                }
            }
            else if (_subMenuMode == SubMenuMode.Item)
            {
                var inv = _battleController.BattleInventory;
                if (index < 0 || index >= inv.Count) return;

                ItemDefinition item = inv[index].Item;

                // Revive items auto-execute (no target selection needed)
                if (item.IsRevive)
                {
                    IBattleUnit active = _battleController.ActiveUnit;
                    string who = active != null ? active.UnitName : "???";
                    AppendLog($"  {who} uses <b>{item.Name}</b>");
                    _battleController.SelectItem(index, null);
                    HideSubMenu();
                    return;
                }

                if (item.TargetType == SkillTargetType.Enemy)
                {
                    // Need enemy target selection
                    _pendingAction = PlayerAction.Item;
                    _waitingForTarget = true;
                    AppendLog("  <i>Select a target...</i>");
                    HighlightEnemyButtons(true);
                }
                else
                {
                    // Ally target — auto-target lowest HP ally
                    IBattleUnit active = _battleController.ActiveUnit;
                    string who = active != null ? active.UnitName : "???";
                    AppendLog($"  {who} uses <b>{item.Name}</b>");
                    _battleController.SelectItem(index, null);
                    HideSubMenu();
                }
            }
        }

        private void OnSubMenuBack()
        {
            HideSubMenu();
        }

        private void HideSubMenu()
        {
            _subMenuMode = SubMenuMode.None;
            _selectedSubIndex = -1;
            if (_subMenuPanel != null) _subMenuPanel.SetActive(false);
            if (_actionButtonsPanel != null) _actionButtonsPanel.SetActive(true);
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
            if (_scanButton != null)
                _scanButton.interactable = _battleController.CanPerformAction(PlayerAction.Scan);
            if (_fleeButton != null)
                _fleeButton.interactable = _battleController.CanPerformAction(PlayerAction.Flee);
        }

        private void DisableAllButtons()
        {
            _attackButton.interactable = false;
            _skillButton.interactable = false;
            _itemButton.interactable = false;
            _blockButton.interactable = false;
            _dodgeButton.interactable = false;
            if (_scanButton != null)
                _scanButton.interactable = false;
            if (_fleeButton != null)
                _fleeButton.interactable = false;
        }

        private void RefreshAllHP()
        {
            // Player party HP + resource + element + status effects (one text per party member)
            for (int i = 0; i < _battleController.PlayerParty.Count && i < _playerHPTexts.Length; i++)
            {
                IBattleUnit player = _battleController.PlayerParty[i];
                if (_playerHPTexts[i] != null)
                {
                    if (!player.IsAlive)
                    {
                        _playerHPTexts[i].text = $"<color=#666666>{player.UnitName}: DEAD</color>";
                    }
                    else
                    {
                        string elemTag = player.Element != ElementType.None
                            ? $" <color={GetElementColor(player.Element)}>[{player.Element}]</color>"
                            : "";
                        string hp = $"{player.UnitName}{elemTag}: {player.CurrentHP}/{player.MaxHP}";
                        // Show resource (MP/Charges) if the unit has one
                        if (player.MaxResource > 0)
                            hp += $" | {player.ResourceLabel}: {player.CurrentResource}/{player.MaxResource}";
                        // Show status effect indicators
                        string effects = BuildStatusEffectIndicators(player);
                        if (effects.Length > 0)
                            hp += $" {effects}";
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
                    if (enemy.IsAlive)
                    {
                        string elemTag = enemy.Element != ElementType.None
                            ? $" <color={GetElementColor(enemy.Element)}>[{enemy.Element}]</color>"
                            : "";
                        string hp = $"{enemy.UnitName}{elemTag}: {enemy.CurrentHP}/{enemy.MaxHP}";
                        // Show status effect indicators
                        string effects = BuildStatusEffectIndicators(enemy);
                        if (effects.Length > 0)
                            hp += $" {effects}";
                        _enemyHPTexts[i].text = hp;
                    }
                    else
                    {
                        _enemyHPTexts[i].text = $"<color=#666666>{enemy.UnitName}: DEAD</color>";
                    }
                }
            }

            // Refresh enemy HP bars
            if (_enemyHPBars != null)
            {
                for (int i = 0; i < _battleController.EnemyUnits.Count && i < _enemyHPBars.Length; i++)
                {
                    if (_enemyHPBars[i] != null)
                        _enemyHPBars[i].RefreshHP();
                }
            }

            // Refresh status effect displays
            if (_playerStatusDisplays != null)
            {
                for (int i = 0; i < _playerStatusDisplays.Length; i++)
                {
                    if (_playerStatusDisplays[i] != null)
                        _playerStatusDisplays[i].Refresh();
                }
            }
            if (_enemyStatusDisplays != null)
            {
                for (int i = 0; i < _enemyStatusDisplays.Length; i++)
                {
                    if (_enemyStatusDisplays[i] != null)
                        _enemyStatusDisplays[i].Refresh();
                }
            }

            // Refresh enemy button interactability so dead enemies can't be targeted
            RefreshEnemyButtonStates();
        }

        private string BuildStatusEffectIndicators(IBattleUnit unit)
        {
            var effects = _battleController.GetStatusEffects(unit);
            if (effects.Count == 0) return "";

            var sb = new StringBuilder();
            foreach (var eff in effects)
            {
                string color = GetStatusEffectColor(eff.Type);
                sb.Append($"<color={color}>[{eff.Type}:{eff.RemainingTurns}]</color>");
            }
            return sb.ToString();
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
            _selectedSubIndex = -1;
            HighlightEnemyButtons(false);
        }

        private void AppendLog(string message)
        {
            _logBuilder.AppendLine(message);
            if (_logText != null)
            {
                _logText.text = _logBuilder.ToString();
            }
            // Auto-scroll to bottom after layout updates
            if (_logScrollRect != null)
            {
                StartCoroutine(ScrollToBottomNextFrame());
            }
        }

        private System.Collections.IEnumerator ScrollToBottomNextFrame()
        {
            yield return null; // Wait one frame for layout rebuild
            if (_logScrollRect != null)
                _logScrollRect.verticalNormalizedPosition = 0f;
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

        /// <summary>
        /// Disables enemy buttons for dead enemies so they can't be targeted
        /// after dying mid-turn.
        /// </summary>
        private void RefreshEnemyButtonStates()
        {
            if (!_waitingForTarget) return;
            for (int i = 0; i < _enemyButtons.Length; i++)
            {
                if (i < _battleController.EnemyUnits.Count && !_battleController.EnemyUnits[i].IsAlive)
                    _enemyButtons[i].SetTargetable(false);
            }
        }

        private static string GetFriendlyStateName(BattleState state)
        {
            switch (state)
            {
                case BattleState.None:         return "";
                case BattleState.BattleStart:  return "Battle Starting...";
                case BattleState.PlayerTurn:   return "Your Turn";
                case BattleState.EnemyIntent:  return "Enemy Intents";
                case BattleState.EnemyTurn:    return "Enemy Turn";
                case BattleState.TurnEnd:      return "Turn End";
                case BattleState.BattleWon:    return "Victory!";
                case BattleState.BattleLost:   return "Defeat...";
                default:                       return state.ToString();
            }
        }

        private static string GetElementColor(ElementType element)
        {
            switch (element)
            {
                case ElementType.Ignis:  return "#ff6644";
                case ElementType.Aqua:   return "#4488ff";
                case ElementType.Terra:  return "#88aa44";
                case ElementType.Ventus: return "#aaddaa";
                case ElementType.Lux:    return "#ffdd44";
                case ElementType.Umbra:  return "#aa66ff";
                default:                 return "#cccccc";
            }
        }

        private static string GetStatusEffectColor(StatusEffectType type)
        {
            switch (type)
            {
                case StatusEffectType.Poison:  return "#44cc44";
                case StatusEffectType.Burn:    return "#ff6644";
                case StatusEffectType.Stun:    return "#ffcc00";
                case StatusEffectType.AtkUp:   return "#44ddff";
                case StatusEffectType.AtkDown: return "#ff4444";
                case StatusEffectType.DefUp:   return "#44ddff";
                case StatusEffectType.DefDown: return "#ff4444";
                default:                       return "#cccccc";
            }
        }

        // ==========================================
        // HOTBAR INTEGRATION (public entry points)
        // ==========================================

        /// <summary>
        /// Called by BattleHotbar to trigger skill target selection from the hotbar.
        /// Enters target selection mode for the given skill index.
        /// </summary>
        public void RequestTargetSelectionForSkill(int skillIndex)
        {
            if (_battleController == null || !_battleController.IsPlayerInputEnabled) return;

            var skills = _battleController.ActiveUnitSkills;
            if (skillIndex < 0 || skillIndex >= skills.Count) return;
            if (!_battleController.CanAffordSkill(skillIndex)) return;

            _selectedSubIndex = skillIndex;
            SkillDefinition skill = skills[skillIndex];

            if (skill.TargetType == SkillTargetType.Enemy)
            {
                _pendingAction = PlayerAction.Skill;
                _waitingForTarget = true;
                AppendLog($"  <i>Select a target for {skill.Name}...</i>");
                HighlightEnemyButtons(true);
            }
            else
            {
                IBattleUnit active = _battleController.ActiveUnit;
                string who = active != null ? active.UnitName : "???";
                AppendLog($"  {who} uses <b>{skill.Name}</b> (hotbar)");
                _battleController.SelectSkill(skillIndex, null);
            }
        }

        /// <summary>
        /// Called by BattleHotbar to trigger item target selection from the hotbar.
        /// Enters target selection mode for the given item index.
        /// </summary>
        public void RequestTargetSelectionForItem(int itemIndex)
        {
            if (_battleController == null || !_battleController.IsPlayerInputEnabled) return;

            var inv = _battleController.BattleInventory;
            if (itemIndex < 0 || itemIndex >= inv.Count) return;
            if (inv[itemIndex].Quantity <= 0) return;

            _selectedSubIndex = itemIndex;
            ItemDefinition item = inv[itemIndex].Item;

            if (item.IsRevive)
            {
                IBattleUnit active = _battleController.ActiveUnit;
                string who = active != null ? active.UnitName : "???";
                AppendLog($"  {who} uses <b>{item.Name}</b> (hotbar)");
                _battleController.SelectItem(itemIndex, null);
                return;
            }

            if (item.TargetType == SkillTargetType.Enemy)
            {
                _pendingAction = PlayerAction.Item;
                _waitingForTarget = true;
                AppendLog($"  <i>Select a target for {item.Name}...</i>");
                HighlightEnemyButtons(true);
            }
            else
            {
                IBattleUnit active = _battleController.ActiveUnit;
                string who = active != null ? active.UnitName : "???";
                AppendLog($"  {who} uses <b>{item.Name}</b> (hotbar)");
                _battleController.SelectItem(itemIndex, null);
            }
        }
    }
}
