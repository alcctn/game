using CleanEnergy.Buildings;
using CleanEnergy.Placement;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// IMGUI panel for selecting buildings and viewing wallet / failures.
    /// </summary>
    public sealed class BuildingPlacementUI : MonoBehaviour
    {
        [SerializeField] private PlacementController placementController;

        public void Configure(PlacementController controller)
        {
            placementController = controller;
        }

        private void OnGUI()
        {
            if (placementController == null)
            {
                return;
            }

            const float width = 280f;
            GUILayout.BeginArea(new Rect(Screen.width - width - 12f, 12f, width, 360f), GUI.skin.box);
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
    }
}
