using UnityEngine;
using FrontLine.Models;

namespace FrontLine.Services
{
    public class MapLoader
    {
        private readonly GameState _gameState;
        private readonly MapData _mapData;

        public MapLoader(GameState gameState, MapData mapData)
        {
            _gameState = gameState;
            _mapData = mapData;
        }

        public void Load()
        {
            if (_mapData == null)
            {
                Debug.LogWarning("[MapLoader] No MapData assigned — grid will have no obstacles.");
                return;
            }

            foreach (var entry in _mapData.Entries)
            {
                var tile = _gameState.GetTile(entry.X, entry.Y);
                if (tile == null)
                {
                    Debug.LogWarning($"[MapLoader] Entry ({entry.X},{entry.Y}) out of bounds — skipped.");
                    continue;
                }

                switch (entry.ObjectType)
                {
                    case GridObjectType.Wall:
                    case GridObjectType.HalfCover:
                    case GridObjectType.FullCover:
                        tile.Type = TileType.Blocked;
                        break;
                    // Decoration: intentional no-op — visual only
                }
            }
        }
    }
}
