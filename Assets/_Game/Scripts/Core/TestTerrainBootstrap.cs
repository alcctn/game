using CleanEnergy.Buildings;
using CleanEnergy.CameraSystem;
using CleanEnergy.DebugTools;
using CleanEnergy.Energy;
using CleanEnergy.Maintenance;
using CleanEnergy.Map;
using CleanEnergy.Placement;
using CleanEnergy.Research;
using CleanEnergy.Save;
using CleanEnergy.Scenario;
using CleanEnergy.Simulation;
using CleanEnergy.Tutorial;
using CleanEnergy.UI;
using UnityEngine;

namespace CleanEnergy.Core
{
    /// <summary>
    /// Wires Sprint 01–10 test scene objects at runtime if references are missing.
    /// </summary>
    public sealed class TestTerrainBootstrap : MonoBehaviour
    {
        [SerializeField] private MapGenerationSettings settings;
        [SerializeField] private BuildingDefinition[] buildingDefinitions;
        [SerializeField] private ScenarioDefinition scenarioDefinition;
        [SerializeField] private ResearchTreeDefinition researchTreeDefinition;
        [SerializeField] private bool createSettingsIfMissing = true;
        [SerializeField] private float startingMoney = 1000f;

        private void Awake()
        {
            EnsureLighting();
            var settingsAsset = ResolveSettings();
            var mapRoot = EnsureChild("MapRoot");
            var terrainRoot = EnsureChild("TerrainRoot", mapRoot);
            var buildingRoot = EnsureChild("BuildingRoot");
            var debugRoot = EnsureChild("DebugRoot");
            var cameraRoot = EnsureChild("CameraRoot");
            var simRoot = EnsureChild("SimulationRoot");

            var mapGenerator = FindOrAdd<MapGenerator>(mapRoot);
            mapGenerator.SetSettings(settingsAsset);
            mapGenerator.SetTerrainRoot(terrainRoot);

            var overlayGo = EnsureChild("MapDebugOverlay", debugRoot);
            var overlay = FindOrAdd<MapDebugOverlay>(overlayGo.gameObject);
            overlay.SetMapGenerator(mapGenerator);

            var uiGo = EnsureChild("MapDebugUI", debugRoot);
            var ui = FindOrAdd<MapDebugUI>(uiGo.gameObject);
            ui.Configure(mapGenerator, overlay);

            var placementGo = EnsureChild("PlacementRoot");
            var placement = FindOrAdd<PlacementController>(placementGo.gameObject);
            var buildings = ResolveBuildings();
            placement.Configure(mapGenerator, buildingRoot, buildings, startingMoney);
            overlay.SetPlacementController(placement);

            var placementUiGo = EnsureChild("BuildingPlacementUI", debugRoot);
            var placementUi = FindOrAdd<BuildingPlacementUI>(placementUiGo.gameObject);
            placementUi.Configure(placement);

            var clock = FindOrAdd<SimulationClock>(simRoot.gameObject);
            clock.BindMapGenerator(mapGenerator);
            var network = FindOrAdd<EnergyNetworkService>(simRoot.gameObject);
            var maintenance = FindOrAdd<MaintenanceController>(simRoot.gameObject);
            maintenance.Configure(placement);
            var driver = FindOrAdd<EnergySimulationDriver>(simRoot.gameObject);
            driver.Configure(clock, network, placement, maintenance);

            var scenario = FindOrAdd<ScenarioController>(simRoot.gameObject);
            scenario.Configure(ResolveScenario(), driver, network, clock, mapGenerator);

            var research = FindOrAdd<ResearchController>(simRoot.gameObject);
            research.Configure(ResolveResearchTree(), driver, network, scenario, mapGenerator);
            placement.SetBuildingUnlockQuery(research.Service);
            network.Configure(placement, mapGenerator, research.Service.GetEfficiencyBonus);

            var hudGo = EnsureChild("EnergyHudUI", debugRoot);
            var hud = FindOrAdd<EnergyHudUI>(hudGo.gameObject);
            hud.Configure(driver, clock, placement, research, maintenance);

            var scenarioHudGo = EnsureChild("ScenarioHudUI", debugRoot);
            var scenarioHud = FindOrAdd<ScenarioHudUI>(scenarioHudGo.gameObject);
            scenarioHud.Configure(scenario);

            var researchHudGo = EnsureChild("ResearchHudUI", debugRoot);
            var researchHud = FindOrAdd<ResearchHudUI>(researchHudGo.gameObject);
            researchHud.Configure(research);

            var camTransform = cameraRoot.Find("Main Camera");
            Camera cam;
            if (camTransform == null)
            {
                var existing = Camera.main;
                if (existing != null)
                {
                    existing.transform.SetParent(cameraRoot, true);
                    cam = existing;
                }
                else
                {
                    var camGo = new GameObject("Main Camera");
                    camGo.tag = "MainCamera";
                    camGo.transform.SetParent(cameraRoot, false);
                    cam = camGo.AddComponent<Camera>();
                }
            }
            else
            {
                cam = camTransform.GetComponent<Camera>();
            }

            var controller = cam.GetComponent<IsometricCameraController>();
            if (controller == null)
            {
                controller = cam.gameObject.AddComponent<IsometricCameraController>();
            }

            controller.ConfigureBounds(settingsAsset.TerrainWorldSize);
            if (cam.GetComponent<AudioListener>() == null)
            {
                cam.gameObject.AddComponent<AudioListener>();
            }

            var tutorial = FindOrAdd<TutorialController>(simRoot.gameObject);
            tutorial.Configure(controller, overlay, placement, research, scenario, mapGenerator);
            var tutorialHudGo = EnsureChild("TutorialHudUI", debugRoot);
            var tutorialHud = FindOrAdd<TutorialHudUI>(tutorialHudGo.gameObject);
            tutorialHud.Configure(tutorial);

            var saveLoad = FindOrAdd<SaveLoadController>(simRoot.gameObject);
            saveLoad.Configure(mapGenerator, placement, research, scenario, clock, network, tutorial);
            var saveHudGo = EnsureChild("SaveLoadHudUI", debugRoot);
            var saveHud = FindOrAdd<SaveLoadHudUI>(saveHudGo.gameObject);
            saveHud.Configure(saveLoad);
        }

