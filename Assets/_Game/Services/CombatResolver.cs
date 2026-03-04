using System;
using FrontLine.Models;

namespace FrontLine.Services
{
    public class CombatResult
    {
        public bool Hit { get; private set; }
        public int DamageDealt { get; private set; }
        public bool Killed { get; private set; }
        public int RollResult { get; private set; }
        public int HitChance { get; private set; }

        public static CombatResult Miss(int roll, int hitChance)
            => new CombatResult { Hit = false, DamageDealt = 0, Killed = false, RollResult = roll, HitChance = hitChance };

        public static CombatResult Success(int damage, bool killed, int roll, int hitChance)
            => new CombatResult { Hit = true, DamageDealt = damage, Killed = killed, RollResult = roll, HitChance = hitChance };

        public override string ToString()
            => Hit
                ? $"Hit! Damage:{DamageDealt} Killed:{Killed} Roll:{RollResult} Needed:{HitChance}"
                : $"Miss. Roll:{RollResult} Needed:{HitChance}";
    }

    public class CombatResolver
    {
        private readonly Random _random;

        // Base hit chance percentage (0-100)
        private const int BaseHitChance = 75;

        // How much hit chance drops per tile of distance beyond 1
        private const int DistancePenaltyPerTile = 10;

        // Minimum hit chance floor — never impossible to hit
        private const int MinHitChance = 15;

        public CombatResolver(Random random = null)
        {
            _random = random ?? new Random();
        }

        public CombatResult Resolve(UnitData attacker, UnitData target)
        {
            if (attacker == null) throw new ArgumentNullException(nameof(attacker));
            if (target == null) throw new ArgumentNullException(nameof(target));

            int hitChance = CalculateHitChance(attacker, target);
            int roll = _random.Next(1, 101); // 1 to 100 inclusive

            if (roll > hitChance)
                return CombatResult.Miss(roll, hitChance);

            int damage = attacker.Damage;
            bool killed = (target.Health - damage) <= 0;

            return CombatResult.Success(damage, killed, roll, hitChance);
        }

        public int CalculateHitChance(UnitData attacker, UnitData target)
        {
            int distance = Math.Max(
                Math.Abs(target.TileX - attacker.TileX),
                Math.Abs(target.TileY - attacker.TileY));

            int penalty = Math.Max(0, distance - 1) * DistancePenaltyPerTile;
            int chance = Math.Max(MinHitChance, BaseHitChance - penalty);

            return chance;
        }
    }
}
