namespace FrontLine.Models
{
    public readonly struct AttackEvaluation
    {
        public readonly bool CanAttack;   // InRange && HasLOS
        public readonly bool HasLOS;
        public readonly bool InRange;
        public readonly CoverType Cover;  // CoverType.None until cover system exists
        public readonly int HitChance;    // 0 if !CanAttack
        public readonly int Distance;     // raw tile distance for failure message fidelity

        public AttackEvaluation(bool inRange, bool hasLOS, CoverType cover, int hitChance, int distance)
        {
            InRange   = inRange;
            HasLOS    = hasLOS;
            CanAttack = inRange && hasLOS;
            Cover     = cover;
            HitChance = hitChance;
            Distance  = distance;
        }
    }
}
