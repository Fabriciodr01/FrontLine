namespace FrontLine.Models
{
    public readonly struct AttackEvaluation
    {
        public readonly bool CanAttack;
        public readonly bool HasLOS;
        public readonly bool InRange;
        public readonly bool AttackerInSmoke;
        public readonly bool TargetInSmoke;
        public readonly CoverType Cover;  // CoverType.None until cover system exists
        public readonly int HitChance;    // 0 if !CanAttack
        public readonly int Distance;     // raw tile distance for failure message fidelity

        public AttackEvaluation(
            bool inRange,
            bool hasLOS,
            bool attackerInSmoke,
            bool targetInSmoke,
            CoverType cover,
            int hitChance,
            int distance)
        {
            InRange = inRange;
            HasLOS = hasLOS;
            AttackerInSmoke = attackerInSmoke;
            TargetInSmoke = targetInSmoke;
            CanAttack = inRange && hasLOS && !attackerInSmoke && !targetInSmoke;
            Cover = cover;
            HitChance = CanAttack ? hitChance : 0;
            Distance = distance;
        }
    }
}
