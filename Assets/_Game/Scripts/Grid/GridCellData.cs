using System;
using CleanEnergy.Map;
using UnityEngine;

namespace CleanEnergy.Grid
{
    /// <summary>
    /// Per-cell logical map data including Sprint 02 resource layers.
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
        /// <summary>Downslope aspect direction in XZ (normalized when sloped).</summary>
        public Vector2 Aspect;
        /// <summary>Raw upstream accumulation used as water flow / discharge proxy.</summary>
        public float WaterFlow;
        /// <summary>Normalized flow direction to the steepest descent neighbor (grid delta).</summary>
        public Vector2Int FlowDirection;
        public float SolarPotential;
        public float WindPotential;
        public BiomeType Biome;
        public bool IsWater;
        public bool IsBuildable;
        public string OccupyingBuildingId;
    }
}
