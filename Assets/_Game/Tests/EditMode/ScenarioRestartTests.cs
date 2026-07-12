using CleanEnergy.Core;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class ScenarioRestartTests
    {
        [Test]
        public void PlaySceneName_IsTestTerrain()
        {
            Assert.AreEqual("Test_Terrain", SceneFlow.PlaySceneName);
        }
    }
}
