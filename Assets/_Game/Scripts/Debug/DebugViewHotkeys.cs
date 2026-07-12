using UnityEngine;

namespace CleanEnergy.DebugTools
{
    /// <summary>
    /// Maps F1–F8 to debug view modes (GDD §3).
    /// </summary>
    public static class DebugViewHotkeys
    {
        public static bool TryMapKey(KeyCode key, out DebugViewMode mode)
        {
            switch (key)
            {
                case KeyCode.F1:
                    mode = DebugViewMode.Normal;
                    return true;
                case KeyCode.F2:
                    mode = DebugViewMode.Height;
                    return true;
                case KeyCode.F3:
                    mode = DebugViewMode.Slope;
                    return true;
                case KeyCode.F4:
                    mode = DebugViewMode.Water;
                    return true;
                case KeyCode.F5:
                    mode = DebugViewMode.Solar;
                    return true;
                case KeyCode.F6:
                    mode = DebugViewMode.Wind;
                    return true;
                case KeyCode.F7:
                    mode = DebugViewMode.Network;
                    return true;
                case KeyCode.F8:
                    mode = DebugViewMode.Production;
                    return true;
                default:
                    mode = DebugViewMode.Normal;
                    return false;
            }
        }
    }
}
