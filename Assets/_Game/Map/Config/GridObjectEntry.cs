using System;
using FrontLine.Map.Grid;

namespace FrontLine.Map.Config
{
    [Serializable]
    public class GridObjectEntry
    {
        public int X;
        public int Y;
        public GridObjectType ObjectType;
    }
}
