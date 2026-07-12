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
    }
}
