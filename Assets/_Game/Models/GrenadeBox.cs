namespace FrontLine.Models
{
    public class GrenadeBox
    {
        public GrenadeType GrenadeType { get; private set; }
        public int TileX { get; private set; }
        public int TileY { get; private set; }

        public GrenadeBox(GrenadeType grenadeType, int tileX, int tileY)
        {
            GrenadeType = grenadeType;
            TileX = tileX;
            TileY = tileY;
        }
    }
}