        private BuildingDefinition[] ResolveBuildings()
        {
            if (buildingDefinitions == null || buildingDefinitions.Length == 0)
            {
                return CreateDefaultBuildings();
            }

            var defaults = CreateDefaultBuildings();
            var merged = new System.Collections.Generic.List<BuildingDefinition>();
            var seen = new System.Collections.Generic.HashSet<string>();

            for (var i = 0; i < buildingDefinitions.Length; i++)
            {
                var def = buildingDefinitions[i];
                if (def == null || string.IsNullOrEmpty(def.Id) || !seen.Add(def.Id))
                {
                    continue;
                }

                merged.Add(def);
            }

            for (var i = 0; i < defaults.Length; i++)
            {
                if (seen.Add(defaults[i].Id))
                {
                    merged.Add(defaults[i]);
                }
            }

            return merged.ToArray();
        }

        private ScenarioDefinition ResolveScenario()
        {
            if (scenarioDefinition != null)
            {
                return scenarioDefinition;
            }

            scenarioDefinition = ScenarioProgressService.CreateRuntimeDefault();
            return scenarioDefinition;
        }

        private ResearchTreeDefinition ResolveResearchTree()
        {
            if (researchTreeDefinition != null)
            {
                return researchTreeDefinition;
            }

            researchTreeDefinition = ResearchService.CreateRuntimeDefaultTree();
            return researchTreeDefinition;
        }

