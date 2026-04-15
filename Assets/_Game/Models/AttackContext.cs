namespace FrontLine.Models
{
    public readonly struct AttackContext
    {
        public readonly UnitData Attacker;
        public readonly UnitData Target;

        public AttackContext(UnitData attacker, UnitData target)
        {
            Attacker = attacker;
            Target   = target;
        }
    }
}
