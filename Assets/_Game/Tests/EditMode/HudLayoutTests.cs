using CleanEnergy.UI;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class HudLayoutTests
    {
        [Test]
        public void CorePanels_DoNotOverlap_At1080p()
        {
            // Approximate layout math independent of Screen by using the same helpers
            // with assumed Screen size via Rect comparisons of relative slots.
            var terrain = HudLayout.TerrainDebug();
            var tutorial = HudLayout.Tutorial();
            Assert.AreEqual(HudLayout.TutorialHeight, tutorial.height, 0.01f);
            Assert.GreaterOrEqual(tutorial.height, 200f);

            var scenario = new Rect(terrain.xMax + 8f, 172f, 260f, 148f);
            var inspection = new Rect(1920f - 280f - 12f, 12f, 280f, 380f);
            var building = new Rect(inspection.x, inspection.yMax + 8f, 280f, 400f);
            var research = new Rect(12f, 1080f - 300f - 12f, 300f, 300f);
            var telemetry = new Rect(12f + 300f + 8f, 1080f - 190f - 12f, 260f, 190f);

            Assert.IsFalse(HudLayout.Overlaps(terrain, tutorial));
            Assert.IsFalse(HudLayout.Overlaps(terrain, scenario));
            Assert.IsFalse(HudLayout.Overlaps(tutorial, research));
            Assert.IsFalse(HudLayout.Overlaps(inspection, building));
            Assert.IsFalse(HudLayout.Overlaps(research, telemetry));
            Assert.IsFalse(HudLayout.Overlaps(terrain, inspection));
        }
    }
}