        public static BuildingDefinition[] CreateDefaultBuildings()
        {
            return new[]
            {
                CreateRuntimeBuilding(
                    "water_wheel", "Water Wheel", "Starter hydro",
                    BuildingCategory.Energy, 80f, 8f, 25f, 8f, 0f, 0f, true, false,
                    new Color(0.25f, 0.55f, 0.95f)),
                CreateRuntimeBuilding(
                    "small_solar", "Small Solar", "Daytime solar array",
                    BuildingCategory.Energy, 120f, 12f, 20f, 0f, 0.45f, 0f, false, true,
                    new Color(0.95f, 0.8f, 0.2f)),
                CreateRuntimeBuilding(
                    "small_wind", "Small Wind", "Open-area turbine",
                    BuildingCategory.Energy, 150f, 14f, 28f, 0f, 0f, 0.4f, false, true,
                    new Color(0.65f, 0.85f, 0.95f)),
                CreateRuntimeBuilding(
                    "village", "Village", "Energy demand settlement",
                    BuildingCategory.Settlement, 200f, 0f, 20f, 0f, 0f, 0f, false, true,
                    new Color(0.75f, 0.55f, 0.35f), demand: 10f),
                CreateRuntimeBuilding(
                    "battery", "Battery", "Stores surplus energy",
                    BuildingCategory.Storage, 180f, 0f, 25f, 0f, 0f, 0f, false, true,
                    new Color(0.35f, 0.85f, 0.45f), capacity: 50f, charge: 15f, discharge: 15f),
                CreateRuntimeBuilding(
                    "maintenance_depot", "Maintenance Depot", "Repairs nearby producers",
                    BuildingCategory.Service, 160f, 0f, 30f, 0f, 0f, 0f, false, true,
                    new Color(0.85f, 0.55f, 0.25f), linkRange: 5),
                CreateRuntimeBuilding(
                    "power_line", "Power Line", "Connects nearby energy nodes",
                    BuildingCategory.Network, 40f, 0f, 35f, 0f, 0f, 0f, false, true,
                    new Color(0.9f, 0.9f, 0.4f), linkRange: 5, hub: true),
                CreateRuntimeBuilding(
                    "distribution_hub", "Distribution Hub", "Long-range network center",
                    BuildingCategory.Network, 120f, 0f, 30f, 0f, 0f, 0f, false, true,
                    new Color(0.55f, 0.75f, 0.95f), linkRange: 10, hub: true)
            };
        }

        private static BuildingDefinition CreateRuntimeBuilding(
            string id,
            string name,
            string description,
            BuildingCategory category,
            float cost,
            float power,
            float maxSlope,
            float minWater,
            float minSolar,
            float minWind,
            bool adjacentWater,
            bool requireBuildable,
            Color color,
            float demand = 0f,
            float capacity = 0f,
            float charge = 20f,
            float discharge = 20f,
            int linkRange = 4,
            bool hub = false)
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.name = id;
            def.Configure(
                id, name, description, category,
                cost, power, maxSlope, minWater, minSolar, minWind,
                adjacentWater, requireBuildable, color,
                demand, capacity, charge, discharge, linkRange, hub);
            return def;
        }

        private MapGenerationSettings ResolveSettings()
        {
            if (settings != null)
            {
                return settings;
            }

            if (!createSettingsIfMissing)
            {
                Debug.LogError("[Bootstrap] MapGenerationSettings is missing.");
                return null;
            }

            settings = ScriptableObject.CreateInstance<MapGenerationSettings>();
            settings.name = "RuntimeMapGenerationSettings";
            settings.SetSeed("12345");
            return settings;
        }

        private static void EnsureLighting()
        {
            if (Object.FindAnyObjectByType<Light>() != null)
            {
                return;
            }

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private Transform EnsureChild(string name, Transform parent = null)
        {
            var root = parent != null ? parent : transform;
            var existing = root.Find(name);
            if (existing != null)
            {
                return existing;
            }

            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            return go.transform;
        }

        private static T FindOrAdd<T>(GameObject host) where T : Component
        {
            var component = host.GetComponent<T>();
            return component != null ? component : host.AddComponent<T>();
        }

        private static T FindOrAdd<T>(Transform host) where T : Component => FindOrAdd<T>(host.gameObject);
    }
}
