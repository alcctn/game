using CleanEnergy.Audio;
using CleanEnergy.UI;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class SfxServiceTests
    {
        [TearDown]
        public void TearDown()
        {
            SettingsService.ClearPrefs();
            AudioListener.volume = 1f;
        }

        [Test]
        public void Play_WhenMuted_DoesNotIncrementPlayCount()
        {
            SettingsService.ClearPrefs();
            var go = new GameObject("SfxTest");
            try
            {
                var sfx = go.AddComponent<SfxService>();
                SfxService.IsMuted = true;
                sfx.ResetPlayCount();

                sfx.Play(SfxId.Place);

                Assert.AreEqual(0, sfx.PlayCount);
                Assert.IsTrue(sfx.LastPlayBlocked);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Play_WhenUnmuted_IncrementsPlayCount()
        {
            SettingsService.ClearPrefs();
            var go = new GameObject("SfxTest");
            try
            {
                var sfx = go.AddComponent<SfxService>();
                SfxService.IsMuted = false;
                sfx.ResetPlayCount();

                sfx.Play(SfxId.Place);
                sfx.Play(SfxId.Demolish);

                Assert.AreEqual(2, sfx.PlayCount);
                Assert.IsFalse(sfx.LastPlayBlocked);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Play_WhenSuppressed_BlocksPlayback()
        {
            SettingsService.ClearPrefs();
            var go = new GameObject("SfxTest");
            try
            {
                var sfx = go.AddComponent<SfxService>();
                SfxService.IsMuted = false;
                sfx.SuppressPlayback = true;
                sfx.ResetPlayCount();

                sfx.Play(SfxId.Shortage);

                Assert.AreEqual(0, sfx.PlayCount);
                Assert.IsTrue(sfx.LastPlayBlocked);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void MutePrefsKey_MatchesSettingsService()
        {
            Assert.AreEqual(SettingsService.SfxMuteKey, SfxService.MutePrefsKey);
            Assert.AreEqual("ce_sfx_mute", SfxService.MutePrefsKey);
        }
    }
}
