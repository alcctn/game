using CleanEnergy.DebugTools;
using CleanEnergy.Telemetry;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class SessionTelemetryTests
    {
        [Test]
        public void Reset_ClearsCounters()
        {
            var service = new SessionTelemetryService();
            service.Reset(0f);
            service.RecordInvalidPlacement();
            service.RecordBuildingPlaced(2f);
            service.RecordBalanceTick(5f, true, 3f);
            service.RecordDebugMode(DebugViewMode.Solar);
            service.RecordScenarioEnded(true, 10f);

            service.Reset(20f);

            Assert.AreEqual(20f, service.SessionStartTime, 0.001f);
            Assert.IsNull(service.TimeToFirstBuildingSeconds);
            Assert.IsNull(service.TimeToFirstProductionSeconds);
            Assert.AreEqual(0, service.InvalidPlacementAttempts);
            Assert.AreEqual(0, service.BalanceTicks);
            Assert.AreEqual(0, service.ShortageTicks);
            Assert.IsNull(service.ScenarioEndElapsedSeconds);
            Assert.AreEqual(string.Empty, service.FailReason);
            Assert.AreEqual(DebugViewMode.Normal, service.PreferredDebugLayer);
        }

        [Test]
        public void InvalidPlacement_Increments()
        {
            var service = new SessionTelemetryService();
            service.Reset(0f);
            service.RecordInvalidPlacement();
            service.RecordInvalidPlacement();
            Assert.AreEqual(2, service.InvalidPlacementAttempts);
        }

        [Test]
        public void FirstProduction_RecordsOnce()
        {
            var service = new SessionTelemetryService();
            service.Reset(10f);
            service.RecordBalanceTick(0f, false, 11f);
            Assert.IsNull(service.TimeToFirstProductionSeconds);

            service.RecordBalanceTick(4f, false, 15f);
            Assert.AreEqual(5f, service.TimeToFirstProductionSeconds.Value, 0.001f);

            service.RecordBalanceTick(8f, false, 20f);
            Assert.AreEqual(5f, service.TimeToFirstProductionSeconds.Value, 0.001f);
        }

        [Test]
        public void ShortageRatio_UsesBalanceTicks()
        {
            var service = new SessionTelemetryService();
            service.Reset(0f);
            service.RecordBalanceTick(1f, true, 1f);
            service.RecordBalanceTick(1f, false, 2f);
            service.RecordBalanceTick(1f, true, 3f);
            service.RecordBalanceTick(1f, false, 4f);

            Assert.AreEqual(4, service.BalanceTicks);
            Assert.AreEqual(2, service.ShortageTicks);
            Assert.AreEqual(0.5f, service.AverageShortageRatio, 0.001f);
        }

        [Test]
        public void PreferredLayer_IgnoresNormal()
        {
            var service = new SessionTelemetryService();
            service.Reset(0f);
            service.RecordDebugMode(DebugViewMode.Normal);
            service.RecordDebugMode(DebugViewMode.Normal);
            service.RecordDebugMode(DebugViewMode.Water);
            service.RecordDebugMode(DebugViewMode.Solar);
            service.RecordDebugMode(DebugViewMode.Solar);

            Assert.AreEqual(DebugViewMode.Solar, service.PreferredDebugLayer);
        }

        [Test]
        public void ScenarioFail_SetsReasonOnce()
        {
            var service = new SessionTelemetryService();
            service.Reset(5f);
            service.RecordScenarioEnded(true, 15f);
            Assert.AreEqual(10f, service.ScenarioEndElapsedSeconds.Value, 0.001f);
            Assert.AreEqual("shortage", service.FailReason);

            service.RecordScenarioEnded(false, 20f);
            Assert.AreEqual(10f, service.ScenarioEndElapsedSeconds.Value, 0.001f);
            Assert.AreEqual("shortage", service.FailReason);
        }
    }
}
