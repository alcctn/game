using UnityEngine;

namespace CleanEnergy.CameraSystem
{
    /// <summary>
    /// Pure helpers for focus tween progress.
    /// </summary>
    public static class CameraFocusMath
    {
        public static float SmoothStep01(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * (3f - 2f * t);
        }

        public static Vector3 LerpFocus(Vector3 from, Vector3 to, float normalizedTime)
        {
            return Vector3.LerpUnclamped(from, to, SmoothStep01(normalizedTime));
        }
    }
}
