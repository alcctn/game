using CleanEnergy.DebugTools;
using CleanEnergy.Grid;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class PlacementCameraFocusTests
    {
        [Test]
        public void ShouldFocus_WhenHoverChangesDuringPlacement()
        {
            Assert.IsTrue(SelectionCameraFocus.ShouldFocusPlacementHover(
                true, new GridCoordinate(1, 2), null));
            Assert.IsTrue(SelectionCameraFocus.ShouldFocusPlacementHover(
                true, new GridCoordinate(2, 2), new GridCoordinate(1, 2)));
            Assert.IsFalse(SelectionCameraFocus.ShouldFocusPlacementHover(
                true, new GridCoordinate(1, 2), new GridCoordinate(1, 2)));
            Assert.IsFalse(SelectionCameraFocus.ShouldFocusPlacementHover(
                false, new GridCoordinate(1, 2), null));
            Assert.IsFalse(SelectionCameraFocus.ShouldFocusPlacementHover(
                true, null, new GridCoordinate(1, 2)));
        }
    }
}
