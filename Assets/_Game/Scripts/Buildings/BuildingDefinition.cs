using UnityEngine;

namespace CleanEnergy.Buildings
{
    /// <summary>
    /// Data-driven building definition for placement and economy.
    /// </summary>
    [CreateAssetMenu(fileName = "BuildingDefinition", menuName = "Clean Energy/Building Definition")]
    public sealed class BuildingDefinition : ScriptableObject
    {
        [SerializeField] private string id = "building";
        [SerializeField] private string displayName = "Building";
        [SerializeField] private string description = "";
        [SerializeField] private BuildingCategory category = BuildingCategory.Energy;
        [SerializeField] private GameObject prefab;
        [SerializeField] private Vector2Int size = Vector2Int.one;
        [SerializeField] private float cost = 100f;
        [SerializeField] private float maintenanceCost = 1f;
        [SerializeField] private float installedPower = 10f;
        [SerializeField] private float efficiency = 0.8f;
        [SerializeField] private float maxSlopeDegrees = 30f;
        [SerializeField] private float minWaterFlow;
        [SerializeField] private float minSolarPotential;
        [SerializeField] private float minWindPotential;
        [SerializeField] private bool requiresAdjacentWater;
        [SerializeField] private bool requireBuildableCell = true;
        [SerializeField] private Color gizmoColor = new Color(0.3f, 0.75f, 0.9f, 1f);

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public BuildingCategory Category => category;
        public GameObject Prefab => prefab;
        public Vector2Int Size => size;
        public float Cost => cost;
        public float MaintenanceCost => maintenanceCost;
        public float InstalledPower => installedPower;
        public float Efficiency => efficiency;
        public float MaxSlopeDegrees => maxSlopeDegrees;
        public float MinWaterFlow => minWaterFlow;
        public float MinSolarPotential => minSolarPotential;
        public float MinWindPotential => minWindPotential;
        public bool RequiresAdjacentWater => requiresAdjacentWater;
        public bool RequireBuildableCell => requireBuildableCell;
        public Color GizmoColor => gizmoColor;

        public void Configure(
            string buildingId,
            string name,
            string desc,
            BuildingCategory buildingCategory,
            float buildingCost,
            float power,
            float slopeMax,
            float waterMin,
            float solarMin,
            float windMin,
            bool adjacentWater,
            bool buildableRequired,
            Color color)
        {
            id = buildingId;
            displayName = name;
            description = desc;
            category = buildingCategory;
            cost = buildingCost;
            installedPower = power;
            maxSlopeDegrees = slopeMax;
            minWaterFlow = waterMin;
            minSolarPotential = solarMin;
            minWindPotential = windMin;
            requiresAdjacentWater = adjacentWater;
            requireBuildableCell = buildableRequired;
            gizmoColor = color;
            size = Vector2Int.one;
            efficiency = 0.8f;
        }
    }
}
