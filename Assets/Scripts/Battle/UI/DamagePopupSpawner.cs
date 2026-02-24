using System.Collections.Generic;
using UnityEngine;

namespace Battle.UI
{
    /// <summary>
    /// Subscribes to BattleController combat events and spawns DamagePopup instances
    /// at unit anchor positions. Standalone — does not modify BattleUIController.
    /// </summary>
    public class DamagePopupSpawner : MonoBehaviour
    {
        [Header("=== References ===")]
        [SerializeField] private BattleController _battleController;
        [SerializeField] private DamagePopup _popupPrefab;
        [SerializeField] private Transform _parentCanvas;

        [Header("=== Anchor Positions (one RectTransform per unit slot) ===")]
        [SerializeField] private RectTransform[] _playerPopupAnchors;
        [SerializeField] private RectTransform[] _enemyPopupAnchors;

        // Track whether the next OnDamageTaken should display as critical
        private bool _nextPopupIsCrit;

        // Color palette
        private static readonly Color ColorPhysical = Color.white;
        private static readonly Color ColorHealing = new Color(0.2f, 0.9f, 0.3f);
        private static readonly Color ColorCritical = new Color(1f, 0.2f, 0.2f);
        private static readonly Color ColorBlocked = new Color(0.6f, 0.6f, 0.6f);
        private static readonly Color ColorMiss = new Color(0.3f, 0.9f, 0.9f);
        private static readonly Color ColorPoison = new Color(0.3f, 0.8f, 0.3f);
        private static readonly Color ColorBurn = new Color(1f, 0.4f, 0.2f);

        private void OnEnable()
        {
            if (_battleController == null) return;

            _battleController.OnDamageTaken += HandleDamageTaken;
            _battleController.OnHealApplied += HandleHealApplied;
            _battleController.OnCriticalHit += HandleCriticalHit;
            _battleController.OnDodgeSuccess += HandleDodgeSuccess;
            _battleController.OnBlockSuccess += HandleBlockSuccess;
            _battleController.OnStatusEffectTick += HandleStatusEffectTick;
        }

        private void OnDisable()
        {
            if (_battleController == null) return;

            _battleController.OnDamageTaken -= HandleDamageTaken;
            _battleController.OnHealApplied -= HandleHealApplied;
            _battleController.OnCriticalHit -= HandleCriticalHit;
            _battleController.OnDodgeSuccess -= HandleDodgeSuccess;
            _battleController.OnBlockSuccess -= HandleBlockSuccess;
            _battleController.OnStatusEffectTick -= HandleStatusEffectTick;
        }

        private void HandleCriticalHit(IBattleUnit attacker, IBattleUnit target)
        {
            _nextPopupIsCrit = true;
        }

        private void HandleDamageTaken(IBattleUnit target, IBattleUnit attacker, int damage)
        {
            bool isCrit = _nextPopupIsCrit;
            _nextPopupIsCrit = false;

            Color color = isCrit ? ColorCritical : ColorPhysical;
            SpawnPopup(target, damage.ToString(), color, isCrit);
        }

        private void HandleHealApplied(IBattleUnit unit, int amount)
        {
            SpawnPopup(unit, $"+{amount}", ColorHealing, false);
        }

        private void HandleDodgeSuccess(IBattleUnit dodger, IBattleUnit attacker)
        {
            SpawnPopup(dodger, "MISS", ColorMiss, false);
        }

        private void HandleBlockSuccess(IBattleUnit blocker, IBattleUnit attacker, int reducedDamage)
        {
            SpawnPopup(blocker, $"BLOCKED ({reducedDamage})", ColorBlocked, false);
        }

        private void HandleStatusEffectTick(IBattleUnit unit, StatusEffectType type, int damage)
        {
            Color color = type == StatusEffectType.Poison ? ColorPoison :
                          type == StatusEffectType.Burn ? ColorBurn : ColorPhysical;
            SpawnPopup(unit, damage.ToString(), color, false);
        }

        private void SpawnPopup(IBattleUnit unit, string text, Color color, bool isCrit)
        {
            if (_popupPrefab == null || _parentCanvas == null) return;

            RectTransform anchor = FindUnitAnchor(unit);
            if (anchor == null) return;

            DamagePopup popup = Instantiate(_popupPrefab, _parentCanvas);
            RectTransform rt = popup.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.position = anchor.position;
                // Add small random horizontal offset to avoid stacking
                rt.localPosition += new Vector3(Random.Range(-15f, 15f), Random.Range(-5f, 5f), 0f);
            }

            popup.Init(text, color, isCrit);
        }

        private RectTransform FindUnitAnchor(IBattleUnit unit)
        {
            // Check player party
            IReadOnlyList<IBattleUnit> players = _battleController.PlayerParty;
            for (int i = 0; i < players.Count && i < _playerPopupAnchors.Length; i++)
            {
                if (players[i] == unit)
                    return _playerPopupAnchors[i];
            }

            // Check enemy units
            IReadOnlyList<IBattleUnit> enemies = _battleController.EnemyUnits;
            for (int i = 0; i < enemies.Count && i < _enemyPopupAnchors.Length; i++)
            {
                if (enemies[i] == unit)
                    return _enemyPopupAnchors[i];
            }

            return null;
        }
    }
}
