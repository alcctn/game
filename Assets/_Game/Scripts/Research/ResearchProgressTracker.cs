using CleanEnergy.Energy;
using CleanEnergy.Economy;
using UnityEngine;

namespace CleanEnergy.Research
{
    /// <summary>
    /// Grants research points from energy coverage and diversity milestones.
    /// </summary>
    public sealed class ResearchProgressTracker
    {
        private readonly ResearchService _research;
        private readonly float _coverageThreshold;
        private bool _diversityBonusGranted;

        public ResearchProgressTracker(ResearchService research, float coverageThreshold = 0.95f)
        {
            _research = research;
            _coverageThreshold = coverageThreshold;
        }

        public void Reset()
        {
            _diversityBonusGranted = false;
        }

        public void MarkDiversityBonusGranted()
        {
            _diversityBonusGranted = true;
        }

        public void OnBalanceTick(EnergyBalanceResult result, int activeProducerTypeCount)
        {
            if (_research == null || result == null)
            {
                return;
            }

            if (result.Demand > 0.0001f && result.CoverageRatio + 0.0001f >= _coverageThreshold)
            {
                _research.Wallet.Add(1f);
            }

            if (!_diversityBonusGranted && activeProducerTypeCount >= 2)
            {
                _diversityBonusGranted = true;
                _research.Wallet.Add(10f);
                Debug.Log("[Research] Diversity bonus +10 RP");
            }
        }
    }
}
