using CleanEnergy.Core;
using CleanEnergy.Simulation;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class PlayHotkeysTests
    {
        [Test]
        public void TogglePause_SwitchesPausedAndOne()
        {
            Assert.AreEqual(SimulationSpeed.One, PlayHotkeys.ResolveTogglePause(SimulationSpeed.Paused));
            Assert.AreEqual(SimulationSpeed.Paused, PlayHotkeys.ResolveTogglePause(SimulationSpeed.One));
            Assert.AreEqual(SimulationSpeed.Paused, PlayHotkeys.ResolveTogglePause(SimulationSpeed.Two));
        }
    }
}
