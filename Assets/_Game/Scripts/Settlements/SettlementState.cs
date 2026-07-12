using UnityEngine;

namespace CleanEnergy.Settlements
{
    /// <summary>
    /// Village population that scales consumer demand over ticks.
    /// </summary>
    public sealed class SettlementState
    {
        public const float DefaultStartingPopulation = 100f;
        public const float PopulationCap = 150f;
        public const float PopulationFloor = 80f;
        public const float GrowthPerTick = 0.02f;
        public const float ShrinkPerTick = 0.01f;
        public const float GrowthCoverageThreshold = 0.95f;
        public const float ShrinkCoverageThreshold = 0.5f;

        public float Population { get; private set; } = DefaultStartingPopulation;
        public float StartingPopulation { get; private set; } = DefaultStartingPopulation;

        public float DemandScale =>
            StartingPopulation > 0.0001f ? Population / StartingPopulation : 1f;

        public void Reset(float startingPopulation)
        {
            StartingPopulation = Mathf.Max(1f, startingPopulation);
            Population = StartingPopulation;
        }

        public void Restore(float population, float startingPopulation)
        {
            StartingPopulation = Mathf.Max(1f, startingPopulation);
            Population = Mathf.Clamp(population, PopulationFloor, PopulationCap);
        }

        public void Tick(float coverageRatio)
        {
            if (coverageRatio >= GrowthCoverageThreshold)
            {
                Population = Mathf.Min(PopulationCap, Population + GrowthPerTick);
            }
            else if (coverageRatio < ShrinkCoverageThreshold)
            {
                Population = Mathf.Max(PopulationFloor, Population - ShrinkPerTick);
            }
        }
    }
}
