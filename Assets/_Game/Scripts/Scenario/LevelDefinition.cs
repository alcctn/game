using UnityEngine;

namespace CleanEnergy.Scenario
{
    /// <summary>
    /// Data-driven Level 1 (and future level) thresholds. Tunable; v0 values avoid soft-locks.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelDefinition", menuName = "Clean Energy/Level Definition")]
    public sealed class LevelDefinition : ScriptableObject
    {
        [SerializeField] private string levelId = "level_01_village_power";
        [SerializeField] private string displayName = "Village Power";
        [SerializeField] private float startingMoney = 250f;
        [SerializeField] private float engineerHireCost = 40f;
        [SerializeField] private float technicianHireCost = 40f;
        [SerializeField] private int placementRadius = 10;
        [SerializeField] private float connectionCostPerCell = 2f;
        [SerializeField] private bool autoConnectEnabled = true;
        [SerializeField] private bool restrictBuildMenuToEnergy = true;
        [SerializeField] private float incomePerSuppliedEnergy = 1f;
        [SerializeField] private float requiredCoverageRatio = 0.95f;
        [SerializeField] private int requiredCoverageTicks = 40;
        [SerializeField] private float rewardWaterBuilt = 40f;
        [SerializeField] private float rewardTechnicianCreated = 50f;
        [SerializeField] private int weightEngineer = 15;
        [SerializeField] private int weightWater = 20;
        [SerializeField] private int weightTechnician = 15;
        [SerializeField] private int weightWind = 20;
        [SerializeField] private int weightCoverage = 30;

        public string LevelId => levelId;
        public string DisplayName => displayName;
        public float StartingMoney => startingMoney;
        public float EngineerHireCost => Mathf.Max(0f, engineerHireCost);
        public float TechnicianHireCost => Mathf.Max(0f, technicianHireCost);
        public int PlacementRadius => Mathf.Max(1, placementRadius);
        public float ConnectionCostPerCell => Mathf.Max(0f, connectionCostPerCell);
        public bool AutoConnectEnabled => autoConnectEnabled;
        /// <summary>When true, only Energy-tab buildings may be selected/placed (Level 1).</summary>
        public bool RestrictBuildMenuToEnergy => restrictBuildMenuToEnergy;
        public float IncomePerSuppliedEnergy => Mathf.Max(0f, incomePerSuppliedEnergy);
        public float RequiredCoverageRatio => Mathf.Clamp01(requiredCoverageRatio);
        public int RequiredCoverageTicks => Mathf.Max(1, requiredCoverageTicks);
        public float RewardWaterBuilt => Mathf.Max(0f, rewardWaterBuilt);
        public float RewardTechnicianCreated => Mathf.Max(0f, rewardTechnicianCreated);
        public int WeightEngineer => Mathf.Max(0, weightEngineer);
        public int WeightWater => Mathf.Max(0, weightWater);
        public int WeightTechnician => Mathf.Max(0, weightTechnician);
        public int WeightWind => Mathf.Max(0, weightWind);
        public int WeightCoverage => Mathf.Max(0, weightCoverage);

        public void ConfigureLevel01VillagePower()
        {
            levelId = "level_01_village_power";
            displayName = "Köyü Aydınlat";
            startingMoney = 250f;
            engineerHireCost = 40f;
            technicianHireCost = 40f;
            placementRadius = 10;
            connectionCostPerCell = 2f;
            autoConnectEnabled = true;
            restrictBuildMenuToEnergy = true;
            incomePerSuppliedEnergy = 1f;
            requiredCoverageRatio = 0.95f;
            requiredCoverageTicks = 40;
            rewardWaterBuilt = 40f;
            rewardTechnicianCreated = 50f;
            weightEngineer = 15;
            weightWater = 20;
            weightTechnician = 15;
            weightWind = 20;
            weightCoverage = 30;
        }

        public static LevelDefinition CreateRuntimeDefault()
        {
            var level = CreateInstance<LevelDefinition>();
            level.ConfigureLevel01VillagePower();
            return level;
        }
    }
}
