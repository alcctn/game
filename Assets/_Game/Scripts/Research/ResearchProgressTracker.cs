using System;
using CleanEnergy.Energy;
using UnityEngine;

namespace CleanEnergy.Research
{
    /// <summary>
    /// Grants research points from energy coverage and diversity milestones.
    /// </summary>
    public sealed class ResearchProgressTracker
    {
        public const float CoverageRpPerTick = 1f;
        public const float DiversityBonusRp = 10f;

        private readonly ResearchService _research;
        private readonly float _coverageThreshold;
        private bool _diversityBonusGranted;

        /// <summary>Coverage RP granted on the most recent tick (0 when none).</summary>
        public float LastCoverageRpGranted { get; private set; }

        /// <summary>True when the diversity bonus was granted on the most recent tick.</summary>
        public bool LastDiversityBonusGranted { get; private set; }

        /// <summary>Raised once when the diversity RP bonus is granted.</summary>
        public event Action DiversityBonusGranted;

        public ResearchProgressTracker(ResearchService research, float coverageThreshold = 0.95f)
        {
            _research = research;
            _coverageThreshold = coverageThreshold;
        }

        public void Reset()
        {
            _diversityBonusGranted = false;
            LastCoverageRpGranted = 0f;
            LastDiversityBonusGranted = false;
        }

        public void MarkDiversityBonusGranted()
        {
            _diversityBonusGranted = true;
        }

        public void OnBalanceTick(EnergyBalanceResult result, int activeProducerTypeCount)
        {
            LastCoverageRpGranted = 0f;
            LastDiversityBonusGranted = false;

            if (_research == null || result == null)
            {
                return;
            }

            if (result.Demand > 0.0001f && result.CoverageRatio + 0.0001f >= _coverageThreshold)
            {
                _research.Wallet.Add(CoverageRpPerTick);
                LastCoverageRpGranted = CoverageRpPerTick;
            }

            if (!_diversityBonusGranted && activeProducerTypeCount >= 2)
            {
                _diversityBonusGranted = true;
                LastDiversityBonusGranted = true;
                _research.Wallet.Add(DiversityBonusRp);
                DiversityBonusGranted?.Invoke();
                Debug.Log($"[Research] Diversity bonus +{DiversityBonusRp:F0} RP");
            }
        }
    }
}
