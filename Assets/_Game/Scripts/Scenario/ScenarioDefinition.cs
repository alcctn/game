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
        [SerializeField] private string[] countedProducerTypeIds =
        {
            "water_wheel",
            "small_solar",
            "small_wind"
        };

        public string ScenarioId => scenarioId;
        public string DisplayName => displayName;
        public float RequiredCoverageRatio => requiredCoverageRatio;
        public int RequiredCoverageTicks => Mathf.Max(1, requiredCoverageTicks);
        public int RequiredProducerTypes => Mathf.Max(1, requiredProducerTypes);
        public float InitialSatisfaction => initialSatisfaction;
        public float ShortageSatisfactionPenalty => shortageSatisfactionPenalty;
        public float CoverageSatisfactionRecovery => coverageSatisfactionRecovery;
        public float RiskSatisfactionThreshold => riskSatisfactionThreshold;
        public string[] CountedProducerTypeIds => countedProducerTypeIds;

        public void Configure(
            string id,
            string name,
            float coverageRatio,
            int coverageTicks,
            int producerTypes,
            float satisfaction,
            float shortagePenalty,
            float recovery,
            float riskThreshold)
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
                "small_solar",
                "small_wind"
            };
        }
    }
}
