using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Battle.UI
{
    /// <summary>
    /// Shows loot, XP, gold, and bonus conditions after victory.
    /// Subscribes to BattleRewardCalculator.OnRewardsCalculated.
    /// </summary>
    public class RewardDisplayUI : MonoBehaviour
    {
        [Header("=== References ===")]
        [SerializeField] private BattleRewardCalculator _rewardCalculator;

        [Header("=== Panel ===")]
        [SerializeField] private GameObject _rewardPanel;

        [Header("=== Text Fields ===")]
        [SerializeField] private TMP_Text _xpText;
        [SerializeField] private TMP_Text _goldText;
        [SerializeField] private TMP_Text _multiplierText;
        [SerializeField] private TMP_Text _gradeText;

        [Header("=== Loot Display ===")]
        [SerializeField] private Transform _lootContainer;
        [SerializeField] private GameObject _lootEntryPrefab;

        [Header("=== Bonus Display ===")]
        [SerializeField] private Transform _bonusContainer;
        [SerializeField] private GameObject _bonusEntryPrefab;

        private void Awake()
        {
            if (_rewardPanel != null)
                _rewardPanel.SetActive(false);
        }

        private void OnEnable()
        {
            if (_rewardCalculator != null)
                _rewardCalculator.OnRewardsCalculated += ShowRewards;
        }

        private void OnDisable()
        {
            if (_rewardCalculator != null)
                _rewardCalculator.OnRewardsCalculated -= ShowRewards;
        }

        private void ShowRewards(BattleRewards rewards)
        {
            if (_rewardPanel == null || rewards == null) return;

            // Grade
            if (_gradeText != null)
            {
                string gradeColor = GetGradeColor(rewards.Grade);
                _gradeText.text = $"<color={gradeColor}><size=150%>{rewards.Grade}</size></color>";
            }

            // XP breakdown
            if (_xpText != null)
            {
                _xpText.text = $"XP: {rewards.FinalXP} (Base {rewards.BaseXP} x{rewards.TotalMultiplier:F2} + {rewards.BonusXP} bonus)";
            }

            // Gold
            if (_goldText != null)
            {
                _goldText.text = $"Gold: {rewards.GoldEarned}";
            }

            // Multiplier
            if (_multiplierText != null)
            {
                _multiplierText.text = $"Multiplier: x{rewards.TotalMultiplier:F2} (Grade x{rewards.GradeMultiplier:F2} * Streak x{rewards.StreakMultiplier:F2}) | Streak: {rewards.WinStreak}";
            }

            // Loot entries
            if (_lootContainer != null)
            {
                // Clear existing
                for (int i = _lootContainer.childCount - 1; i >= 0; i--)
                    Destroy(_lootContainer.GetChild(i).gameObject);

                if (rewards.Loot != null)
                {
                    foreach (var drop in rewards.Loot)
                    {
                        if (_lootEntryPrefab != null)
                        {
                            GameObject entry = Instantiate(_lootEntryPrefab, _lootContainer);
                            TMP_Text label = entry.GetComponentInChildren<TMP_Text>();
                            if (label != null)
                            {
                                string itemName = drop.ItemDataRef != null ? drop.ItemDataRef.itemName : $"[{drop.Rarity} Item]";
                                string color = GetRarityColor(drop.Rarity);
                                label.text = $"<color={color}>{itemName} x{drop.Quantity}</color>";
                            }
                        }
                        else
                        {
                            // Fallback: create text object
                            var go = new GameObject("LootEntry");
                            go.transform.SetParent(_lootContainer, false);
                            var text = go.AddComponent<TMP_Text>();
                            string itemName = drop.ItemDataRef != null ? drop.ItemDataRef.itemName : $"[{drop.Rarity} Item]";
                            string color = GetRarityColor(drop.Rarity);
                            text.text = $"<color={color}>{itemName} x{drop.Quantity}</color>";
                            text.fontSize = 14;
                        }
                    }
                }
            }

            // Bonus condition entries
            if (_bonusContainer != null)
            {
                // Clear existing
                for (int i = _bonusContainer.childCount - 1; i >= 0; i--)
                    Destroy(_bonusContainer.GetChild(i).gameObject);

                if (rewards.BonusConditions != null)
                {
                    foreach (var bonus in rewards.BonusConditions)
                    {
                        if (_bonusEntryPrefab != null)
                        {
                            GameObject entry = Instantiate(_bonusEntryPrefab, _bonusContainer);
                            TMP_Text label = entry.GetComponentInChildren<TMP_Text>();
                            if (label != null)
                            {
                                string checkmark = bonus.Completed ? "<color=green>[v]</color>" : "<color=red>[x]</color>";
                                string xpStr = bonus.Completed ? $" +{bonus.BonusXP} XP" : "";
                                label.text = $"{checkmark} {bonus.Description}{xpStr}";
                            }
                        }
                        else
                        {
                            var go = new GameObject("BonusEntry");
                            go.transform.SetParent(_bonusContainer, false);
                            var text = go.AddComponent<TMP_Text>();
                            string checkmark = bonus.Completed ? "<color=green>[v]</color>" : "<color=red>[x]</color>";
                            string xpStr = bonus.Completed ? $" +{bonus.BonusXP} XP" : "";
                            text.text = $"{checkmark} {bonus.Description}{xpStr}";
                            text.fontSize = 14;
                        }
                    }
                }
            }

            _rewardPanel.SetActive(true);
        }

        private static string GetGradeColor(BattleGrade grade)
        {
            switch (grade)
            {
                case BattleGrade.S: return "#ffdd44";
                case BattleGrade.A: return "#44ff44";
                case BattleGrade.B: return "#4488ff";
                case BattleGrade.C: return "#aaaaaa";
                case BattleGrade.D: return "#cc4444";
                default: return "#cccccc";
            }
        }

        private static string GetRarityColor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Common:    return "#cccccc";
                case ItemRarity.Uncommon:  return "#44cc44";
                case ItemRarity.Rare:      return "#4488ff";
                case ItemRarity.Epic:      return "#aa44ff";
                case ItemRarity.Legendary: return "#ffdd44";
                default:                   return "#cccccc";
            }
        }
    }
}
