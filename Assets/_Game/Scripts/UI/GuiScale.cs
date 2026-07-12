using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// Applies <see cref="SettingsService.UiScale"/> to IMGUI via <see cref="GUI.matrix"/>.
    /// </summary>
    public static class GuiScale
    {
        public static float Current => SettingsService.UiScale;

        /// <summary>Call at the start of each main HUD <c>OnGUI</c>.</summary>
        public static void Apply()
        {
            ApplyGuiScale();
        }

        public static void ApplyGuiScale()
        {
            var scale = SettingsService.UiScale;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
        }
    }
}
