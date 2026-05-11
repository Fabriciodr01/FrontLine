using UnityEngine;
using FrontLine.Models;
using FrontLine.Controllers;

namespace FrontLine.Views
{
    public class GrenadeBoxView : MonoBehaviour
    {
        [Header("Fallback Authoring")]
        [SerializeField] private int _tileX;
        [SerializeField] private int _tileY;
        [SerializeField] private GrenadeType _grenadeType;

        private CommandProcessor _commandProcessor;
        private GridObject _gridObject;
        private int _bakedTileX;
        private int _bakedTileY;
        private GrenadeType _bakedGrenadeType;

        public void Initialize(CommandProcessor commandProcessor)
        {
            _commandProcessor = commandProcessor;
            CacheBakedIdentity();
            _commandProcessor.OnGrenadeCollected += HandleGrenadeCollected;
        }

        private void OnDestroy()
        {
            if (_commandProcessor != null)
                _commandProcessor.OnGrenadeCollected -= HandleGrenadeCollected;
        }

        public bool TryGetBakedEntry(out GridObjectEntry entry)
        {
            CacheBakedIdentity();
            entry = new GridObjectEntry
            {
                X = _bakedTileX,
                Y = _bakedTileY,
                ObjectType = _bakedGrenadeType == GrenadeType.Frag
                    ? GridObjectType.FragGrenadeBox
                    : GridObjectType.SmokeGrenadeBox
            };
            return true;
        }

        private void CacheBakedIdentity()
        {
            _gridObject = GetComponent<GridObject>();
            if (_gridObject != null &&
                (_gridObject.ObjectType == GridObjectType.FragGrenadeBox ||
                 _gridObject.ObjectType == GridObjectType.SmokeGrenadeBox))
            {
                var (x, y) = _gridObject.GetAnchorCell();
                _bakedTileX = x;
                _bakedTileY = y;
                _bakedGrenadeType = _gridObject.ObjectType == GridObjectType.SmokeGrenadeBox
                    ? GrenadeType.Smoke
                    : GrenadeType.Frag;
                return;
            }

            _bakedTileX = _tileX;
            _bakedTileY = _tileY;
            _bakedGrenadeType = _grenadeType;
        }

        private void HandleGrenadeCollected(string unitId, GrenadeBox box)
        {
            if (box.TileX != _bakedTileX || box.TileY != _bakedTileY) return;
            if (box.GrenadeType != _bakedGrenadeType) return;
            Destroy(gameObject);
        }
    }
}
