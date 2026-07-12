using System;
using UnityEngine;

namespace CleanEnergy.Grid
{
    /// <summary>
    /// Per-cell logical map data for Sprint 01.
    /// </summary>
    [Serializable]
    public sealed class GridCellData
    {
        public int X;
        public int Y;
        public Vector3 WorldPosition;
        public float Elevation;
        /// <summary>Slope in degrees (0 = flat).</summary>
        public float Slope;
        public bool IsBuildable;
    }
}
