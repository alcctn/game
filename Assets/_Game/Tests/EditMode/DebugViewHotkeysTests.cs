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

            Assert.IsTrue(DebugViewHotkeys.TryMapKey(KeyCode.F7, out var f7));
            Assert.AreEqual(DebugViewMode.Network, f7);

            Assert.IsTrue(DebugViewHotkeys.TryMapKey(KeyCode.F8, out var f8));
            Assert.AreEqual(DebugViewMode.Production, f8);

            Assert.IsTrue(DebugViewHotkeys.TryMapKey(KeyCode.F9, out var f9));
            Assert.AreEqual(DebugViewMode.Demand, f9);
        }

        [Test]
        public void OtherKey_ReturnsFalse()
        {
            Assert.IsFalse(DebugViewHotkeys.TryMapKey(KeyCode.F10, out _));
            Assert.IsFalse(DebugViewHotkeys.TryMapKey(KeyCode.A, out _));
            Assert.IsFalse(DebugViewHotkeys.TryMapKey(KeyCode.Escape, out _));
        }
    }
}
