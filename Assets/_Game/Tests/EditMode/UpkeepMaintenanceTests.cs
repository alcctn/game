using CleanEnergy.Economy;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class UpkeepMaintenanceTests
    {
        [Test]
        public void MaintenanceMultiplier_FullIsOne_BrokenIsTwo()
        {
            Assert.AreEqual(1f, UpkeepService.MaintenanceMultiplier(1f), 0.001f);
            Assert.AreEqual(2f, UpkeepService.MaintenanceMultiplier(0f), 0.001f);
            Assert.AreEqual(1.5f, UpkeepService.MaintenanceMultiplier(0.5f), 0.001f);
        }
    }
}
