using CleanEnergy.CameraSystem;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// PlayerPrefs-backed audio / camera settings applied immediately.
    /// </summary>
    public static class SettingsService
    {
        public const string MasterVolumeKey = "ce_master_volume";
        public const string SfxMuteKey = "ce_sfx_mute";
        public const string ZoomSpeedKey = "ce_zoom_speed";

        public const float DefaultMasterVolume = 1f;
        public const float DefaultZoomSpeed = 10f;
        public const float MinZoomSpeed = 1f;
        public const float MaxZoomSpeed = 40f;

        public static float MasterVolume
        {
            get => Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, DefaultMasterVolume));
        }

        public static bool SfxMute
        {
            get => PlayerPrefs.GetInt(SfxMuteKey, 0) != 0;
        }

        public static float ZoomSpeed
        {
            get => Mathf.Clamp(
                PlayerPrefs.GetFloat(ZoomSpeedKey, DefaultZoomSpeed),
                MinZoomSpeed,
                MaxZoomSpeed);
        }

        public static void SetMasterVolume(float volume)
        {
            var clamped = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(MasterVolumeKey, clamped);
            PlayerPrefs.Save();
            AudioListener.volume = clamped;
        }

        public static void SetSfxMute(bool muted)
        {
            PlayerPrefs.SetInt(SfxMuteKey, muted ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void SetZoomSpeed(float speed, IsometricCameraController camera = null)
        {
            var clamped = Mathf.Clamp(speed, MinZoomSpeed, MaxZoomSpeed);
            PlayerPrefs.SetFloat(ZoomSpeedKey, clamped);
            PlayerPrefs.Save();
            if (camera != null)
            {
                camera.ZoomSpeed = clamped;
            }
        }

        /// <summary>
        /// Applies persisted prefs to AudioListener and optional camera.
        /// </summary>
        public static void ApplyAll(IsometricCameraController camera = null)
        {
            AudioListener.volume = MasterVolume;
            if (camera != null)
            {
                camera.ZoomSpeed = ZoomSpeed;
            }
        }

        /// <summary>Deletes settings keys (tests).</summary>
        public static void ClearPrefs()
        {
            PlayerPrefs.DeleteKey(MasterVolumeKey);
            PlayerPrefs.DeleteKey(SfxMuteKey);
            PlayerPrefs.DeleteKey(ZoomSpeedKey);
            PlayerPrefs.Save();
        }
    }
}
