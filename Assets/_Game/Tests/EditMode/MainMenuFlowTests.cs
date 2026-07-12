using CleanEnergy.Core;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class MainMenuFlowTests
    {
        [Test]
        public void SceneNames_AreStable()
        {
            Assert.AreEqual("MainMenu", SceneFlow.MainMenuSceneName);
            Assert.AreEqual("Test_Terrain", SceneFlow.PlaySceneName);
        }
    }
}
