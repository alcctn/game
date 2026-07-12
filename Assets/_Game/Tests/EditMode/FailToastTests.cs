using CleanEnergy.Scenario;
using CleanEnergy.UI;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class FailToastTests
    {
        [Test]
        public void FormatFailToast_ParityWithWinStyle()
        {
            Assert.AreEqual("Scenario failed", NotificationController.FormatFailToast(null));
            Assert.AreEqual("Scenario failed", NotificationController.FormatFailToast(string.Empty));
            Assert.AreEqual(
                "Scenario failed — Village satisfaction collapsed from prolonged shortages.",
                NotificationController.FormatFailToast(ScenarioFailedEvent.DefaultReason));
        }

        [Test]
        public void ScenarioFailedEvent_CarriesDefaultReason()
        {
            var evt = new ScenarioFailedEvent("green_valley");
            Assert.AreEqual("green_valley", evt.ScenarioId);
            Assert.AreEqual(ScenarioFailedEvent.DefaultReason, evt.Reason);
        }

        [Test]
        public void ScenarioHud_ReturnToMenu_ExistsLikeWin()
        {
            var go = new GameObject("FailToastHud");
            try
            {
                var hud = go.AddComponent<ScenarioHudUI>();
                Assert.IsNotNull(hud);
                Assert.AreEqual(
                    typeof(void),
                    typeof(ScenarioHudUI).GetMethod(nameof(ScenarioHudUI.ReturnToMainMenu)).ReturnType);
                Assert.AreEqual(
                    typeof(void),
                    typeof(ScenarioHudUI).GetMethod(nameof(ScenarioHudUI.RestartScenario)).ReturnType);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
