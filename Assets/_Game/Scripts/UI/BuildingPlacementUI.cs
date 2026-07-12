using CleanEnergy.Buildings;
using CleanEnergy.Energy;
using CleanEnergy.Placement;
using CleanEnergy.Research;
using CleanEnergy.Simulation;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// IMGUI panel for selecting buildings and viewing wallet / failures / expected yield.
    /// </summary>
    public sealed class BuildingPlacementUI : MonoBehaviour
    {
        [SerializeField] private PlacementController placementController;
        [SerializeField] private SimulationClock simulationClock;
        [SerializeField] private ResearchController researchController;

        public void Configure(
            PlacementController controller,
            SimulationClock clock = null,
            ResearchController research = null)
        {
            placementController = controller;
            simulationClock = clock;
            researchController = research;
        }

        private void OnGUI()
        {
            if (placementController == null)
            {
                return;
            }

            const float width = 280f;
            GUILayout.BeginArea(new Rect(Screen.width - width - 12f, 12f, width, 400f), GUI.skin.box);
            GUILayout.Label("Buildings");
            GUILayout.Label($"Money: {placementController.Wallet.Money:F0}");

            var buildings = placementController.AvailableBuildings;
            if (buildings != null)
            {
                for (var i = 0; i < buildings.Count; i++)
                {
                    var def = buildings[i];
                    if (def == null)
                    {
                        continue;
                    }

                    var unlocked = placementController.BuildingUnlocks == null
                                   || placementController.BuildingUnlocks.IsBuildingUnlocked(def.Id);
                    var selected = placementController.SelectedBuilding == def;
                    var label = unlocked
                        ? $"{def.DisplayName} ({def.Cost:F0})"
                        : $"{def.DisplayName} (Locked)";
                    GUI.enabled = unlocked;
                    if (GUILayout.Toggle(selected, label, "Button") && unlocked && !selected)
                    {
                        placementController.SelectBuilding(def);
                    }

                    GUI.enabled = true;
                }
            }

            if (GUILayout.Button("Cancel (Esc)"))
            {
                placementController.CancelPlacement();
            }

            if (placementController.SelectedBuilding != null)
            {
                GUILayout.Label($"Selected: {placementController.SelectedBuilding.DisplayName}");
                DrawYieldPreview(placementController.SelectedBuilding);
            }

            var failures = placementController.LastFailureReasons;
            if (failures != null && failures.Count > 0)
            {
                GUILayout.Space(8f);
                GUILayout.Label("Issues:");
                for (var i = 0; i < failures.Count; i++)
                {
                    GUILayout.Label($"- {failures[i]}");
                }
            }

            GUILayout.EndArea();
        }

        private void DrawYieldPreview(BuildingDefinition def)
        {
            GUILayout.Label($"Cost: {def.Cost:F0}");
            if (!def.IsProducer)
            {
                return;
            }

            if (!placementController.IsPlacementActive || !placementController.HoverCoordinate.HasValue)
            {
                GUILayout.Label("Expected: —");
                return;
            }

            if (!placementController.IsHoverValid)
            {
                GUILayout.Label("Expected: —");
                return;
            }

            var map = placementController.MapGenerator;
            if (map == null || !map.Grid.IsInitialized)
            {
                GUILayout.Label("Expected: —");
                return;
            }

            var context = simulationClock != null
                ? simulationClock.CreateContextSnapshot()
                : new SimulationContext(0, 0.5f, SimulationSpeed.One, 0.3f, DayPhase.Noon);
            var bonus = researchController != null
                ? researchController.Service.GetEfficiencyBonus(def.Id)
                : 0f;
            var expected = ProductionEstimate.Estimate(
                def,
                placementController.HoverCoordinate.Value,
                map.Grid,
                map.Settings,
                context,
                placementController.Occupancy,
                bonus);
            GUILayout.Label($"Expected: {expected:F1}");
        }
    }
}
