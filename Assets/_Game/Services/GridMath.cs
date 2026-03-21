namespace FrontLine.Services
{
    public static class GridMath
    {
        // Diagonal and cardinal steps both count as 1 tile in the current alpha rules.
        public static int GetTileDistance(int fromX, int fromY, int toX, int toY)
        {
            int deltaX = System.Math.Abs(toX - fromX);
            int deltaY = System.Math.Abs(toY - fromY);
            return System.Math.Max(deltaX, deltaY);
        }
    }
}
