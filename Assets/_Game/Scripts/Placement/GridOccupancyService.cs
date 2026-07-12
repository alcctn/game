using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Grid;

namespace CleanEnergy.Placement
{
    /// <summary>
    /// Tracks which grid cells are occupied by building instances.
    /// </summary>
    public sealed class GridOccupancyService
    {
        private readonly Dictionary<GridCoordinate, BuildingInstance> _occupied =
            new Dictionary<GridCoordinate, BuildingInstance>();

        public IReadOnlyDictionary<GridCoordinate, BuildingInstance> Occupied => _occupied;

        public bool IsOccupied(GridCoordinate coordinate) => _occupied.ContainsKey(coordinate);

        public bool TryGet(GridCoordinate coordinate, out BuildingInstance instance) =>
            _occupied.TryGetValue(coordinate, out instance);

        public bool TryOccupy(BuildingInstance instance)
        {
            if (instance == null)
            {
                return false;
            }

            if (_occupied.ContainsKey(instance.Coordinate))
            {
                return false;
            }

            _occupied[instance.Coordinate] = instance;
            return true;
        }

        public bool Release(GridCoordinate coordinate)
        {
            return _occupied.Remove(coordinate);
        }

        public void Clear()
        {
            foreach (var pair in _occupied)
            {
                if (pair.Value?.GameObject != null)
                {
                    UnityEngine.Object.Destroy(pair.Value.GameObject);
                }
            }

            _occupied.Clear();
        }

        public void ClearWithoutDestroyingVisuals()
        {
            _occupied.Clear();
        }
    }
}
