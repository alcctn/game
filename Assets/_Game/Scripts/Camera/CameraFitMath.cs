using System.Collections.Generic;
using UnityEngine;

namespace CleanEnergy.CameraSystem
{
    /// <summary>
    /// Pure helpers for zoom-to-fit orthographic sizing.
    /// </summary>
    public static class CameraFitMath
    {
        public const int SelectionPaddingCells = 4;

        public static float OrthographicSizeForBounds(
            Bounds worldBounds,
            float aspect,
            float minSize,
            float maxSize)
        {
            var halfHeight = Mathf.Max(0.01f, worldBounds.extents.z);
            var halfWidth = Mathf.Max(0.01f, worldBounds.extents.x);
            var safeAspect = Mathf.Max(0.01f, aspect);
            var sizeFromHeight = halfHeight;
            var sizeFromWidth = halfWidth / safeAspect;
            var size = Mathf.Max(sizeFromHeight, sizeFromWidth);
            return Mathf.Clamp(size, minSize, maxSize);
        }

        public static Bounds BoundsAroundCell(Vector3 cellWorld, float cellSize, int paddingCells)
        {
            var pad = Mathf.Max(0, paddingCells) * Mathf.Max(0.01f, cellSize);
            var extents = new Vector3(pad, 0f, pad);
            return new Bounds(cellWorld, extents * 2f);
        }

        /// <summary>
        /// Axis-aligned bounds covering one or more cell centers with padding.
        /// </summary>
        public static Bounds BoundsAroundCells(
            IReadOnlyList<Vector3> cellWorldPositions,
            float cellSize,
            int paddingCells)
        {
            if (cellWorldPositions == null || cellWorldPositions.Count == 0)
            {
                return BoundsAroundCell(Vector3.zero, cellSize, paddingCells);
            }

            var bounds = new Bounds(cellWorldPositions[0], Vector3.zero);
            for (var i = 1; i < cellWorldPositions.Count; i++)
            {
                bounds.Encapsulate(cellWorldPositions[i]);
            }

            var pad = Mathf.Max(0, paddingCells) * Mathf.Max(0.01f, cellSize);
            bounds.Expand(new Vector3(pad * 2f, 0f, pad * 2f));
            return bounds;
        }
    }
}
