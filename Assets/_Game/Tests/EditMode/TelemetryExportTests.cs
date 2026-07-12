using System.IO;
using CleanEnergy.DebugTools;
using CleanEnergy.Telemetry;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class TelemetryExportTests
    {
        [Test]
        public void CsvHeader_ContainsExpectedColumns()
        {
            Assert.That(SessionTelemetryService.CsvHeader, Does.Contain("invalid_placements"));
            Assert.That(SessionTelemetryService.CsvHeader, Does.Contain("shortage_ratio"));
        }

        [Test]
        public void ToCsvLine_IncludesRecordedValues()
        {
            var service = new SessionTelemetryService();
            service.Reset(0f);
            service.RecordInvalidPlacement();
            service.RecordInvalidPlacement();
            service.RecordBalanceTick(5f, true, 1f);
            service.RecordBalanceTick(5f, false, 2f);
            service.RecordDebugMode(DebugViewMode.Solar);
            service.RecordScenarioEnded(true, 10f);

            var line = service.ToCsvLine();
            Assert.That(line, Does.Contain("2"));
            Assert.That(line, Does.Contain("Solar"));
            Assert.That(line, Does.Contain("0.5000"));
            Assert.That(line, Does.Contain("shortage"));
        }

        [Test]
        public void ExportToPath_WritesHeaderAndLine()
        {
            var path = Path.Combine(Path.GetTempPath(), "cleanenergy_telemetry_test.csv");
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                var service = new SessionTelemetryService();
                service.Reset(1f);
                service.RecordBuildingPlaced(3f);
                service.ExportToPath(path);

                var text = File.ReadAllText(path);
                Assert.That(text, Does.Contain(SessionTelemetryService.CsvHeader));
                Assert.That(text, Does.Contain("2.000"));
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
    }
}
