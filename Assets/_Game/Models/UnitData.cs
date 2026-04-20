namespace FrontLine.Models
{
    public class UnitData
    {
        public string UnitId { get; private set; }
        public string OwnerId { get; private set; }

        public int TileX { get; set; }
        public int TileY { get; set; }

        public HealthComponent Health { get; private set; }
        public int ActionPoints { get; set; }
        public int MaxActionPoints { get; private set; }

        public int MoveRange { get; private set; }
        public int DashRange { get; private set; }
        public int AttackRange { get; private set; }
        public int Damage { get; private set; }
        public int FragGrenades { get; set; }
        public int SmokeGrenades { get; set; }

        public bool IsAlive => Health.IsAlive;

        public UnitData(string unitId, string ownerId, int tileX, int tileY)
        {
            UnitId = unitId;
            OwnerId = ownerId;
            TileX = tileX;
            TileY = tileY;

            // TODO-POST-ALPHA: Alpha defaults — we'll make these configurable later with SO
            Health = new HealthComponent(3);
            MaxActionPoints = 2;
            ActionPoints = MaxActionPoints;
            MoveRange = 3;
            DashRange = 5;
            AttackRange = 4;
            Damage = 1;
        }

        public void ResetActionPoints() => ActionPoints = MaxActionPoints;

        public override string ToString()
            => $"Unit({UnitId}) Owner:{OwnerId} HP:{Health.Current}/{Health.Max} AP:{ActionPoints}";
    }
}
