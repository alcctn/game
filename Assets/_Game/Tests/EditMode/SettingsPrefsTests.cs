using CleanEnergy.CameraSystem;
using CleanEnergy.UI;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class SettingsPrefsTests
    {
        [TearDown]
        public void TearDown()
        {
            SettingsService.ClearPrefs();
            AudioListener.volume = 1f;
        }

        [Test]
        public void SetMasterVolume_PersistsAndAppliesToAudioListener()
        {
            SettingsService.ClearPrefs();
            SettingsService.SetMasterVolume(0.42f);

            Assert.AreEqual(0.42f, SettingsService.MasterVolume, 0.001f);
            Assert.AreEqual(0.42f, AudioListener.volume, 0.001f);
            Assert.AreEqual(0.42f, PlayerPrefs.GetFloat(SettingsService.MasterVolumeKey), 0.001f);
        }

        [Test]
        public void SetSfxMute_PersistsToPlayerPrefs()
        {
            SettingsService.ClearPrefs();
            Assert.IsFalse(SettingsService.SfxMute);

            SettingsService.SetSfxMute(true);
            Assert.IsTrue(SettingsService.SfxMute);
            Assert.AreEqual(1, PlayerPrefs.GetInt(SettingsService.SfxMuteKey));

            SettingsService.SetSfxMute(false);
            Assert.IsFalse(SettingsService.SfxMute);
            Assert.AreEqual(0, PlayerPrefs.GetInt(SettingsService.SfxMuteKey));
        }

        [Test]
        public void SetZoomSpeed_PersistsAndAppliesToCamera()
        {
            SettingsService.ClearPrefs();
            var go = new GameObject("CamSettings");
            try
            {
                go.AddComponent<Camera>();
                var camera = go.AddComponent<IsometricCameraController>();
                SettingsService.SetZoomSpeed(17.5f, camera);

                Assert.AreEqual(17.5f, SettingsService.ZoomSpeed, 0.001f);
                Assert.AreEqual(17.5f, camera.ZoomSpeed, 0.001f);
                Assert.AreEqual(17.5f, PlayerPrefs.GetFloat(SettingsService.ZoomSpeedKey), 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ApplyAll_RestoresVolumeAndZoomFromPrefs()
        {
            SettingsService.ClearPrefs();
            PlayerPrefs.SetFloat(SettingsService.MasterVolumeKey, 0.55f);
            PlayerPrefs.SetFloat(SettingsService.ZoomSpeedKey, 22f);
            PlayerPrefs.Save();

            var go = new GameObject("CamApply");
            try
            {
                go.AddComponent<Camera>();
                var camera = go.AddComponent<IsometricCameraController>();
                camera.ZoomSpeed = 10f;
                AudioListener.volume = 1f;

                SettingsService.ApplyAll(camera);

                Assert.AreEqual(0.55f, AudioListener.volume, 0.001f);
                Assert.AreEqual(22f, camera.ZoomSpeed, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void PrefsKeys_AreLockedNames()
        {
            Assert.AreEqual("ce_master_volume", SettingsService.MasterVolumeKey);
            Assert.AreEqual("ce_sfx_mute", SettingsService.SfxMuteKey);
            Assert.AreEqual("ce_music_volume", SettingsService.MusicVolumeKey);
            Assert.AreEqual("ce_zoom_speed", SettingsService.ZoomSpeedKey);
            Assert.AreEqual("ce_ui_scale", SettingsService.UiScaleKey);
            Assert.AreEqual("ce_locale", SettingsService.LocaleKey);
        }

        [Test]
        public void SetMusicVolume_PersistsAndDoesNotRequireSfxUnmute()
        {
            SettingsService.ClearPrefs();
            SettingsService.SetSfxMute(true);
            SettingsService.SetMusicVolume(0.33f);

            Assert.AreEqual(0.33f, SettingsService.MusicVolume, 0.001f);
            Assert.AreEqual(0.33f, PlayerPrefs.GetFloat(SettingsService.MusicVolumeKey), 0.001f);
            Assert.IsTrue(SettingsService.SfxMute);
        }
    }
}
