using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Static utility class for grade scoring, multiplier calculations, and element checks.
    /// </summary>
    public static class PerformanceMetrics
    {
        /// <summary>
        /// Point-based scoring (0-100) that maps to BattleGrade.
        /// Base 50, +20 no deaths, +15 speed (<=5 turns), +10 crits (3+), +10 no damage taken, -15 per death.
        /// S: 90+, A: 75+, B: 55+, C: 35+, D: less than 35.
        /// </summary>
        public static BattleGrade CalculateGrade(BattleStatisticsTracker tracker)
        {
            int score = 50;

            if (tracker.PlayerDeaths == 0)
                score += 20;
            else
                score -= tracker.PlayerDeaths * 15;

            if (tracker.TurnsElapsed <= 5)
                score += 15;

            if (tracker.CriticalHitsLanded >= 3)
                score += 10;

            if (tracker.TotalDamageTaken == 0)
                score += 10;

            score = Mathf.Clamp(score, 0, 100);

            if (score >= 90) return BattleGrade.S;
            if (score >= 75) return BattleGrade.A;
            if (score >= 55) return BattleGrade.B;
            if (score >= 35) return BattleGrade.C;
            return BattleGrade.D;
        }

        public static float GetGradeXPMultiplier(BattleGrade grade)
        {
            switch (grade)
            {
                case BattleGrade.S: return 1.5f;
                case BattleGrade.A: return 1.25f;
                case BattleGrade.B: return 1.0f;
                case BattleGrade.C: return 0.75f;
                case BattleGrade.D: return 0.5f;
                default: return 1.0f;
            }
        }

        /// <summary>
        /// Streak multiplier: 1.0 + min(streak, 20) * 0.15. Streak 20 = 4.0x.
        /// </summary>
        public static float GetStreakMultiplier(int streak)
        {
            return 1.0f + Mathf.Min(streak, 20) * 0.15f;
        }

        public static float GetGradeDropRateMultiplier(BattleGrade grade)
        {
            switch (grade)
            {
                case BattleGrade.S: return 1.5f;
                case BattleGrade.A: return 1.25f;
                case BattleGrade.B: return 1.0f;
                case BattleGrade.C: return 0.8f;
                case BattleGrade.D: return 0.6f;
                default: return 1.0f;
            }
        }

        /// <summary>
        /// Drop rate streak multiplier: 1.0 + min(streak, 20) * 0.05. Cap 2.0.
        /// </summary>
        public static float GetStreakDropRateMultiplier(int streak)
        {
            return 1.0f + Mathf.Min(streak, 20) * 0.05f;
        }

        /// <summary>
        /// Returns true if attacker element is strong against target element.
        /// Classical cycle: Ignis > Ventus > Terra > Aqua > Ignis.
        /// Arcane duality: Lux and Umbra mutually super-effective.
        /// </summary>
        public static bool IsWeaknessExploit(ElementType attacker, ElementType target)
        {
            if (attacker == ElementType.None || target == ElementType.None)
                return false;

            return (attacker == ElementType.Ignis  && target == ElementType.Ventus) ||
                   (attacker == ElementType.Ventus && target == ElementType.Terra)  ||
                   (attacker == ElementType.Terra  && target == ElementType.Aqua)   ||
                   (attacker == ElementType.Aqua   && target == ElementType.Ignis)  ||
                   (attacker == ElementType.Lux    && target == ElementType.Umbra)  ||
                   (attacker == ElementType.Umbra  && target == ElementType.Lux);
        }
    }
}
