using System;
using UnityEngine;

namespace CleanEnergy.Grid
{
    /// <summary>
    /// Logical grid storage and world/grid coordinate conversion.
    /// </summary>
    public sealed class GridService
    {
        private GridCellData[,] _cells;
        private int _width;
        private int _height;
        private float _cellSize;
        private Vector3 _origin;

        public int Width => _width;
        public int Height => _height;
        public float CellSize => _cellSize;
        public Vector3 Origin => _origin;
        public bool IsInitialized => _cells != null;

        /// <summary>
        /// Creates an empty grid covering [origin, origin + width*cellSize].
        /// </summary>
        public void Create(int width, int height, float cellSize, Vector3 origin)
        {
            if (width < 1 || height < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "[Grid] Width and height must be positive.");
            }

            if (cellSize <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(cellSize), "[Grid] Cell size must be positive.");
            }

            _width = width;
            _height = height;
            _cellSize = cellSize;
            _origin = origin;
            _cells = new GridCellData[width, height];

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var cell = new GridCellData
                    {
                        X = x,
                        Y = y,
                        WorldPosition = GridToWorld(new GridCoordinate(x, y)),
                        Elevation = 0f,
                        Slope = 0f,
                        IsBuildable = true
                    };
                    _cells[x, y] = cell;
                }
            }
        }

        public bool InBounds(GridCoordinate coordinate)
        {
            return coordinate.X >= 0 && coordinate.X < _width &&
                   coordinate.Y >= 0 && coordinate.Y < _height;
        }

        public bool InBounds(int x, int y) => InBounds(new GridCoordinate(x, y));

        public bool TryGetCell(GridCoordinate coordinate, out GridCellData cell)
        {
            if (!IsInitialized || !InBounds(coordinate))
            {
                cell = null;
                return false;
            }

            cell = _cells[coordinate.X, coordinate.Y];
            return true;
        }

        public GridCellData GetCell(GridCoordinate coordinate)
        {
            if (!TryGetCell(coordinate, out var cell))
            {
                throw new ArgumentOutOfRangeException(nameof(coordinate),
                    $"[Grid] Coordinate {coordinate} is out of bounds ({_width}x{_height}).");
            }

            return cell;
        }

        public Vector3 GridToWorld(GridCoordinate coordinate)
        {
            var x = _origin.x + (coordinate.X + 0.5f) * _cellSize;
            var z = _origin.z + (coordinate.Y + 0.5f) * _cellSize;
            return new Vector3(x, _origin.y, z);
        }

        /// <summary>
        /// Converts a world position to a grid coordinate. Returns false when outside the grid.
        /// </summary>
        public bool TryWorldToGrid(Vector3 worldPosition, out GridCoordinate coordinate)
        {
            var localX = worldPosition.x - _origin.x;
            var localZ = worldPosition.z - _origin.z;
            var x = Mathf.FloorToInt(localX / _cellSize);
            var y = Mathf.FloorToInt(localZ / _cellSize);
            coordinate = new GridCoordinate(x, y);
            return InBounds(coordinate);
        }

        public GridCoordinate WorldToGridClamped(Vector3 worldPosition)
        {
            var localX = worldPosition.x - _origin.x;
            var localZ = worldPosition.z - _origin.z;
            var x = Mathf.Clamp(Mathf.FloorToInt(localX / _cellSize), 0, Mathf.Max(0, _width - 1));
            var y = Mathf.Clamp(Mathf.FloorToInt(localZ / _cellSize), 0, Mathf.Max(0, _height - 1));
            return new GridCoordinate(x, y);
        }

        public void SetElevation(GridCoordinate coordinate, float elevation)
        {
            var cell = GetCell(coordinate);
            cell.Elevation = elevation;
            var world = cell.WorldPosition;
            cell.WorldPosition = new Vector3(world.x, elevation, world.z);
        }

        public void SetSlope(GridCoordinate coordinate, float slopeDegrees, bool isBuildable)
        {
            var cell = GetCell(coordinate);
            cell.Slope = slopeDegrees;
            cell.IsBuildable = isBuildable;
        }
    }
}
