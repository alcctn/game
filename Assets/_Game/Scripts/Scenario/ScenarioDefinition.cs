using CleanEnergy.Map;
using UnityEngine;

namespace CleanEnergy.Scenario
{
    /// <summary>
    /// Data-driven win / soft-lose thresholds for a playable scenario.
    /// </summary>
    [CreateAssetMenu(fileName = "ScenarioDefinition", menuName = "Clean Energy/Scenario Definition")]
    public sealed class ScenarioDefinition : ScriptableObject
    {
        [SerializeField] private string scenarioId = "green_valley";
        [SerializeField] private string displayName = "Green Valley";
        [SerializeField] private float requiredCoverageRatio = 0.95f;
        [SerializeField] private int requiredCoverageTicks = 60;
        [SerializeField] private int requiredProducerTypes = 2;
        [SerializeField] private float initialSatisfaction = 100f;
        [SerializeField] private float shortageSatisfactionPenalty = 2f;
        [SerializeField] private float coverageSatisfactionRecovery = 0.25f;
        [SerializeField] private float riskSatisfactionThreshold = 30f;
        [SerializeField] private string mapSeed = "";
        [SerializeField] private float baseClimateSolarOverride = -1f;
        [SerializeField] private float baseClimateWindOverride = -1f;
        [SerializeField] private float streamAccumulationOverride = -1f;
        [SerializeField] private string[] countedProducerTypeIds =
        {
            "water_wheel",
            "small_hydro",
            "small_solar",
            "small_wind"
        };
        [SerializeField] private string[] requiredResearchNodeIds =
        {
            "solar_basic"
        };
        [SerializeField] private float startingPopulation = 100f;

        public string ScenarioId => scenarioId;
        public string DisplayName => displayName;
        public float RequiredCoverageRatio => requiredCoverageRatio;
        public int RequiredCoverageTicks => Mathf.Max(1, requiredCoverageTicks);
        public int RequiredProducerTypes => Mathf.Max(1, requiredProducerTypes);
        public float InitialSatisfaction => initialSatisfaction;
        public float ShortageSatisfactionPenalty => shortageSatisfactionPenalty;
        public float CoverageSatisfactionRecovery => coverageSatisfactionRecovery;
        public float RiskSatisfactionThreshold => riskSatisfactionThreshold;
        public string MapSeed => mapSeed ?? string.Empty;
        public float BaseClimateSolarOverride => baseClimateSolarOverride;
        public float BaseClimateWindOverride => baseClimateWindOverride;
        public float StreamAccumulationOverride => streamAccumulationOverride;
        public string[] CountedProducerTypeIds => countedProducerTypeIds ?? System.Array.Empty<string>();
        public string[] RequiredResearchNodeIds => requiredResearchNodeIds ?? System.Array.Empty<string>();
        public float StartingPopulation => startingPopulation > 0.001f ? startingPopulation : 100f;

        public void Configure(
            string id,
            string name,
            float coverageRatio,
            int coverageTicks,
            int producerTypes,
            float satisfaction,
            float shortagePenalty,
            float recovery,
            float riskThreshold,
            string[] researchNodeIds = null,
            string seed = "",
            float solarOverride = -1f,
            float streamOverride = -1f,
            float population = 100f,
            float windOverride = -1f)
        {
            scenarioId = id;
            displayName = name;
            requiredCoverageRatio = coverageRatio;
            requiredCoverageTicks = coverageTicks;
            requiredProducerTypes = producerTypes;
            initialSatisfaction = satisfaction;
            shortageSatisfactionPenalty = shortagePenalty;
            coverageSatisfactionRecovery = recovery;
            riskSatisfactionThreshold = riskThreshold;
            countedProducerTypeIds = new[]
            {
                "water_wheel",
                "small_hydro",
                "small_solar",
                "small_wind"
            };
            requiredResearchNodeIds = researchNodeIds ?? new[] { "solar_basic" };
            mapSeed = seed ?? string.Empty;
            baseClimateSolarOverride = solarOverride;
            streamAccumulationOverride = streamOverride;
            startingPopulation = population > 0.001f ? population : 100f;
            baseClimateWindOverride = windOverride;
        }

        public void ApplyToMapSettings(MapGenerationSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(mapSeed))
            {
                settings.SetSeed(mapSeed);
            }

            if (baseClimateSolarOverride >= 0f)
            {
                settings.SetBaseClimateSolar(baseClimateSolarOverride);
            }

            if (baseClimateWindOverride >= 0f)
            {
                settings.SetBaseWind(baseClimateWindOverride);
            }

            if (streamAccumulationOverride > 0f)
            {
                var lake = Mathf.Max(settings.LakeAccumulationThreshold, streamAccumulationOverride * 3f);
                settings.SetWaterThresholds(streamAccumulationOverride, lake);
            }
        }
    }
}
