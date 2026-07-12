using CleanEnergy.Audio;
using CleanEnergy.UI;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class MusicServiceTests
    {
        [TearDown]
        public void TearDown()
        {
            SettingsService.ClearPrefs();
            AudioListener.volume = 1f;
        }

        [Test]
        public void VolumePrefsKey_IsLockedName()
        {
            Assert.AreEqual("ce_music_volume", MusicService.VolumePrefsKey);
            Assert.AreEqual(SettingsService.MusicVolumeKey, MusicService.VolumePrefsKey);
        }

        [Test]
        public void NullClip_IsNoOp_DoesNotPlay()
        {
            SettingsService.ClearPrefs();
            var go = new GameObject("MusicNull");
            try
            {
                var music = go.AddComponent<MusicService>();
                music.Configure(null);
                Assert.IsFalse(music.IsPlaying);
                Assert.IsNull(music.LoopClip);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ApplyVolume_UsesMusicVolume_IndependentOfSfxMute()
        {
            SettingsService.ClearPrefs();
            SettingsService.SetSfxMute(true);
            SettingsService.SetMusicVolume(0.4f);

            var go = new GameObject("MusicVol");
            try
            {
                var music = go.AddComponent<MusicService>();
                var clip = AudioClip.Create("t", 8, 1, 22050, false);
                music.Configure(clip);
                music.ApplyVolume();

                var source = go.GetComponent<AudioSource>();
                Assert.IsNotNull(source);
                Assert.AreEqual(0.4f, source.volume, 0.001f);
                Assert.IsTrue(SettingsService.SfxMute);
                Assert.IsTrue(music.IsPlaying);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ApplyAll_AppliesMusicVolumeFromPrefs()
        {
            SettingsService.ClearPrefs();
            PlayerPrefs.SetFloat(SettingsService.MusicVolumeKey, 0.25f);
            PlayerPrefs.Save();

            var go = new GameObject("MusicApplyAll");
            try
            {
                var music = go.AddComponent<MusicService>();
                var clip = AudioClip.Create("t2", 8, 1, 22050, false);
                music.Configure(clip);
                var source = go.GetComponent<AudioSource>();
                source.volume = 1f;

                SettingsService.ApplyAll();
                Assert.AreEqual(0.25f, source.volume, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
