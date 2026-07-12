using CleanEnergy.Buildings;
using CleanEnergy.Energy;

namespace CleanEnergy.UI
{
    public enum InspectionNetworkStatus
    {
        NoBuilding,
        NotInNetwork,
        Isolated,
        Connected
    }

    /// <summary>
    /// Pure helpers for the right-hand inspection panel.
    /// </summary>
    public static class InspectionStatus
    {
        public static InspectionNetworkStatus ResolveNetwork(
            BuildingInstance building,
            EnergyNetworkGraph graph)
        {
            if (building == null)
            {
                return InspectionNetworkStatus.NoBuilding;
            }

            if (graph == null || !graph.Nodes.ContainsKey(building.InstanceId))
            {
                return InspectionNetworkStatus.NotInNetwork;
            }

            foreach (var component in graph.Components)
            {
                var contains = false;
                var otherCount = 0;
                for (var i = 0; i < component.Nodes.Count; i++)
                {
                    if (component.Nodes[i].Id == building.InstanceId)
                    {
                        contains = true;
                    }
                    else
                    {
                        otherCount++;
                    }
                }

                if (contains)
                {
                    return otherCount > 0
                        ? InspectionNetworkStatus.Connected
                        : InspectionNetworkStatus.Isolated;
                }
            }

            return InspectionNetworkStatus.NotInNetwork;
        }

        public static string NetworkLabel(InspectionNetworkStatus status)
        {
            switch (status)
            {
                case InspectionNetworkStatus.NoBuilding:
                    return "No building";
                case InspectionNetworkStatus.NotInNetwork:
                    return "Not in network";
                case InspectionNetworkStatus.Isolated:
                    return "Isolated";
                case InspectionNetworkStatus.Connected:
                    return "Connected";
                default:
                    return "Unknown";
            }
        }

        public static string FormatEfficiencyHint(BuildingDefinition definition, float maintenanceLevel)
        {
            if (definition == null || !definition.IsProducer)
            {
                return string.Empty;
            }

            return string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "Prod ~ {0:F0} x potential x {1:F2} x maint {2:F2}",
                definition.InstalledPower,
                definition.Efficiency,
                maintenanceLevel);
        }
    }
}
