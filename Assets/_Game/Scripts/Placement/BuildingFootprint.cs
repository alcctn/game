using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using UnityEngine;

namespace CleanEnergy.Placement
{
    /// <summary>
    /// Computes rotated building footprints (anchor = min X/Y corner).
    /// </summary>
    public static class BuildingFootprint
    {
        /// <summary>
        /// Returns footprint size with axes swapped on odd 90° rotations.
        /// </summary>
        public static Vector2Int GetFootprintSize(BuildingDefinition definition, int rotation)
        {
            var size = definition != null ? definition.Size : Vector2Int.one;
            if (size.x < 1)
            {
                size.x = 1;
            }

            if (size.y < 1)
            {
                size.y = 1;
            }

            var rot = ((rotation % 4) + 4) % 4;
            return (rot % 2 == 1) ? new Vector2Int(size.y, size.x) : size;
        }

        /// <summary>
        /// Cells occupied by a footprint with the given size, starting at anchor.
        /// </summary>
        public static List<GridCoordinate> GetCells(GridCoordinate anchor, Vector2Int size)
        {
            var width = Mathf.Max(1, size.x);
            var height = Mathf.Max(1, size.y);
            var cells = new List<GridCoordinate>(width * height);
            for (var dx = 0; dx < width; dx++)
            {
                for (var dy = 0; dy < height; dy++)
                {
                    cells.Add(new GridCoordinate(anchor.X + dx, anchor.Y + dy));
                }
            }

            return cells;
        }

        /// <summary>
        /// Cells occupied by a building definition at anchor with rotation.
        /// </summary>
        public static List<GridCoordinate> GetCells(
            BuildingDefinition definition,
            GridCoordinate anchor,
            int rotation)
        {
            return GetCells(anchor, GetFootprintSize(definition, rotation));
        }
    }
}
