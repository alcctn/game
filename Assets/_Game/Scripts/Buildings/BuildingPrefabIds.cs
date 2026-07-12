namespace CleanEnergy.Buildings
{
    /// <summary>
    /// Canonical building ids that receive editor placeholder prefabs under Prefabs/Buildings.
    /// </summary>
    public static class BuildingPrefabIds
    {
        /// <summary>All nine prototype buildings with persisted placeholder prefabs.</summary>
        public static readonly string[] All =
        {
            "village",
            "distribution_hub",
            "battery",
            "small_solar",
            "small_wind",
            "water_wheel",
            "small_hydro",
            "power_line",
            "maintenance_depot"
        };

        /// <summary>True when the placeholder prefab should include a child named Spin.</summary>
        public static bool NeedsSpinChild(string buildingId)
        {
            return buildingId == "small_wind"
                   || buildingId == "water_wheel"
                   || buildingId == "small_hydro";
        }
    }
}
