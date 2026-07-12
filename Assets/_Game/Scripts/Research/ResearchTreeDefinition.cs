using System;
using UnityEngine;

namespace CleanEnergy.Research
{
    /// <summary>
    /// Prototype research tree: hydro / solar / wind branches.
    /// </summary>
    [CreateAssetMenu(fileName = "ResearchTreeDefinition", menuName = "Clean Energy/Research Tree")]
    public sealed class ResearchTreeDefinition : ScriptableObject
    {
        [SerializeField] private string treeId = "green_valley_research";
        [SerializeField] private string displayName = "Green Valley Research";
        [SerializeField] private string[] alwaysUnlockedBuildingIds =
        {
            "water_wheel",
            "village",
            "battery",
            "power_line",
            "maintenance_depot",
            "distribution_hub"
        };
        [SerializeField] private ResearchNodeDefinition[] nodes = Array.Empty<ResearchNodeDefinition>();

        public string TreeId => treeId;
        public string DisplayName => displayName;
        public string[] AlwaysUnlockedBuildingIds => alwaysUnlockedBuildingIds ?? Array.Empty<string>();
        public ResearchNodeDefinition[] Nodes => nodes ?? Array.Empty<ResearchNodeDefinition>();

        public void ConfigureGreenValleyPrototype()
        {
            treeId = "green_valley_research";
            displayName = "Green Valley Research";
            alwaysUnlockedBuildingIds = new[]
            {
                "water_wheel",
                "village",
                "battery",
                "power_line",
                "maintenance_depot",
                "distribution_hub"
            };

            var hydroBasic = new ResearchNodeDefinition();
            hydroBasic.Configure(
                "hydro_basic", "Basic Hydro", "Unlocks water wheel",
                "", 0f, true, new[] { "water_wheel" }, "", 0f);

            var hydroEff = new ResearchNodeDefinition();
            hydroEff.Configure(
                "hydro_eff", "Hydro Efficiency", "+0.1 water wheel efficiency",
                "hydro_basic", 20f, false, Array.Empty<string>(), "water_wheel", 0.1f);

            var solarBasic = new ResearchNodeDefinition();
            solarBasic.Configure(
                "solar_basic", "Basic Solar", "Unlocks small solar",
                "", 15f, false, new[] { "small_solar" }, "", 0f);

            var solarEff = new ResearchNodeDefinition();
            solarEff.Configure(
                "solar_eff", "Solar Efficiency", "+0.1 solar efficiency",
                "solar_basic", 25f, false, Array.Empty<string>(), "small_solar", 0.1f);

            var windBasic = new ResearchNodeDefinition();
            windBasic.Configure(
                "wind_basic", "Basic Wind", "Unlocks small wind",
                "", 20f, false, new[] { "small_wind" }, "", 0f);

            var windEff = new ResearchNodeDefinition();
            windEff.Configure(
                "wind_eff", "Wind Efficiency", "+0.1 wind efficiency",
                "wind_basic", 25f, false, Array.Empty<string>(), "small_wind", 0.1f);

            nodes = new[]
            {
                hydroBasic, hydroEff,
                solarBasic, solarEff,
                windBasic, windEff
            };
        }
    }
}
