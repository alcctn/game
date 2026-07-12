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
        /// Draws volume, SFX mute, music volume, and zoom sensitivity. Applies changes immediately.
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
            GUILayout.Label("Music Volume");
            var music = SettingsService.MusicVolume;
            var newMusic = GUILayout.HorizontalSlider(music, 0f, 1f);
            GUILayout.Label($"{newMusic:P0}");
            if (!Mathf.Approximately(newMusic, music))
            {
                SettingsService.SetMusicVolume(newMusic);
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

            GUILayout.Space(8f);
            GUILayout.Label("Keybinds");
            DrawKeybind(RemappableAction.Pause, "Pause");
            DrawKeybind(RemappableAction.Speed1, "Speed 1x");
            DrawKeybind(RemappableAction.Speed2, "Speed 2x");
            DrawKeybind(RemappableAction.Speed3, "Speed 4x");
            DrawKeybind(RemappableAction.Undo, "Undo (with Ctrl)");
            DrawKeybind(RemappableAction.Home, "Home Fit");
        }

        private static RemappableAction? _listening;

        private static void DrawKeybind(RemappableAction action, string label)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(120f));
            var current = KeybindService.Get(action);
            var buttonLabel = _listening == action ? "Press key..." : current.ToString();
            if (GUILayout.Button(buttonLabel, GUILayout.Width(120f)))
            {
                _listening = action;
            }

            GUILayout.EndHorizontal();

            if (_listening != action)
            {
                return;
            }

            var e = Event.current;
            if (e == null || e.type != EventType.KeyDown || e.keyCode == KeyCode.None)
            {
                return;
            }

            if (KeybindService.TrySet(action, e.keyCode))
            {
                _listening = null;
            }

            e.Use();
        }
    }
}
