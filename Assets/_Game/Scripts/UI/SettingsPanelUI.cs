using CleanEnergy.CameraSystem;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// Shared IMGUI settings fields for Main Menu and Pause overlay.
    /// </summary>
    public static class SettingsPanelUI
    {
        /// <summary>
        /// Draws volume, SFX mute, and zoom sensitivity. Applies changes immediately.
        /// </summary>
        public static void Draw(IsometricCameraController camera = null)
        {
            GUILayout.Label("Master Volume");
            var volume = SettingsService.MasterVolume;
            var newVolume = GUILayout.HorizontalSlider(volume, 0f, 1f);
            GUILayout.Label($"{newVolume:P0}");
            if (!Mathf.Approximately(newVolume, volume))
            {
                SettingsService.SetMasterVolume(newVolume);
            }

            GUILayout.Space(4f);
            var mute = SettingsService.SfxMute;
            var newMute = GUILayout.Toggle(mute, "Mute SFX");
            if (newMute != mute)
            {
                SettingsService.SetSfxMute(newMute);
            }

            GUILayout.Space(4f);
            GUILayout.Label("Scroll Zoom Sensitivity");
            var zoom = SettingsService.ZoomSpeed;
            var newZoom = GUILayout.HorizontalSlider(
                zoom, SettingsService.MinZoomSpeed, SettingsService.MaxZoomSpeed);
            GUILayout.Label($"{newZoom:F1}");
            if (!Mathf.Approximately(newZoom, zoom))
            {
                SettingsService.SetZoomSpeed(newZoom, camera);
            }
        }
    }
}
