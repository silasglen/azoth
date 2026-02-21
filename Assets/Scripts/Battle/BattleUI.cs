using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Event-driven battle UI.  Subscribes to <see cref="BattleManager"/> events
/// and updates panels/text accordingly.  Calls
/// <see cref="BattleManager.SubmitPlayerChoice"/> when the player selects
/// a skill and target.  Has zero logic about battle rules.
/// </summary>
public class BattleUI : MonoBehaviour
{
    // ── Serialized references ────────────────────────────────────────────
    [Header("Root")]
    [SerializeField] GameObject battlePanel;

    [Header("Player HUD")]
    [SerializeField] Slider  playerHPBar;
    [SerializeField] Slider  playerMPBar;
    [SerializeField] TextMeshProUGUI playerHPText;
    [SerializeField] TextMeshProUGUI playerMPText;

    [Header("Enemy List")]
    [SerializeField] Transform  enemyContainer;
    [SerializeField] GameObject enemyEntryPrefab;

    [Header("Skill Selection")]
    [SerializeField] Transform  skillButtonContainer;
    [SerializeField] GameObject skillButtonPrefab;
    [SerializeField] Button     attackButton;

    [Header("Target Selection")]
    [SerializeField] GameObject targetPanel;
    [SerializeField] Transform  targetButtonContainer;
    [SerializeField] GameObject targetButtonPrefab;

    [Header("Battle Log")]
    [SerializeField] TextMeshProUGUI battleLogText;
    [SerializeField] ScrollRect      battleLogScroll;

    [Header("Result")]
    [SerializeField] GameObject       resultPanel;
    [SerializeField] TextMeshProUGUI  resultText;

    // ── Runtime state ────────────────────────────────────────────────────
    SkillData _pendingSkill;
    readonly List<GameObject> _spawnedEnemyEntries = new();
    readonly List<GameObject> _spawnedSkillButtons = new();
    readonly List<GameObject> _spawnedTargetButtons = new();
    // Map enemy combatant → its UI entry for defeat greying.
    readonly Dictionary<Combatant, CanvasGroup> _enemyGroups = new();

    // ── Lifecycle ────────────────────────────────────────────────────────
    void OnEnable()
    {
        if (BattleManager.Instance == null) return;
        Subscribe();
    }

    void OnDisable()
    {
        Unsubscribe();
    }

    void Start()
    {
        battlePanel.SetActive(false);
        resultPanel.SetActive(false);
        targetPanel.SetActive(false);

        if (BattleManager.Instance != null)
            Subscribe();
    }

    void Subscribe()
    {
        var bm = BattleManager.Instance;
        bm.OnBattleStart      += HandleBattleStart;
        bm.OnBattleEnd        += HandleBattleEnd;
        bm.OnPlayerChooseSkill += HandlePlayerChooseSkill;
        bm.OnBattleLog        += HandleBattleLog;
        bm.OnCombatantHPChanged += HandleHPChanged;
        bm.OnCombatantDefeated  += HandleDefeated;
    }

    void Unsubscribe()
    {
        if (BattleManager.Instance == null) return;
        var bm = BattleManager.Instance;
        bm.OnBattleStart      -= HandleBattleStart;
        bm.OnBattleEnd        -= HandleBattleEnd;
        bm.OnPlayerChooseSkill -= HandlePlayerChooseSkill;
        bm.OnBattleLog        -= HandleBattleLog;
        bm.OnCombatantHPChanged -= HandleHPChanged;
        bm.OnCombatantDefeated  -= HandleDefeated;
    }

    // ── Event handlers ───────────────────────────────────────────────────

    void HandleBattleStart()
    {
        battlePanel.SetActive(true);
        resultPanel.SetActive(false);
        targetPanel.SetActive(false);
        battleLogText.text = "";

        RefreshPlayerBars();
        BuildEnemyList();
    }

    void HandleBattleEnd(bool victory)
    {
        ClearSkillButtons();
        targetPanel.SetActive(false);

        resultText.text = victory ? "Victory!" : "Defeat...";
        resultPanel.SetActive(true);

        // Hide entire battle panel after a delay.
        Invoke(nameof(HideBattlePanel), 2f);
    }

    void HideBattlePanel()
    {
        battlePanel.SetActive(false);
        resultPanel.SetActive(false);
        ClearEnemyList();
    }

    void HandlePlayerChooseSkill()
    {
        BuildSkillButtons();
    }

    void HandleBattleLog(string msg)
    {
        battleLogText.text += msg + "\n";
        // Auto-scroll to bottom.
        Canvas.ForceUpdateCanvases();
        if (battleLogScroll != null)
            battleLogScroll.verticalNormalizedPosition = 0f;
    }

