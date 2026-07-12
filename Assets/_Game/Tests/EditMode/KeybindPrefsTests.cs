using CleanEnergy.UI;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class KeybindPrefsTests
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
        public void Get_DefaultsWhenMissing()
        {
            Assert.AreEqual(KeyCode.Space, KeybindService.Get(RemappableAction.Pause));
            Assert.AreEqual(KeyCode.Z, KeybindService.Get(RemappableAction.Undo));
            Assert.AreEqual(KeyCode.Home, KeybindService.Get(RemappableAction.Home));
        }

        [Test]
        public void TrySet_RejectsFunctionKeys()
        {
            Assert.IsTrue(KeybindService.IsForbiddenDebugKey(KeyCode.F7));
            Assert.IsFalse(KeybindService.TrySet(RemappableAction.Pause, KeyCode.F1));
            Assert.AreEqual(KeyCode.Space, KeybindService.Get(RemappableAction.Pause));
        }

        [Test]
        public void ParseOrDefault_FallsBackOnInvalid()
        {
            Assert.AreEqual(KeyCode.Space, KeybindService.ParseOrDefault("not_a_key", KeyCode.Space));
            Assert.AreEqual(KeyCode.Space, KeybindService.ParseOrDefault("F5", KeyCode.Space));
            Assert.AreEqual(KeyCode.P, KeybindService.ParseOrDefault("P", KeyCode.Space));
        }

        [Test]
        public void TrySet_PersistsValidKey()
        {
            Assert.IsTrue(KeybindService.TrySet(RemappableAction.Pause, KeyCode.P));
            Assert.AreEqual(KeyCode.P, KeybindService.Get(RemappableAction.Pause));
            Assert.AreEqual("P", PlayerPrefs.GetString(KeybindService.PrefKey(RemappableAction.Pause)));
        }
    }
}
