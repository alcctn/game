using UnityEngine;

namespace CleanEnergy.Energy
{
    /// <summary>
    /// Hub capacity utilization for network debug coloring.
    /// </summary>
    public static class NetworkUtilization
    {
        public static float Compute(float deliveredProduction, float linkCapacity)
        {
            if (linkCapacity <= 0.0001f)
            {
                return 0f;
            }

            return Mathf.Clamp01(deliveredProduction / linkCapacity);
        }

        public static Color ColorForUtilization(float utilization)
        {
            return Color.Lerp(
                new Color(0.25f, 0.85f, 0.35f, 0.75f),
                new Color(0.9f, 0.15f, 0.1f, 0.85f),
                Mathf.Clamp01(utilization));
        }

        public static Color EnergyNodeColor => new Color(0.25f, 0.45f, 0.95f, 0.55f);
        public static Color EmptyCellColor => new Color(0.15f, 0.15f, 0.15f, 0.12f);
    }
}
