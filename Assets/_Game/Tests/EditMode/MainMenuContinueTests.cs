using CleanEnergy.Save;
using CleanEnergy.Scenario;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class MainMenuContinueTests
    {
        [Test]
        public void ConsumeLoadSaveOnPlay_ClearsFlag()
        {
            ScenarioSession.LoadSaveOnPlay = true;
            Assert.IsTrue(ScenarioSession.ConsumeLoadSaveOnPlay());
            Assert.IsFalse(ScenarioSession.LoadSaveOnPlay);
            Assert.IsFalse(ScenarioSession.ConsumeLoadSaveOnPlay());
        }

        [Test]
        public void Slot1_FileNameIsStable()
        {
            Assert.AreEqual("slot1.json", SaveGameService.SlotFileName(1));
            Assert.AreEqual("slot1.json", SaveGameService.DefaultSlotFileName);
        }
    }
}
