using CleanEnergy.Grid;
using UnityEngine;

namespace CleanEnergy.Buildings
{
    /// <summary>
    /// Runtime building placed on the grid.
    /// </summary>
    public sealed class BuildingInstance
    {
        public string InstanceId { get; }
        public BuildingDefinition Definition { get; }
        public GridCoordinate Coordinate { get; }
        public int Rotation { get; }
        public GameObject GameObject { get; }
        public float MaintenanceLevel { get; set; } = 1f;
        public float CurrentProduction { get; set; }

        public BuildingInstance(
            string instanceId,
            BuildingDefinition definition,
            GridCoordinate coordinate,
            int rotation,
            GameObject gameObject)
        {
            InstanceId = instanceId;
            Definition = definition;
            Coordinate = coordinate;
            Rotation = rotation;
            GameObject = gameObject;
        }
    }
}
