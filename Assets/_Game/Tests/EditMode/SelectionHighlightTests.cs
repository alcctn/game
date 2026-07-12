using CleanEnergy.DebugTools;
using CleanEnergy.Grid;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class SelectionHighlightTests
    {
        [Test]
        public void SetSelection_StoresCoordinate()
        {
            var go = new GameObject("Overlay");
            try
            {
                var overlay = go.AddComponent<MapDebugOverlay>();
                GridCoordinate? received = null;
                var count = 0;
                overlay.SelectionChanged += c =>
                {
                    received = c;
                    count++;
                };

                var cell = new GridCoordinate(3, 5);
                overlay.SetSelection(cell);

                Assert.AreEqual(cell, overlay.SelectedCell);
                Assert.AreEqual(cell, received);
                Assert.AreEqual(1, count);

                overlay.SetSelection(cell);
                Assert.AreEqual(1, count);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ClearSelection_ClearsCoordinate()
        {
            var go = new GameObject("Overlay");
            try
            {
                var overlay = go.AddComponent<MapDebugOverlay>();
                overlay.SetSelection(new GridCoordinate(1, 2));
                Assert.IsTrue(overlay.SelectedCell.HasValue);

                GridCoordinate? received = new GridCoordinate(9, 9);
                overlay.SelectionChanged += c => received = c;
                overlay.ClearSelection();

                Assert.IsFalse(overlay.SelectedCell.HasValue);
                Assert.IsFalse(received.HasValue);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void WorldCenter_MatchesGridCell()
        {
            var grid = new GridService();
            grid.Create(8, 8, 2f, Vector3.zero);
            var coordinate = new GridCoordinate(2, 3);
            grid.SetElevation(coordinate, 7.5f);
            var cell = grid.GetCell(coordinate);

            const float baseOffset = 0.5f;
            var center = SelectionHighlightGeometry.GetWorldCenter(cell, baseOffset);
            var expected = cell.WorldPosition + Vector3.up * (baseOffset + SelectionHighlightGeometry.ExtraHeightOffset);

            Assert.AreEqual(expected.x, center.x, 0.001f);
            Assert.AreEqual(expected.y, center.y, 0.001f);
            Assert.AreEqual(expected.z, center.z, 0.001f);
            Assert.AreEqual(2f * 0.48f, SelectionHighlightGeometry.GetHalfExtent(2f), 0.001f);
        }
    }
}
