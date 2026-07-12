using CleanEnergy.UI;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class CamKeybindTests
    {
        [SetUp]
        public void SetUp()
        {
            KeybindService.ClearPrefs();
        }

        [TearDown]
        public void TearDown()
        {
            KeybindService.ClearPrefs();
        }

        [Test]
        public void CameraDefaults_AreWasdAndQE()
        {
            Assert.AreEqual(KeyCode.W, KeybindService.DefaultOf(RemappableAction.CamForward));
            Assert.AreEqual(KeyCode.S, KeybindService.DefaultOf(RemappableAction.CamBack));
            Assert.AreEqual(KeyCode.A, KeybindService.DefaultOf(RemappableAction.CamLeft));
            Assert.AreEqual(KeyCode.D, KeybindService.DefaultOf(RemappableAction.CamRight));
            Assert.AreEqual(KeyCode.Q, KeybindService.DefaultOf(RemappableAction.CamRotateLeft));
            Assert.AreEqual(KeyCode.E, KeybindService.DefaultOf(RemappableAction.CamRotateRight));

            Assert.AreEqual(KeyCode.W, KeybindService.Get(RemappableAction.CamForward));
            Assert.AreEqual(KeyCode.E, KeybindService.Get(RemappableAction.CamRotateRight));
        }

        [Test]
        public void CameraRemap_RejectsFunctionKeys()
        {
            Assert.IsFalse(KeybindService.TrySet(RemappableAction.CamForward, KeyCode.F1));
            Assert.AreEqual(KeyCode.W, KeybindService.Get(RemappableAction.CamForward));
        }

        [Test]
        public void CameraRemap_PersistsValidKey()
        {
            Assert.IsTrue(KeybindService.TrySet(RemappableAction.CamForward, KeyCode.UpArrow));
            Assert.AreEqual(KeyCode.UpArrow, KeybindService.Get(RemappableAction.CamForward));
        }
    }
}
