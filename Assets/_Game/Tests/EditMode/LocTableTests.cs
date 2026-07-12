using CleanEnergy.UI;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class LocTableTests
    {
        [TearDown]
        public void TearDown()
        {
            SettingsService.ClearPrefs();
        }

        [Test]
        public void LocaleKey_IsLocked()
        {
            Assert.AreEqual("ce_locale", SettingsService.LocaleKey);
            SettingsService.ClearPrefs();
            Assert.AreEqual("en", SettingsService.Locale);
        }

        [Test]
        public void Get_ReturnsEnglishDefaults()
        {
            Assert.AreEqual("Build", StringTable.Get(StringKeys.Build, "en"));
            Assert.AreEqual("Research", StringTable.Get(StringKeys.Research, "en"));
            Assert.AreEqual("Settings", StringTable.Get(StringKeys.Settings, "en"));
            Assert.AreEqual("Paused", StringTable.Get(StringKeys.Pause, "en"));
            Assert.AreEqual("Continue", StringTable.Get(StringKeys.Continue, "en"));
        }

        [Test]
        public void Get_ReturnsTurkish()
        {
            Assert.AreEqual("İnşa", StringTable.Get(StringKeys.Build, "tr"));
            Assert.AreEqual("Araştırma", StringTable.Get(StringKeys.Research, "tr"));
            Assert.AreEqual("Ayarlar", StringTable.Get(StringKeys.Settings, "tr"));
            Assert.AreEqual("Duraklatıldı", StringTable.Get(StringKeys.Pause, "tr"));
            Assert.AreEqual("Devam", StringTable.Get(StringKeys.Continue, "tr"));
        }

        [Test]
        public void SetLocale_PersistsAndDrivesGet()
        {
            SettingsService.ClearPrefs();
            SettingsService.SetLocale("tr");
            Assert.AreEqual("tr", SettingsService.Locale);
            Assert.AreEqual("Ayarlar", StringTable.Get(StringKeys.Settings));

            SettingsService.SetLocale("en");
            Assert.AreEqual("Settings", StringTable.Get(StringKeys.Settings));
        }

        [Test]
        public void NormalizeLocale_ClampsToEnOrTr()
        {
            Assert.AreEqual("en", SettingsService.NormalizeLocale("de"));
            Assert.AreEqual("tr", SettingsService.NormalizeLocale("TR"));
            Assert.AreEqual("en", SettingsService.NormalizeLocale(null));
        }
    }
}
