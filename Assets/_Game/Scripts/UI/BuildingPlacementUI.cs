using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Energy;
using CleanEnergy.Placement;
using CleanEnergy.Research;
using CleanEnergy.Simulation;
using CleanEnergy.Tutorial;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// IMGUI panel for selecting buildings and viewing wallet / failures / expected yield.
    /// </summary>
    public sealed class BuildingPlacementUI : MonoBehaviour
    {
        public const string BuildTabPrefsKey = "ce_build_tab";

        private static readonly BuildingCategory[] TabOrder =
        {
            BuildingCategory.Energy,
            BuildingCategory.Network,
            BuildingCategory.Storage,
            BuildingCategory.Settlement,
            BuildingCategory.Service
        };

        private static readonly string[] TabLabels =
        {
            "Energy",
            "Network",
            "Storage",
            "Settlement",
            "Service"
        };

        [SerializeField] private PlacementController placementController;
        [SerializeField] private SimulationClock simulationClock;
        [SerializeField] private ResearchController researchController;
        [SerializeField] private TutorialController tutorialController;

        private BuildingCategory _activeCategory = BuildingCategory.Energy;
        private bool _tabLoaded;

        /// <summary>Currently selected build-menu category tab.</summary>
        public BuildingCategory ActiveCategory
        {
            get
            {
                EnsureTabLoaded();
                return _activeCategory;
            }
        }

        public void Configure(
            PlacementController controller,
            SimulationClock clock = null,
            ResearchController research = null,
            TutorialController tutorial = null)
        {
            placementController = controller;
            simulationClock = clock;
            researchController = research;
            if (tutorial != null)
            {
                tutorialController = tutorial;
            }
        }

        /// <summary>Sets the active tab and persists it to PlayerPrefs.</summary>
        public void SetActiveCategory(BuildingCategory category)
        {
            _activeCategory = category;
            _tabLoaded = true;
            SaveActiveCategory(category);
        }

        /// <summary>Loads the last build tab from PlayerPrefs (default Energy).</summary>
        public static BuildingCategory LoadActiveCategory()
        {
            var raw = PlayerPrefs.GetInt(BuildTabPrefsKey, (int)BuildingCategory.Energy);
            if (!IsKnownCategory(raw))
            {
                return BuildingCategory.Energy;
            }

            return (BuildingCategory)raw;
        }

        /// <summary>Persists the active build tab.</summary>
        public static void SaveActiveCategory(BuildingCategory category)
        {
            PlayerPrefs.SetInt(BuildTabPrefsKey, (int)category);
            PlayerPrefs.Save();
        }

        /// <summary>True when the definition belongs to the active category tab.</summary>
        public static bool MatchesCategory(BuildingDefinition definition, BuildingCategory category)
        {
            return definition != null && definition.Category == category;
        }

        /// <summary>
        /// Returns all buildings in <paramref name="category"/> (locked and unlocked).
        /// Null unlock query does not filter; callers decide selection eligibility separately.
        /// </summary>
        public static List<BuildingDefinition> FilterForTab(
            IReadOnlyList<BuildingDefinition> buildings,
            BuildingCategory category,
            IBuildingUnlockQuery unlocks = null)
        {
            // unlocks retained for call-site compatibility; S85 lists locked rows too.
            _ = unlocks;

            var result = new List<BuildingDefinition>();
            if (buildings == null)
            {
                return result;
            }

            for (var i = 0; i < buildings.Count; i++)
            {
                var def = buildings[i];
                if (!MatchesCategory(def, category))
                {
                    continue;
                }

                result.Add(def);
            }

            return result;
        }

        /// <summary>True when the building may be selected for placement.</summary>
        public static bool CanSelectBuilding(BuildingDefinition definition, IBuildingUnlockQuery unlocks)
        {
            if (definition == null)
            {
                return false;
            }

            return unlocks == null || unlocks.IsBuildingUnlocked(definition.Id);
        }

        /// <summary>Formats the locked-row reason shown in the build menu.</summary>
        public static string FormatLockedReason(string requirementLabel)
        {
            var label = string.IsNullOrEmpty(requirementLabel) ? "?" : requirementLabel;
            return $"Requires: {label}";
        }

        /// <summary>
        /// Resolves the research node display name (or id) that unlocks <paramref name="buildingId"/>.
        /// Falls back to the building id when no unlock node is found.
        /// </summary>
        public static string ResolveUnlockRequirementLabel(
            string buildingId,
            ResearchTreeDefinition tree)
        {
            if (string.IsNullOrEmpty(buildingId))
            {
                return string.Empty;
            }

            if (tree != null)
            {
                var nodes = tree.Nodes;
                for (var i = 0; i < nodes.Length; i++)
                {
                    var node = nodes[i];
                    if (node == null)
                    {
                        continue;
                    }

                    var unlocks = node.UnlockBuildingIds;
                    for (var u = 0; u < unlocks.Length; u++)
                    {
                        if (unlocks[u] != buildingId)
                        {
                            continue;
                        }

                        return !string.IsNullOrEmpty(node.DisplayName) ? node.DisplayName : node.Id;
                    }
                }
            }

            return buildingId;
        }

        /// <summary>Deletes the build-tab prefs key (tests).</summary>
        public static void ClearTabPrefs()
        {
            PlayerPrefs.DeleteKey(BuildTabPrefsKey);
            PlayerPrefs.Save();
        }

        private void OnGUI()
        {
            if (placementController == null)
            {
                return;
            }

            GuiScale.Apply();
            EnsureTabLoaded();

            const float width = 280f;
            GUILayout.BeginArea(new Rect(Screen.width / GuiScale.Current - width - 12f, 12f, width, 460f), GUI.skin.box);
            GUILayout.Label(StringTable.Get(StringKeys.Build));
            GUILayout.Label($"Money: {placementController.Wallet.Money:F0}");

            DrawCategoryTabs();

            var buildings = placementController.AvailableBuildings;
            if (buildings != null)
            {
                var tree = researchController != null && researchController.Service != null
                    ? researchController.Service.Tree
                    : null;

                for (var i = 0; i < buildings.Count; i++)
                {
                    var def = buildings[i];
                    if (!MatchesCategory(def, _activeCategory))
                    {
                        continue;
                    }

                    var unlocked = CanSelectBuilding(def, placementController.BuildingUnlocks);
                    if (!unlocked)
                    {
                        var requirement = ResolveUnlockRequirementLabel(def.Id, tree);
                        var lockedLabel = $"{def.DisplayName} — {FormatLockedReason(requirement)}";
                        var previous = GUI.enabled;
                        GUI.enabled = false;
                        GUILayout.Toggle(false, lockedLabel, "Button");
                        GUI.enabled = previous;
                        continue;
                    }

                    var selected = placementController.SelectedBuilding == def;
                    var label = $"{def.DisplayName} ({def.Cost:F0})";
                    var highlightId = ResolveTutorialBuildTargetId();
                    var highlighted = !string.IsNullOrEmpty(highlightId) && def.Id == highlightId;
                    label = TutorialProgressService.FormatSoftHighlightLabel(label, highlighted);
                    if (GUILayout.Toggle(selected, label, "Button") && !selected)
                    {
                        placementController.SelectBuilding(def);
                    }
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

        private void DrawCategoryTabs()
        {
            var selectedIndex = IndexOfCategory(_activeCategory);
            var next = GUILayout.SelectionGrid(selectedIndex, TabLabels, 3);
            if (next != selectedIndex && next >= 0 && next < TabOrder.Length)
            {
                SetActiveCategory(TabOrder[next]);
                if (placementController.SelectedBuilding != null
                    && placementController.SelectedBuilding.Category != _activeCategory)
                {
                    placementController.CancelPlacement();
                }
            }
        }

        private void EnsureTabLoaded()
        {
            if (_tabLoaded)
            {
                return;
            }

            _activeCategory = LoadActiveCategory();
            _tabLoaded = true;
        }

        private static int IndexOfCategory(BuildingCategory category)
        {
            for (var i = 0; i < TabOrder.Length; i++)
            {
                if (TabOrder[i] == category)
                {
                    return i;
                }
            }

            return 0;
        }

        private static bool IsKnownCategory(int raw)
        {
            return raw >= (int)BuildingCategory.Energy && raw <= (int)BuildingCategory.Settlement;
        }

        private void DrawYieldPreview(BuildingDefinition def)
        {
            GUILayout.Label($"Cost: {placementController.GetHoverEffectiveCost():F0}");
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
            var networkFactor = NetworkLoadFactor.ResolveForPlacement(
                placementController.HoverCoordinate.Value,
                def,
                placementController.Occupancy);
            var deliveryFactor = TransmissionLoss.ResolveDeliveryFactorForPlacement(
                placementController.HoverCoordinate.Value,
                placementController.Occupancy,
                def);
            var expected = ProductionEstimate.BreakDown(
                def,
                placementController.HoverCoordinate.Value,
                map.Grid,
                map.Settings,
                context,
                placementController.Occupancy,
                bonus,
                networkFactor: networkFactor,
                deliveryFactor: deliveryFactor).Production;
            GUILayout.Label($"Expected: {expected:F1}");
        }

        private string ResolveTutorialBuildTargetId()
        {
            if (tutorialController == null || !tutorialController.IsEnabled || tutorialController.Progress == null)
            {
                return null;
            }

            if (tutorialController.Progress.IsComplete)
            {
                return null;
            }

            return TutorialProgressService.ResolveBuildTargetId(tutorialController.Progress.CurrentStep);
        }
    }
}
