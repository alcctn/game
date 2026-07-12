using CleanEnergy.Grid;

namespace CleanEnergy.Settlements
{
    public interface IActiveSettlementQuery
    {
        bool HasActiveSettlement { get; }
        GridCoordinate Coordinate { get; }
        int PlacementRadius { get; }
    }

    /// <summary>Active Level 1 village anchor for radius and auto-grid.</summary>
    public sealed class ActiveSettlementService : IActiveSettlementQuery
    {
        public bool HasActiveSettlement { get; private set; }
        public GridCoordinate Coordinate { get; private set; }
        public int PlacementRadius { get; private set; } = 10;
        public string InstanceId { get; private set; }

        public void Clear()
        {
            HasActiveSettlement = false;
            Coordinate = default;
            InstanceId = null;
        }

        public void Set(GridCoordinate coordinate, int radius, string instanceId = null)
        {
            HasActiveSettlement = true;
            Coordinate = coordinate;
            PlacementRadius = radius > 0 ? radius : 10;
            InstanceId = instanceId;
        }

        public int ManhattanDistance(GridCoordinate other)
        {
            var dx = Coordinate.X - other.X;
            var dy = Coordinate.Y - other.Y;
            if (dx < 0) dx = -dx;
            if (dy < 0) dy = -dy;
            return dx + dy;
        }

        public bool IsInsideRadius(GridCoordinate other)
        {
            return HasActiveSettlement && ManhattanDistance(other) <= PlacementRadius;
        }
    }
}