    void HandleHPChanged(Combatant c)
    {
        if (c.IsPlayer)
            RefreshPlayerBars();
    }

    void HandleDefeated(Combatant c)
    {
        if (!c.IsPlayer && _enemyGroups.TryGetValue(c, out var cg))
            cg.alpha = 0.35f;
    }

    // ── Player bars ──────────────────────────────────────────────────────

    void RefreshPlayerBars()
    {
        var p = BattleManager.Instance.PlayerCombatant;
        if (p == null) return;

        playerHPBar.maxValue = p.MaxHP;
        playerHPBar.value    = p.CurrentHP;
        playerHPText.text    = $"{p.CurrentHP} / {p.MaxHP}";

        playerMPBar.maxValue = p.MaxMP;
        playerMPBar.value    = p.CurrentMP;
        playerMPText.text    = $"{p.CurrentMP} / {p.MaxMP}";
    }

    // ── Enemy list ───────────────────────────────────────────────────────

    void BuildEnemyList()
    {
        ClearEnemyList();
        foreach (var enemy in BattleManager.Instance.Enemies)
        {
            var go = Instantiate(enemyEntryPrefab, enemyContainer);
            _spawnedEnemyEntries.Add(go);

            // Try to set name label.
            var label = go.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = enemy.Name;

            // Try to set sprite.
            var img = go.GetComponentInChildren<Image>();
            if (img != null && enemy.Sprite != null)
                img.sprite = enemy.Sprite;

            // Track for defeat greying.
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();
            _enemyGroups[enemy] = cg;
        }
    }

    void ClearEnemyList()
    {
        foreach (var go in _spawnedEnemyEntries) Destroy(go);
        _spawnedEnemyEntries.Clear();
        _enemyGroups.Clear();
    }

    // ── Skill buttons ────────────────────────────────────────────────────

    void BuildSkillButtons()
    {
        ClearSkillButtons();

        // "Attack" button (always available, costs 0 MP).
        if (attackButton != null)
        {
            attackButton.gameObject.SetActive(true);
            attackButton.onClick.RemoveAllListeners();
            attackButton.onClick.AddListener(() => OnSkillSelected(null));
        }

        var player = BattleManager.Instance.PlayerCombatant;
        foreach (var skill in player.Skills)
        {
            if (skill == null) continue;
            var go = Instantiate(skillButtonPrefab, skillButtonContainer);
            _spawnedSkillButtons.Add(go);

            var label = go.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = $"{skill.skillName} ({skill.mpCost} MP)";

            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                bool canAfford = player.CurrentMP >= skill.mpCost;
                btn.interactable = canAfford;
                var captured = skill;
                btn.onClick.AddListener(() => OnSkillSelected(captured));
            }
        }
    }

    void ClearSkillButtons()
    {
        if (attackButton != null)
            attackButton.gameObject.SetActive(false);
        foreach (var go in _spawnedSkillButtons) Destroy(go);
        _spawnedSkillButtons.Clear();
    }

    void OnSkillSelected(SkillData skill)
    {
        _pendingSkill = skill;
        ClearSkillButtons();

        // If targetsAll or only one living enemy, auto-target.
        var living = GetLivingEnemies();
        if ((skill != null && skill.targetsAll) || living.Count <= 1)
        {
            var target = living.Count > 0 ? living[0] : null;
            BattleManager.Instance.SubmitPlayerChoice(_pendingSkill, target);
            return;
        }

        ShowTargetSelection(living);
    }

    // ── Target selection ─────────────────────────────────────────────────

    void ShowTargetSelection(List<Combatant> targets)
    {
        ClearTargetButtons();
        targetPanel.SetActive(true);

        foreach (var enemy in targets)
        {
            var go = Instantiate(targetButtonPrefab, targetButtonContainer);
            _spawnedTargetButtons.Add(go);

            var label = go.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = $"{enemy.Name} ({enemy.CurrentHP}/{enemy.MaxHP})";

            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                var captured = enemy;
                btn.onClick.AddListener(() =>
                {
                    targetPanel.SetActive(false);
                    ClearTargetButtons();
                    BattleManager.Instance.SubmitPlayerChoice(_pendingSkill, captured);
                });
            }
        }
    }

    void ClearTargetButtons()
    {
        foreach (var go in _spawnedTargetButtons) Destroy(go);
        _spawnedTargetButtons.Clear();
    }

    // ── Utility ──────────────────────────────────────────────────────────

    List<Combatant> GetLivingEnemies()
    {
        var list = new List<Combatant>();
        foreach (var e in BattleManager.Instance.Enemies)
            if (e.IsAlive) list.Add(e);
        return list;
    }
}
