using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using UnityEngine;

namespace CleanEnergy.Energy
{
    /// <summary>
    /// Hop-based delivery loss from producer to nearest load (GDD transmission loss).
    /// </summary>
    public static class TransmissionLoss
    {
        public const float LossPerHop = 0.05f;
        public const float MinDeliveryFactor = 0.25f;

        public static float ResolveDeliveryFactor(
            GridCoordinate producerCoordinate,
            EnergyNetworkComponent component)
        {
            if (component?.Nodes == null || component.Nodes.Count == 0)
            {
                return 1f;
            }

            var best = int.MaxValue;
            for (var i = 0; i < component.Nodes.Count; i++)
            {
                var node = component.Nodes[i];
                if (node.Consumer == null && node.Storage == null)
                {
                    continue;
                }

                var d = Manhattan(producerCoordinate, node.Coordinate);
                if (d < best)
                {
                    best = d;
                }
            }

            if (best == int.MaxValue)
            {
                return 1f;
            }

            return Mathf.Clamp(1f - LossPerHop * best, MinDeliveryFactor, 1f);
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

                    return ResolveDeliveryFactor(building.Coordinate, component);
                }
            }

            return 1f;
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
