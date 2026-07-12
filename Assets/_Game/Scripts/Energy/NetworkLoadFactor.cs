using CleanEnergy.Buildings;
using CleanEnergy.Grid;

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
    }
}
