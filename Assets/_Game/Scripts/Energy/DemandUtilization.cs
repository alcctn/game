using UnityEngine;

namespace CleanEnergy.Energy
{
    /// <summary>
    /// Consumer demand ratio for demand debug coloring.
    /// </summary>
    public static class DemandUtilization
    {
        public static float ComputeRatio(float currentDemand, float baseDemand)
        {
            if (baseDemand <= 0.0001f)
            {
                return 0f;
            }

            return Mathf.Clamp01(currentDemand / baseDemand);
        }

        public static Color ColorForRatio(float ratio)
        {
            return Color.Lerp(
                new Color(0.25f, 0.45f, 0.95f, 0.75f),
                new Color(0.95f, 0.55f, 0.15f, 0.85f),
                Mathf.Clamp01(ratio));
        }

        public static Color NeutralCellColor => new Color(0.35f, 0.35f, 0.38f, 0.2f);
        public static Color EmptyCellColor => new Color(0.15f, 0.15f, 0.15f, 0.08f);
    }
}
