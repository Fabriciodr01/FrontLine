using System.Collections.Generic;
using UnityEngine;

namespace FrontLine.Map.Config
{
    [CreateAssetMenu(fileName = "MapData", menuName = "FrontLine/MapData")]
    public class MapData : ScriptableObject
    {
        [SerializeField] private List<GridObjectEntry> _entries = new();
        public IReadOnlyList<GridObjectEntry> Entries => _entries;

        public void SetEntries(List<GridObjectEntry> entries) =>
            _entries = entries ?? new List<GridObjectEntry>();
    }
}
