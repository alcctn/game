using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using CleanEnergy.Placement;
using UnityEngine;

namespace CleanEnergy.Energy
{
    /// <summary>
    /// Isolated producers (no consumer/storage in component) contribute zero production.
    /// </summary>
    public static class NetworkLoadFactor
    {
        public static bool ComponentHasLoad(EnergyNetworkComponent component)
        {
            if (component?.Nodes == null)
            {
                return false;
            }

            for (var i = 0; i < component.Nodes.Count; i++)
            {
                var node = component.Nodes[i];
                if (node.Consumer != null || node.Storage != null)
                {
                    return true;
                }
            }

            return false;
        }

        public static float ResolveForBuilding(BuildingInstance building, EnergyNetworkGraph graph)
        {
            if (building == null || graph == null)
            {
                return 1f;
            }

            foreach (var component in graph.Components)
            {
                for (var i = 0; i < component.Nodes.Count; i++)
                {
                    if (component.Nodes[i].Id != building.InstanceId)
                    {
                        continue;
                    }

                    return ComponentHasLoad(component) ? 1f : 0f;
                }
            }

            return 0f;
        }

        /// <summary>
        /// Placement ghost preview: load exists if a consumer or storage is within ConnectionRange.
        /// Hub alone does not count as load.
        /// </summary>
        public static float ResolveForPlacement(
            GridCoordinate coordinate,
            BuildingDefinition definition,
            GridOccupancyService occupancy)
        {
            if (definition == null || occupancy == null)
            {
                return 0f;
            }

            var range = Mathf.Max(0, definition.ConnectionRange);
            foreach (var pair in occupancy.Occupied)
            {
                var other = pair.Value?.Definition;
                if (other == null || (!other.IsConsumer && !other.IsStorage))
                {
                    continue;
                }

                if (Manhattan(coordinate, pair.Key) <= range)
                {
                    return 1f;
                }
            }

            return 0f;
        }

        private static int Manhattan(GridCoordinate a, GridCoordinate b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            if (dx < 0) dx = -dx;
            if (dy < 0) dy = -dy;
            return dx + dy;
        }
    }
}
