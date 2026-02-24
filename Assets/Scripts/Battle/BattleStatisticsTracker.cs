using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Subscribes to BattleController combat events and accumulates statistics
    /// for the victory screen. Calculates a battle grade (S/A/B/C).
    /// </summary>
    public class BattleStatisticsTracker : MonoBehaviour
    {
        [SerializeField] private BattleController _battleController;

        // Accumulated stats
        public int TotalDamageDealt { get; private set; }
        public int TotalDamageTaken { get; private set; }
        public int TotalHealingDone { get; private set; }
        public int EnemiesKilled { get; private set; }
        public int DodgesSucceeded { get; private set; }
        public int BlocksSucceeded { get; private set; }
        public int CriticalHitsLanded { get; private set; }
        public int TurnsElapsed { get; private set; }
        public int PlayerDeaths { get; private set; }
        public int SkillsUsed { get; private set; }
        public int ItemsUsed { get; private set; }
        public int StatusEffectsApplied { get; private set; }
        public int StatusEffectsCured { get; private set; }
        public int WeaknessExploits { get; private set; }
        public float BattleDuration { get; private set; }

        // Win streak persists across battles
        public static int WinStreak { get; set; }

        private float _battleStartTime;

        private void OnEnable()
        {
            if (_battleController == null) return;

            _battleController.OnBattleStart += HandleBattleStart;
            _battleController.OnDamageTaken += HandleDamageTaken;
            _battleController.OnHealApplied += HandleHealApplied;
            _battleController.OnUnitDied += HandleUnitDied;
            _battleController.OnDodgeSuccess += HandleDodgeSuccess;
            _battleController.OnBlockSuccess += HandleBlockSuccess;
            _battleController.OnCriticalHit += HandleCriticalHit;
            _battleController.OnTurnEnd += HandleTurnEnd;
            _battleController.OnPlayerActionSelected += HandlePlayerAction;
            _battleController.OnBattleWon += HandleBattleWon;
            _battleController.OnBattleLost += HandleBattleLost;
            _battleController.OnStatusEffectApplied += HandleStatusEffectApplied;
            _battleController.OnStatusEffectExpired += HandleStatusEffectCured;
            _battleController.OnDamageTaken += HandleDamageTakenForWeakness;
            _battleController.OnBattleRetreated += HandleBattleRetreated;
        }

        private void OnDisable()
        {
            if (_battleController == null) return;

            _battleController.OnBattleStart -= HandleBattleStart;
            _battleController.OnDamageTaken -= HandleDamageTaken;
            _battleController.OnHealApplied -= HandleHealApplied;
            _battleController.OnUnitDied -= HandleUnitDied;
            _battleController.OnDodgeSuccess -= HandleDodgeSuccess;
            _battleController.OnBlockSuccess -= HandleBlockSuccess;
            _battleController.OnCriticalHit -= HandleCriticalHit;
            _battleController.OnTurnEnd -= HandleTurnEnd;
            _battleController.OnPlayerActionSelected -= HandlePlayerAction;
            _battleController.OnBattleWon -= HandleBattleWon;
            _battleController.OnBattleLost -= HandleBattleLost;
            _battleController.OnStatusEffectApplied -= HandleStatusEffectApplied;
            _battleController.OnStatusEffectExpired -= HandleStatusEffectCured;
            _battleController.OnDamageTaken -= HandleDamageTakenForWeakness;
            _battleController.OnBattleRetreated -= HandleBattleRetreated;
        }

        public void Reset()
        {
            TotalDamageDealt = 0;
            TotalDamageTaken = 0;
            TotalHealingDone = 0;
            EnemiesKilled = 0;
            DodgesSucceeded = 0;
            BlocksSucceeded = 0;
            CriticalHitsLanded = 0;
            TurnsElapsed = 0;
            PlayerDeaths = 0;
            SkillsUsed = 0;
            ItemsUsed = 0;
            StatusEffectsApplied = 0;
            StatusEffectsCured = 0;
            WeaknessExploits = 0;
            BattleDuration = 0f;
        }

        /// <summary>
        /// Calculates a battle grade based on performance.
        /// S: No deaths, quick, crits   A: No deaths   B: 1 death   C: 2+ deaths
        /// </summary>
        public string CalculateGrade()
        {
            if (PlayerDeaths == 0 && TurnsElapsed <= 5 && CriticalHitsLanded >= 2)
                return "S";
            if (PlayerDeaths == 0)
                return "A";
            if (PlayerDeaths <= 1)
                return "B";
            return "C";
        }

        private void HandleBattleStart()
        {
            Reset();
            _battleStartTime = Time.time;
        }

        private void HandleDamageTaken(IBattleUnit target, IBattleUnit attacker, int damage)
        {
            // Determine if the target is a player or enemy
            bool targetIsPlayer = false;
            for (int i = 0; i < _battleController.PlayerParty.Count; i++)
            {
                if (_battleController.PlayerParty[i] == target)
                {
                    targetIsPlayer = true;
                    break;
                }
            }

            if (targetIsPlayer)
                TotalDamageTaken += damage;
            else
                TotalDamageDealt += damage;
        }

        private void HandleHealApplied(IBattleUnit unit, int amount)
        {
            TotalHealingDone += amount;
        }

        private void HandleUnitDied(IBattleUnit dead, IBattleUnit killer)
        {
            bool deadIsPlayer = false;
            for (int i = 0; i < _battleController.PlayerParty.Count; i++)
            {
                if (_battleController.PlayerParty[i] == dead)
                {
                    deadIsPlayer = true;
                    break;
                }
            }

            if (deadIsPlayer)
                PlayerDeaths++;
            else
                EnemiesKilled++;
        }

        private void HandleDodgeSuccess(IBattleUnit dodger, IBattleUnit attacker)
        {
            DodgesSucceeded++;
        }

        private void HandleBlockSuccess(IBattleUnit blocker, IBattleUnit attacker, int reduced)
        {
            BlocksSucceeded++;
        }

        private void HandleCriticalHit(IBattleUnit attacker, IBattleUnit target)
        {
            // Only count player crits
            for (int i = 0; i < _battleController.PlayerParty.Count; i++)
            {
                if (_battleController.PlayerParty[i] == attacker)
                {
                    CriticalHitsLanded++;
                    break;
                }
            }
        }

        private void HandleTurnEnd(int turn)
        {
            TurnsElapsed = turn;
        }

        private void HandlePlayerAction(PlayerAction action, IBattleUnit target)
        {
            if (action == PlayerAction.Skill) SkillsUsed++;
            if (action == PlayerAction.Item) ItemsUsed++;
        }

        private void HandleDamageTakenForWeakness(IBattleUnit target, IBattleUnit attacker, int damage)
        {
            // Check if this was a weakness exploit (attacker element strong vs target element)
            // Only count when player attacks enemy (attacker is player, target is enemy)
            bool attackerIsPlayer = false;
            for (int i = 0; i < _battleController.PlayerParty.Count; i++)
            {
                if (_battleController.PlayerParty[i] == attacker)
                {
                    attackerIsPlayer = true;
                    break;
                }
            }

            if (attackerIsPlayer && PerformanceMetrics.IsWeaknessExploit(attacker.Element, target.Element))
                WeaknessExploits++;
        }

        private void HandleStatusEffectApplied(IBattleUnit unit, StatusEffectType type, int duration, float value)
        {
            StatusEffectsApplied++;
        }

        private void HandleStatusEffectCured(IBattleUnit unit, StatusEffectType type)
        {
            StatusEffectsCured++;
        }

        /// <summary>
        /// Returns the BattleGrade enum via PerformanceMetrics point-based scoring.
        /// </summary>
        public BattleGrade CalculateGradeEnum()
        {
            return PerformanceMetrics.CalculateGrade(this);
        }

        private void HandleBattleRetreated()
        {
            WinStreak = 0;
            BattleDuration = Time.time - _battleStartTime;
        }

        private void HandleBattleWon()
        {
            BattleDuration = Time.time - _battleStartTime;
            WinStreak++;
        }

        private void HandleBattleLost()
        {
            BattleDuration = Time.time - _battleStartTime;
            WinStreak = 0;
        }
    }
}
