using CleanEnergy.Audio;
using CleanEnergy.CameraSystem;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// PlayerPrefs-backed audio / camera / UI settings applied immediately.
    /// </summary>
    public static class SettingsService
    {
        public const string MasterVolumeKey = "ce_master_volume";
        public const string SfxMuteKey = "ce_sfx_mute";
        public const string MusicVolumeKey = "ce_music_volume";
        public const string ZoomSpeedKey = "ce_zoom_speed";
        public const string UiScaleKey = "ce_ui_scale";
        public const string LocaleKey = "ce_locale";

        public const float DefaultMasterVolume = 1f;
        public const float DefaultMusicVolume = 0.6f;
        public const float DefaultZoomSpeed = 10f;
        public const float DefaultUiScale = 1f;
        public const string DefaultLocale = "en";
        public const float MinZoomSpeed = 1f;
        public const float MaxZoomSpeed = 40f;

        public static readonly float[] UiScaleSteps = { 0.85f, 1f, 1.25f };

        public static float MasterVolume
        {
            get => Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, DefaultMasterVolume));
        }

        public static bool SfxMute
        {
            get => PlayerPrefs.GetInt(SfxMuteKey, 0) != 0;
        }

        public static float MusicVolume
        {
            get => Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, DefaultMusicVolume));
        }

        public static float ZoomSpeed
        {
            get => Mathf.Clamp(
                PlayerPrefs.GetFloat(ZoomSpeedKey, DefaultZoomSpeed),
                MinZoomSpeed,
                MaxZoomSpeed);
        }

        public static float UiScale
        {
            get => SnapUiScale(PlayerPrefs.GetFloat(UiScaleKey, DefaultUiScale));
        }

        public static string Locale
        {
            get
            {
                var raw = PlayerPrefs.GetString(LocaleKey, DefaultLocale);
                return NormalizeLocale(raw);
            }
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

        public static void SetMusicVolume(float volume)
        {
            var clamped = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(MusicVolumeKey, clamped);
            PlayerPrefs.Save();
            MusicService.ApplyVolumeFromPrefs();
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

        public static void SetUiScale(float scale)
        {
            var snapped = SnapUiScale(scale);
            PlayerPrefs.SetFloat(UiScaleKey, snapped);
            PlayerPrefs.Save();
        }

        public static void SetLocale(string locale)
        {
            var normalized = NormalizeLocale(locale);
            PlayerPrefs.SetString(LocaleKey, normalized);
            PlayerPrefs.Save();
        }

        /// <summary>Clamps to nearest of 0.85 / 1.0 / 1.25.</summary>
        public static float SnapUiScale(float scale)
        {
            var best = UiScaleSteps[0];
            var bestDist = Mathf.Abs(scale - best);
            for (var i = 1; i < UiScaleSteps.Length; i++)
            {
                var dist = Mathf.Abs(scale - UiScaleSteps[i]);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = UiScaleSteps[i];
                }
            }

            return best;
        }

        public static string NormalizeLocale(string locale)
        {
            if (string.IsNullOrEmpty(locale))
            {
                return DefaultLocale;
            }

            var lower = locale.Trim().ToLowerInvariant();
            if (lower == "tr" || lower.StartsWith("tr-") || lower.StartsWith("tr_"))
            {
                return "tr";
            }

            return "en";
        }

        /// <summary>
        /// Applies persisted prefs to AudioListener, music, and optional camera.
        /// </summary>
        public static void ApplyAll(IsometricCameraController camera = null)
        {
            AudioListener.volume = MasterVolume;
            MusicService.ApplyVolumeFromPrefs();
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
            PlayerPrefs.DeleteKey(MusicVolumeKey);
            PlayerPrefs.DeleteKey(ZoomSpeedKey);
            PlayerPrefs.DeleteKey(UiScaleKey);
            PlayerPrefs.DeleteKey(LocaleKey);
            KeybindService.ClearPrefs();
            PlayerPrefs.Save();
        }
    }
}
