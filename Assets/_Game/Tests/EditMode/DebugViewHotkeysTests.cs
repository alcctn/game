using CleanEnergy.DebugTools;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class DebugViewHotkeysTests
    {
        [Test]
        public void F1toF6_MapToModes()
        {
            Assert.IsTrue(DebugViewHotkeys.TryMapKey(KeyCode.F1, out var f1));
            Assert.AreEqual(DebugViewMode.Normal, f1);

            Assert.IsTrue(DebugViewHotkeys.TryMapKey(KeyCode.F2, out var f2));
            Assert.AreEqual(DebugViewMode.Height, f2);

            Assert.IsTrue(DebugViewHotkeys.TryMapKey(KeyCode.F3, out var f3));
            Assert.AreEqual(DebugViewMode.Slope, f3);

            Assert.IsTrue(DebugViewHotkeys.TryMapKey(KeyCode.F4, out var f4));
            Assert.AreEqual(DebugViewMode.Water, f4);

            Assert.IsTrue(DebugViewHotkeys.TryMapKey(KeyCode.F5, out var f5));
            Assert.AreEqual(DebugViewMode.Solar, f5);

            Assert.IsTrue(DebugViewHotkeys.TryMapKey(KeyCode.F6, out var f6));
            Assert.AreEqual(DebugViewMode.Wind, f6);
        }

        [Test]
        public void OtherKey_ReturnsFalse()
        {
            Assert.IsFalse(DebugViewHotkeys.TryMapKey(KeyCode.F7, out _));
            Assert.IsFalse(DebugViewHotkeys.TryMapKey(KeyCode.F8, out _));
            Assert.IsFalse(DebugViewHotkeys.TryMapKey(KeyCode.A, out _));
            Assert.IsFalse(DebugViewHotkeys.TryMapKey(KeyCode.Escape, out _));
        }
    }
}
