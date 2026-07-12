using CleanEnergy.Core;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class BootstrapFlowTests
    {
        [Test]
        public void SceneNames_IncludeBootstrapFirst()
        {
            Assert.AreEqual("Bootstrap", SceneFlow.BootstrapSceneName);
            Assert.AreEqual("MainMenu", SceneFlow.MainMenuSceneName);
            Assert.AreEqual("Test_Terrain", SceneFlow.PlaySceneName);

            var order = SceneFlow.BuildOrderSceneNames();
            Assert.AreEqual(3, order.Length);
            Assert.AreEqual(SceneFlow.BootstrapSceneName, order[0]);
            Assert.AreEqual(SceneFlow.MainMenuSceneName, order[1]);
            Assert.AreEqual(SceneFlow.PlaySceneName, order[2]);
        }

        [Test]
        public void IsBootstrapScene_MatchesName()
        {
            Assert.IsTrue(SceneFlow.IsBootstrapScene("Bootstrap"));
            Assert.IsFalse(SceneFlow.IsBootstrapScene("MainMenu"));
            Assert.IsFalse(SceneFlow.IsBootstrapScene(null));
        }
    }
}
