using UnityEngine;

namespace CleanEnergy.Energy
{
    /// <summary>
    /// Producer output ratio for production debug coloring.
    /// </summary>
    public static class ProductionUtilization
    {
        public static float ComputeRatio(float currentProduction, float installedPower)
        {
            if (installedPower <= 0.0001f)
            {
                return 0f;
            }

            return Mathf.Clamp01(currentProduction / installedPower);
        }

        public static Color ColorForRatio(float ratio)
        {
            var t = Mathf.Clamp01(ratio);
            if (t < 0.5f)
            {
                return Color.Lerp(
                    new Color(0.9f, 0.15f, 0.1f, 0.85f),
                    new Color(0.95f, 0.85f, 0.2f, 0.8f),
                    t * 2f);
            }

            return Color.Lerp(
                new Color(0.95f, 0.85f, 0.2f, 0.8f),
                new Color(0.25f, 0.85f, 0.35f, 0.75f),
                (t - 0.5f) * 2f);
        }

        public static Color NeutralCellColor => new Color(0.35f, 0.35f, 0.38f, 0.2f);
        public static Color EmptyCellColor => new Color(0.15f, 0.15f, 0.15f, 0.08f);
    }
}
