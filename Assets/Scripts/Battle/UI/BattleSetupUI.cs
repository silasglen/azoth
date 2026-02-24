using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Battle.UI
{
    /// <summary>
    /// Pre-battle setup panel for BattleTest scene.
    /// Allows selecting which AI patterns to fight against.
    /// Builds its own UI programmatically — just attach to any GameObject in a Canvas.
    /// </summary>
    public class BattleSetupUI : MonoBehaviour
    {
        [Header("=== References ===")]
        [SerializeField] private BattleController _battleController;
        [SerializeField] private List<MonoBehaviour> _playerUnits = new List<MonoBehaviour>();

        [Header("=== Layout ===")]
        [SerializeField] private int _maxEnemies = 3;

        public event Action OnBattleStarted;

        private struct EnemyTemplate
        {
            public string Name;
            public string Desc;
            public Type AIType;
            public ElementType Element;
            public int HP, ATK, DEF;
        }

        private List<EnemyTemplate> _templates;
        private List<int> _selected = new List<int>();
        private GameObject _enemyContainer;

        // UI elements
        private GameObject _panel;
        private TMP_Text _selectedText;
        private TMP_Text _titleText;
        private Button _startButton;
        private List<Button> _aiButtons = new List<Button>();

        private void Awake()
        {
            InitTemplates();
            BuildUI();
        }

        private void OnEnable()
        {
            if (_battleController != null)
            {
                _battleController.OnBattleWon += HandleBattleEnd;
                _battleController.OnBattleLost += HandleBattleEnd;
            }
        }

        private void OnDisable()
        {
            if (_battleController != null)
            {
                _battleController.OnBattleWon -= HandleBattleEnd;
                _battleController.OnBattleLost -= HandleBattleEnd;
            }
        }

        private void HandleBattleEnd()
        {
            // Show setup panel again after a short delay
            Invoke(nameof(ShowPanel), 2f);
        }

        public void ShowPanel()
        {
            _selected.Clear();
            RefreshSelected();
            if (_panel != null) _panel.SetActive(true);
        }

        private void InitTemplates()
        {
            _templates = new List<EnemyTemplate>
            {
                new EnemyTemplate { Name = "Berserker",         Desc = "1  - Enrages at low HP, rising aggression",  AIType = typeof(BerserkerAI),        Element = ElementType.Ignis,  HP = 120, ATK = 25, DEF = 5 },
                new EnemyTemplate { Name = "Tactician",         Desc = "2  - Opens with debuff, exploits weakness",  AIType = typeof(TacticianAI),        Element = ElementType.Ventus, HP = 100, ATK = 20, DEF = 10 },
                new EnemyTemplate { Name = "Bodyguard",         Desc = "3  - Redirects attacks from wounded allies",  AIType = typeof(BodyguardAI),        Element = ElementType.Terra,  HP = 150, ATK = 15, DEF = 15 },
                new EnemyTemplate { Name = "Sniper",            Desc = "4  - Targets lowest DEF, high damage",       AIType = typeof(SniperAI),           Element = ElementType.Ventus, HP = 70,  ATK = 30, DEF = 3 },
                new EnemyTemplate { Name = "Healer Priest",     Desc = "5  - Heals wounded allies first",            AIType = typeof(HealerPriestAI),     Element = ElementType.Lux,    HP = 80,  ATK = 12, DEF = 8 },
                new EnemyTemplate { Name = "Glass Cannon",      Desc = "6  - Always max damage skill",              AIType = typeof(GlassCannonAI),      Element = ElementType.Lux,    HP = 50,  ATK = 35, DEF = 2 },
                new EnemyTemplate { Name = "Debuffer",          Desc = "7  - AtkDown/DefDown rotation cycle",        AIType = typeof(DebufferAI),         Element = ElementType.Umbra,  HP = 90,  ATK = 18, DEF = 8 },
                new EnemyTemplate { Name = "Vampire",           Desc = "8  - Lifesteal, targets highest HP",         AIType = typeof(VampireAI),          Element = ElementType.Umbra,  HP = 100, ATK = 22, DEF = 6 },
                new EnemyTemplate { Name = "Martyr",            Desc = "9  - On death: heals & buffs all allies",    AIType = typeof(MartyrAI),           Element = ElementType.Lux,    HP = 60,  ATK = 10, DEF = 5 },
                new EnemyTemplate { Name = "Mimic",             Desc = "10 - Copies last player skill",             AIType = typeof(MimicAI),            Element = ElementType.None,   HP = 90,  ATK = 20, DEF = 7 },
                new EnemyTemplate { Name = "Coward",            Desc = "11 - Flees at 30% HP",                      AIType = typeof(CowardAI),           Element = ElementType.None,   HP = 80,  ATK = 18, DEF = 4 },
                new EnemyTemplate { Name = "Avenger",           Desc = "12 - +ATK per dead ally, rage skills",       AIType = typeof(AvengerAI),          Element = ElementType.Ignis,  HP = 110, ATK = 20, DEF = 8 },
                new EnemyTemplate { Name = "Ritualist",         Desc = "13 - 3-turn charge then AoE nuke",          AIType = typeof(RitualistAI),        Element = ElementType.Umbra,  HP = 100, ATK = 28, DEF = 5 },
                new EnemyTemplate { Name = "Swarm Drone",       Desc = "14 - Scales with living Swarm allies",      AIType = typeof(SwarmDroneAI),       Element = ElementType.None,   HP = 40,  ATK = 12, DEF = 3 },
                new EnemyTemplate { Name = "Saboteur",          Desc = "15 - Burns MP, destroys items",             AIType = typeof(SaboteurAI),         Element = ElementType.Umbra,  HP = 85,  ATK = 16, DEF = 6 },
                new EnemyTemplate { Name = "Elemental Shifter", Desc = "16 - Adapts element to resist player",      AIType = typeof(ElementalShifterAI), Element = ElementType.Ignis,  HP = 100, ATK = 22, DEF = 7 },
                new EnemyTemplate { Name = "Commander",         Desc = "17 - Aura buffs, rally/debuff rotation",    AIType = typeof(CommanderAI),        Element = ElementType.None,   HP = 130, ATK = 18, DEF = 12 },
                new EnemyTemplate { Name = "Basic Enemy",       Desc = "Default aggression-based TestBattleUnit",    AIType = typeof(TestBattleUnit),     Element = ElementType.None,   HP = 100, ATK = 20, DEF = 5 },
            };
        }

        // =============================================
        // UI CONSTRUCTION
        // =============================================

        private void BuildUI()
        {
            // Main panel — full screen dark overlay
            _panel = CreatePanel("SetupPanel", transform);
            var panelRect = _panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var panelImg = _panel.AddComponent<Image>();
            panelImg.color = new Color(0.08f, 0.08f, 0.12f, 0.97f);

            // Layout
            var mainLayout = _panel.AddComponent<VerticalLayoutGroup>();
            mainLayout.padding = new RectOffset(30, 30, 20, 20);
            mainLayout.spacing = 8;
            mainLayout.childAlignment = TextAnchor.UpperCenter;
            mainLayout.childForceExpandWidth = true;
            mainLayout.childForceExpandHeight = false;

            var fitter = _panel.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            // Title
            _titleText = CreateText("Title", _panel.transform, "BATTLE SETUP", 28, TextAlignmentOptions.Center);
            _titleText.color = new Color(1f, 0.85f, 0.4f);
            SetPreferredHeight(_titleText.gameObject, 40);

            // Subtitle
            var subtitle = CreateText("Subtitle", _panel.transform,
                $"Select 1-{_maxEnemies} enemies, then press Start Battle", 16, TextAlignmentOptions.Center);
            subtitle.color = new Color(0.7f, 0.7f, 0.7f);
            SetPreferredHeight(subtitle.gameObject, 25);

            // Selected enemies display
            _selectedText = CreateText("SelectedText", _panel.transform, "Selected: (none)", 18, TextAlignmentOptions.Center);
            _selectedText.color = new Color(0.5f, 1f, 0.7f);
            SetPreferredHeight(_selectedText.gameObject, 30);

            // Scroll view for AI type buttons
            var scrollObj = CreateScrollView("AIScrollView", _panel.transform);
            var scrollRect = scrollObj.GetComponent<RectTransform>();
            var layoutElem = scrollObj.AddComponent<LayoutElement>();
            layoutElem.flexibleHeight = 1;
            layoutElem.minHeight = 200;

            var content = scrollObj.transform.Find("Viewport/Content");
            var contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 4;
            contentLayout.padding = new RectOffset(5, 5, 5, 5);
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;

            var contentFitter = content.gameObject.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Create a button for each AI template
            for (int i = 0; i < _templates.Count; i++)
            {
                int idx = i; // closure capture
                var btn = CreateAIButton(_templates[i], content);
                btn.onClick.AddListener(() => ToggleEnemy(idx));
                _aiButtons.Add(btn);
            }

            // Preset buttons row
            var presetRow = CreatePanel("PresetRow", _panel.transform);
            var presetLayout = presetRow.AddComponent<HorizontalLayoutGroup>();
            presetLayout.spacing = 8;
            presetLayout.childAlignment = TextAnchor.MiddleCenter;
            presetLayout.childForceExpandWidth = true;
            presetLayout.childForceExpandHeight = false;
            SetPreferredHeight(presetRow, 35);
            var presetBg = presetRow.GetComponent<Image>();
            if (presetBg == null) presetBg = presetRow.AddComponent<Image>();
            presetBg.color = Color.clear;

            CreatePresetButton("Mixed",       presetRow.transform, new[] { 0, 4, 3 });     // Berserker + Healer + Sniper
            CreatePresetButton("Tank Line",   presetRow.transform, new[] { 2, 16, 0 });    // Bodyguard + Commander + Berserker
            CreatePresetButton("Swarm x3",    presetRow.transform, new[] { 13, 13, 13 });  // 3x Swarm Drone
            CreatePresetButton("Boss Fight",  presetRow.transform, new[] { 16, 12, 4 });   // Commander + Ritualist + Healer
            CreatePresetButton("Chaos",       presetRow.transform, new[] { 14, 9, 15 });   // Saboteur + Mimic + Shifter

            // Bottom buttons row
            var bottomRow = CreatePanel("BottomRow", _panel.transform);
            var bottomLayout = bottomRow.AddComponent<HorizontalLayoutGroup>();
            bottomLayout.spacing = 20;
            bottomLayout.childAlignment = TextAnchor.MiddleCenter;
            bottomLayout.childForceExpandWidth = true;
            bottomLayout.childForceExpandHeight = false;
            SetPreferredHeight(bottomRow, 45);
            var bottomBg = bottomRow.GetComponent<Image>();
            if (bottomBg == null) bottomBg = bottomRow.AddComponent<Image>();
            bottomBg.color = Color.clear;

            CreateActionButton("Clear", bottomRow.transform, new Color(0.8f, 0.3f, 0.3f), ClearSelection);
            _startButton = CreateActionButton("START BATTLE", bottomRow.transform, new Color(0.2f, 0.7f, 0.3f), StartBattle);

            // Start hidden — BattleSceneBootstrap.ShowPanel() or encounter flow will show when appropriate
            _panel.SetActive(false);
        }

        private Button CreateAIButton(EnemyTemplate template, Transform parent)
        {
            var go = new GameObject(template.Name + "Btn");
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();

            var img = go.AddComponent<Image>();
            img.color = new Color(0.15f, 0.15f, 0.22f);

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = new Color(0.15f, 0.15f, 0.22f);
            colors.highlightedColor = new Color(0.25f, 0.25f, 0.35f);
            colors.pressedColor = new Color(0.1f, 0.3f, 0.15f);
            colors.selectedColor = new Color(0.15f, 0.15f, 0.22f);
            btn.colors = colors;

            var layout = go.AddComponent<LayoutElement>();
            layout.preferredHeight = 32;
            layout.minHeight = 32;

            // Horizontal layout for name + desc
            var hLayout = go.AddComponent<HorizontalLayoutGroup>();
            hLayout.padding = new RectOffset(10, 10, 2, 2);
            hLayout.spacing = 15;
            hLayout.childAlignment = TextAnchor.MiddleLeft;
            hLayout.childForceExpandWidth = false;
            hLayout.childForceExpandHeight = false;

            var nameText = CreateText("Name", go.transform, template.Name, 16, TextAlignmentOptions.Left);
            nameText.color = Color.white;
            nameText.fontStyle = FontStyles.Bold;
            var nameLayout = nameText.gameObject.AddComponent<LayoutElement>();
            nameLayout.preferredWidth = 180;
            nameLayout.minWidth = 180;

            string elemStr = template.Element != ElementType.None
                ? $"<color={GetElementColorHex(template.Element)}>[{template.Element}]</color> "
                : "";
            string statsStr = $"HP:{template.HP} ATK:{template.ATK} DEF:{template.DEF}";
            var descText = CreateText("Desc", go.transform,
                $"{elemStr}{template.Desc}  <color=#888888>({statsStr})</color>",
                13, TextAlignmentOptions.Left);
            descText.color = new Color(0.65f, 0.65f, 0.7f);
            var descLayout = descText.gameObject.AddComponent<LayoutElement>();
            descLayout.flexibleWidth = 1;

            return btn;
        }

        private void CreatePresetButton(string label, Transform parent, int[] indices)
        {
            var btn = CreateActionButton(label, parent, new Color(0.2f, 0.2f, 0.35f), () => LoadPreset(indices));
            var layout = btn.gameObject.GetComponent<LayoutElement>();
            if (layout == null) layout = btn.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 30;
        }

        private Button CreateActionButton(string label, Transform parent, Color bgColor, Action onClick)
        {
            var go = new GameObject(label + "Btn");
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.color = bgColor;

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = bgColor;
            colors.highlightedColor = bgColor * 1.2f;
            colors.pressedColor = bgColor * 0.8f;
            btn.colors = colors;
            btn.onClick.AddListener(() => onClick());

            var layout = go.AddComponent<LayoutElement>();
            layout.preferredHeight = 40;
            layout.flexibleWidth = 1;

            var text = CreateText("Label", go.transform, label, 18, TextAlignmentOptions.Center);
            text.color = Color.white;
            text.fontStyle = FontStyles.Bold;
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return btn;
        }

        // =============================================
        // SELECTION LOGIC
        // =============================================

        private void ToggleEnemy(int templateIndex)
        {
            // If already at max, only allow removal
            int existingIdx = _selected.IndexOf(templateIndex);
            if (existingIdx >= 0)
            {
                _selected.RemoveAt(existingIdx);
            }
            else if (_selected.Count < _maxEnemies)
            {
                _selected.Add(templateIndex);
            }
            RefreshSelected();
        }

        private void LoadPreset(int[] indices)
        {
            _selected.Clear();
            foreach (int idx in indices)
            {
                if (_selected.Count < _maxEnemies && idx >= 0 && idx < _templates.Count)
                    _selected.Add(idx);
            }
            RefreshSelected();
        }

        private void ClearSelection()
        {
            _selected.Clear();
            RefreshSelected();
        }

        private void RefreshSelected()
        {
            // Update selected text
            if (_selected.Count == 0)
            {
                _selectedText.text = "Selected: <color=#888>(none)</color>";
            }
            else
            {
                var names = new List<string>();
                foreach (int idx in _selected)
                    names.Add(_templates[idx].Name);
                _selectedText.text = $"Selected: <b>{string.Join("</b>, <b>", names)}</b>";
            }

            _startButton.interactable = _selected.Count > 0;

            // Update button visuals
            for (int i = 0; i < _aiButtons.Count; i++)
            {
                int count = 0;
                foreach (int sel in _selected)
                    if (sel == i) count++;

                var img = _aiButtons[i].GetComponent<Image>();
                if (count > 0)
                    img.color = new Color(0.1f, 0.3f, 0.15f);
                else
                    img.color = new Color(0.15f, 0.15f, 0.22f);

                // Update button name to show count if selected multiple times
                var nameText = _aiButtons[i].transform.Find("Name")?.GetComponent<TMP_Text>();
                if (nameText != null)
                {
                    string baseName = _templates[i].Name;
                    nameText.text = count > 1 ? $"{baseName} x{count}" : (count == 1 ? $"* {baseName}" : baseName);
                }
            }
        }

        // =============================================
        // BATTLE START
        // =============================================

        private void StartBattle()
        {
            if (_selected.Count == 0) return;
            if (_battleController == null)
            {
                Debug.LogError("[BattleSetupUI] BattleController not assigned!");
                return;
            }

            // Clean up previous dynamic enemies
            if (_enemyContainer != null)
                Destroy(_enemyContainer);
            _enemyContainer = new GameObject("DynamicEnemies");

            // Create enemy units from selected templates
            var enemies = new List<IBattleUnit>();
            foreach (int idx in _selected)
            {
                var t = _templates[idx];
                var go = new GameObject(t.Name);
                go.transform.SetParent(_enemyContainer.transform);

                var ai = go.AddComponent(t.AIType) as EnemyAIBase;
                if (ai != null)
                {
                    ai.Configure(t.Name, UnitType.Knight, t.Element,
                        t.HP, t.ATK, t.DEF, 0.1f, 0, _battleController);
                    enemies.Add(ai);
                }
            }

            // Collect player party from assigned units
            var players = new List<IBattleUnit>();
            foreach (var mb in _playerUnits)
            {
                if (mb != null && mb is IBattleUnit unit)
                    players.Add(unit);
            }

            if (players.Count == 0)
            {
                Debug.LogError("[BattleSetupUI] No player units assigned!");
                Destroy(_enemyContainer);
                return;
            }

            // Hide setup panel
            _panel.SetActive(false);

            // Reset player units for new battle
            foreach (var player in players)
            {
                if (!player.IsAlive)
                    player.Revive(player.MaxHP);
                player.CurrentHP = player.MaxHP;
                player.CurrentResource = player.MaxResource;
            }

            // Start battle
            _battleController.InitBattle(players, enemies);
            OnBattleStarted?.Invoke();
        }

        // =============================================
        // UI FACTORY HELPERS
        // =============================================

        private static GameObject CreatePanel(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static TMP_Text CreateText(string name, Transform parent, string content, int fontSize, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var text = go.AddComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.richText = true;
            text.overflowMode = TextOverflowModes.Ellipsis;

            return text;
        }

        private static void SetPreferredHeight(GameObject go, float height)
        {
            var layout = go.GetComponent<LayoutElement>();
            if (layout == null) layout = go.AddComponent<LayoutElement>();
            layout.preferredHeight = height;
            layout.minHeight = height;
        }

        private static GameObject CreateScrollView(string name, Transform parent)
        {
            // ScrollView root
            var scrollGO = new GameObject(name, typeof(RectTransform));
            scrollGO.transform.SetParent(parent, false);

            var scrollRect = scrollGO.AddComponent<ScrollRect>();
            var scrollImg = scrollGO.AddComponent<Image>();
            scrollImg.color = new Color(0.1f, 0.1f, 0.14f);

            // Viewport
            var viewport = new GameObject("Viewport", typeof(RectTransform));
            viewport.transform.SetParent(scrollGO.transform, false);
            var vpRect = viewport.GetComponent<RectTransform>();
            vpRect.anchorMin = Vector2.zero;
            vpRect.anchorMax = Vector2.one;
            vpRect.offsetMin = Vector2.zero;
            vpRect.offsetMax = Vector2.zero;

            var mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            var vpImg = viewport.AddComponent<Image>();
            vpImg.color = Color.white;

            // Content
            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.offsetMin = new Vector2(0, 0);
            contentRect.offsetMax = new Vector2(0, 0);

            // Wire scroll rect
            scrollRect.viewport = vpRect;
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 25;

            return scrollGO;
        }

        private static string GetElementColorHex(ElementType element)
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
    }
}
