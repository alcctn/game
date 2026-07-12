using CleanEnergy.UI;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class UiScaleTests
    {
        [TearDown]
        public void TearDown()
        {
            SettingsService.ClearPrefs();
        }

        [Test]
        public void DefaultUiScale_IsOne()
        {
            SettingsService.ClearPrefs();
            Assert.AreEqual(1f, SettingsService.UiScale, 0.001f);
            Assert.AreEqual("ce_ui_scale", SettingsService.UiScaleKey);
        }

        [Test]
        public void SetUiScale_SnapsToAllowedSteps()
        {
            SettingsService.ClearPrefs();
            SettingsService.SetUiScale(0.85f);
            Assert.AreEqual(0.85f, SettingsService.UiScale, 0.001f);

            SettingsService.SetUiScale(1.25f);
            Assert.AreEqual(1.25f, SettingsService.UiScale, 0.001f);

            SettingsService.SetUiScale(0.9f);
            Assert.AreEqual(0.85f, SettingsService.UiScale, 0.001f);

            SettingsService.SetUiScale(1.1f);
            Assert.AreEqual(1f, SettingsService.UiScale, 0.001f);

            SettingsService.SetUiScale(1.2f);
            Assert.AreEqual(1.25f, SettingsService.UiScale, 0.001f);
        }

        [Test]
        public void SnapUiScale_ChoosesNearest()
        {
            Assert.AreEqual(0.85f, SettingsService.SnapUiScale(0.7f), 0.001f);
            Assert.AreEqual(1f, SettingsService.SnapUiScale(1.05f), 0.001f);
            Assert.AreEqual(1.25f, SettingsService.SnapUiScale(2f), 0.001f);
        }

        [Test]
        public void ApplyGuiScale_SetsMatrixFromPrefs()
        {
            SettingsService.ClearPrefs();
            SettingsService.SetUiScale(1.25f);
            GuiScale.ApplyGuiScale();
            Assert.AreEqual(1.25f, GUI.matrix.m00, 0.001f);
            Assert.AreEqual(1.25f, GUI.matrix.m11, 0.001f);
            GUI.matrix = Matrix4x4.identity;
        }
    }
}
