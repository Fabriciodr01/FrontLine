using UnityEngine;
using FrontLine.Models;
using FrontLine.Map.Grid;

namespace FrontLine.Map.Config
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
                Debug.LogWarning("[MapLoader] No MapData assigned - grid will have no baked objects.");
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
                    case GridObjectType.FragGrenadeBox:
                        _gameState.AddGrenadeBox(new GrenadeBox(GrenadeType.Frag, entry.X, entry.Y));
                        break;
                    case GridObjectType.SmokeGrenadeBox:
                        _gameState.AddGrenadeBox(new GrenadeBox(GrenadeType.Smoke, entry.X, entry.Y));
                        break;
                        // Decoration: intentional no-op - visual only
                }
            }
        }
    }
}
