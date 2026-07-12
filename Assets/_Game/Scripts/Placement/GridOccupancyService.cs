using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Grid;

namespace CleanEnergy.Placement
{
    /// <summary>
    /// Tracks which grid cells are occupied by building instances.
    /// Multi-cell buildings occupy every footprint cell pointing at the same instance.
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
            if (instance?.Definition == null)
            {
                return false;
            }

            var cells = BuildingFootprint.GetCells(
                instance.Definition, instance.Coordinate, instance.Rotation);
            for (var i = 0; i < cells.Count; i++)
            {
                if (_occupied.ContainsKey(cells[i]))
                {
                    return false;
                }
            }

            for (var i = 0; i < cells.Count; i++)
            {
                _occupied[cells[i]] = instance;
            }

            return true;
        }

        /// <summary>
        /// Releases every footprint cell for the building at <paramref name="coordinate"/>.
        /// </summary>
        public bool Release(GridCoordinate coordinate)
        {
            if (!_occupied.TryGetValue(coordinate, out var instance) || instance == null)
            {
                return false;
            }

            return ReleaseInstance(instance);
        }

        /// <summary>
        /// Releases every footprint cell owned by <paramref name="instance"/>.
        /// </summary>
        public bool ReleaseInstance(BuildingInstance instance)
        {
            if (instance?.Definition == null)
            {
                return false;
            }

            var cells = BuildingFootprint.GetCells(
                instance.Definition, instance.Coordinate, instance.Rotation);
            var removed = false;
            for (var i = 0; i < cells.Count; i++)
            {
                if (_occupied.TryGetValue(cells[i], out var occupied)
                    && occupied != null
                    && occupied.InstanceId == instance.InstanceId
                    && _occupied.Remove(cells[i]))
                {
                    removed = true;
                }
            }

            return removed;
        }

        public void Clear()
        {
            var seen = new HashSet<string>();
            foreach (var pair in _occupied)
            {
                var instance = pair.Value;
                if (instance?.GameObject == null || !seen.Add(instance.InstanceId))
                {
                    continue;
                }

                UnityEngine.Object.Destroy(instance.GameObject);
            }

            _occupied.Clear();
        }

        public void ClearWithoutDestroyingVisuals()
        {
            _occupied.Clear();
        }
    }
}
