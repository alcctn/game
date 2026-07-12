using System.Collections.Generic;
using CleanEnergy.Energy;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class ProductionSparklineTests
    {
        [Test]
        public void PushSample_WrapsAtCapacity()
        {
            var buffer = new float[ProductionSparklineTracker.SampleCapacity];
            var next = 0;
            var count = 0;
            for (var i = 0; i < 25; i++)
            {
                ProductionSparklineTracker.PushSample(buffer, ref next, ref count, i);
            }

            Assert.AreEqual(ProductionSparklineTracker.SampleCapacity, count);
            var list = new List<float>();
            ProductionSparklineTracker.CopyChronological(buffer, next, count, list);
            Assert.AreEqual(20, list.Count);
            Assert.AreEqual(5f, list[0], 0.001f);
            Assert.AreEqual(24f, list[19], 0.001f);
        }

        [Test]
        public void Tracker_CapsTrackedBuildings()
        {
            var tracker = new ProductionSparklineTracker();
            for (var i = 0; i < ProductionSparklineTracker.MaxTrackedBuildings + 5; i++)
            {
                tracker.Record("id_" + i, i);
            }

            Assert.AreEqual(ProductionSparklineTracker.MaxTrackedBuildings, tracker.TrackedCount);
            Assert.IsFalse(tracker.TryGetSamples("id_0", new List<float>()));
            Assert.IsTrue(tracker.TryGetSamples("id_5", new List<float>()));
        }
    }
}
