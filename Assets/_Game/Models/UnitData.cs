namespace FrontLine.Models
{
    public class UnitData
    {
        public string UnitId { get; private set; }
        public string OwnerId { get; private set; }

        public int TileX { get; set; }
        public int TileY { get; set; }

        public int Health { get; set; }
        public int MaxHealth { get; private set; }
        public int ActionPoints { get; set; }
        public int MaxActionPoints { get; private set; }

        public int MoveRange { get; private set; }
        public int AttackRange { get; private set; }
        public int Damage { get; private set; }

        public bool IsAlive => Health > 0;

        public UnitData(string unitId, string ownerId, int tileX, int tileY)
        {
            UnitId = unitId;
            OwnerId = ownerId;
            TileX = tileX;
            TileY = tileY;

            // TODO: Alpha defaults — we'll make these configurable later
            MaxHealth = 3;
            Health = MaxHealth;
            MaxActionPoints = 2;
            ActionPoints = MaxActionPoints;
            MoveRange = 3;
            AttackRange = 2;
            Damage = 1;
        }

        public void ResetActionPoints() => ActionPoints = MaxActionPoints;

        public void TakeDamage(int amount)
        {
            Health -= amount;
            if (Health < 0) Health = 0;
        }

        public override string ToString()
            => $"Unit({UnitId}) Owner:{OwnerId} HP:{Health}/{MaxHealth} AP:{ActionPoints}";
    }
}
