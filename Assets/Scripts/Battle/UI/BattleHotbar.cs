using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Battle.UI
{
    /// <summary>
    /// 4-slot quick-access bar for skills/items. Supports keyboard shortcuts (1-4).
    /// Calls into BattleUIController's public target selection methods.
    /// </summary>
    public class BattleHotbar : MonoBehaviour
    {
        public enum SlotType
        {
            Empty,
            Skill,
            Item
        }

        [System.Serializable]
        public class HotbarSlot
        {
            public Button Button;
            public TMP_Text Label;
            [HideInInspector] public SlotType Type = SlotType.Empty;
            [HideInInspector] public int DataIndex = -1;
        }

        [Header("=== References ===")]
        [SerializeField] private BattleController _battleController;
        [SerializeField] private BattleUIController _uiController;

        [Header("=== Slots ===")]
        [SerializeField] private HotbarSlot[] _slots = new HotbarSlot[4];

        [Header("=== Keybinds ===")]
        [SerializeField] private KeyCode[] _keyCodes = new KeyCode[]
        {
            KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4
        };

        private void OnEnable()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                int index = i;
                if (_slots[i].Button != null)
                    _slots[i].Button.onClick.AddListener(() => OnSlotActivated(index));
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].Button != null)
                    _slots[i].Button.onClick.RemoveAllListeners();
            }
        }

        private void Update()
        {
            if (_battleController == null || !_battleController.IsPlayerInputEnabled) return;

            for (int i = 0; i < _keyCodes.Length && i < _slots.Length; i++)
            {
                if (Input.GetKeyDown(_keyCodes[i]))
                {
                    OnSlotActivated(i);
                }
            }
        }

        /// <summary>
        /// Assigns a skill to a hotbar slot.
        /// </summary>
        public void SetSlotSkill(int slotIndex, int skillIndex, string displayName)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Length) return;

            _slots[slotIndex].Type = SlotType.Skill;
            _slots[slotIndex].DataIndex = skillIndex;
            if (_slots[slotIndex].Label != null)
                _slots[slotIndex].Label.text = $"{slotIndex + 1}: {displayName}";
        }

        /// <summary>
        /// Assigns an item to a hotbar slot.
        /// </summary>
        public void SetSlotItem(int slotIndex, int itemIndex, string displayName)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Length) return;

            _slots[slotIndex].Type = SlotType.Item;
            _slots[slotIndex].DataIndex = itemIndex;
            if (_slots[slotIndex].Label != null)
                _slots[slotIndex].Label.text = $"{slotIndex + 1}: {displayName}";
        }

        /// <summary>
        /// Clears a hotbar slot.
        /// </summary>
        public void ClearSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Length) return;

            _slots[slotIndex].Type = SlotType.Empty;
            _slots[slotIndex].DataIndex = -1;
            if (_slots[slotIndex].Label != null)
                _slots[slotIndex].Label.text = $"{slotIndex + 1}: ---";
        }

        private void OnSlotActivated(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Length) return;
            if (_uiController == null || _battleController == null) return;
            if (!_battleController.IsPlayerInputEnabled) return;

            HotbarSlot slot = _slots[slotIndex];
            if (slot.Type == SlotType.Empty || slot.DataIndex < 0) return;

            switch (slot.Type)
            {
                case SlotType.Skill:
                    _uiController.RequestTargetSelectionForSkill(slot.DataIndex);
                    break;
                case SlotType.Item:
                    _uiController.RequestTargetSelectionForItem(slot.DataIndex);
                    break;
            }
        }
    }
}
