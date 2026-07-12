using System.IO;
using CleanEnergy.Buildings;
using CleanEnergy.CameraSystem;
using CleanEnergy.Core;
using CleanEnergy.DebugTools;
using CleanEnergy.Energy;
using CleanEnergy.Maintenance;
using CleanEnergy.Map;
using CleanEnergy.Placement;
using CleanEnergy.Research;
using CleanEnergy.Save;
using CleanEnergy.Scenario;
using CleanEnergy.Simulation;
using CleanEnergy.Telemetry;
using CleanEnergy.Tutorial;
using CleanEnergy.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CleanEnergy.Editor
{
    /// <summary>
    /// Creates MapGenerationSettings, buildings, scenario, research tree, and Test_Terrain scene.
    /// </summary>
    public static class TestTerrainSceneSetup
    {
        private const string SettingsPath = "Assets/_Game/Data/Map/MapGenerationSettings.asset";
        private const string ScenarioPath = "Assets/_Game/Data/Scenarios/green_valley.asset";
        private const string ResearchPath = "Assets/_Game/Data/Research/green_valley_research.asset";
        private const string ScenePath = "Assets/_Game/Scenes/Test_Terrain.unity";
        private const string BuildingsFolder = "Assets/_Game/Data/Buildings";
        private const string ScenariosFolder = "Assets/_Game/Data/Scenarios";
        private const string ResearchFolder = "Assets/_Game/Data/Research";

        [MenuItem("Clean Energy/Setup Test Terrain Scene")]
        public static void Setup()
        {
            EnsureFolders();
            var settings = CreateOrLoadSettings();
            var scenario = CreateOrLoadScenario();
            var research = CreateOrLoadResearch();
            var buildings = CreateOrLoadBuildings();
            CreateScene(settings, buildings, scenario, research);
            AssetDatabase.SaveAssets();
            Debug.Log("[Setup] Test_Terrain scene and data assets are ready.");
        }

        [MenuItem("Clean Energy/Create Building Definitions")]
        public static void CreateBuildingDefinitionsMenu()
        {
            EnsureFolders();
            CreateOrLoadBuildings();
            AssetDatabase.SaveAssets();
            Debug.Log("[Setup] Building definitions created/updated.");
        }

        [MenuItem("Clean Energy/Create Scenario Definition")]
        public static void CreateScenarioDefinitionMenu()
        {
            EnsureFolders();
            CreateOrLoadScenario();
            AssetDatabase.SaveAssets();
            Debug.Log("[Setup] Scenario definition created/updated.");
        }

        [MenuItem("Clean Energy/Create Research Tree")]
        public static void CreateResearchTreeMenu()
        {
            EnsureFolders();
            CreateOrLoadResearch();
            AssetDatabase.SaveAssets();
            Debug.Log("[Setup] Research tree created/updated.");
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory("Assets/_Game/Data/Map");
            Directory.CreateDirectory(BuildingsFolder);
            Directory.CreateDirectory(ScenariosFolder);
            Directory.CreateDirectory(ResearchFolder);
            Directory.CreateDirectory("Assets/_Game/Scenes");
        }

        private static MapGenerationSettings CreateOrLoadSettings()
        {
            var existing = AssetDatabase.LoadAssetAtPath<MapGenerationSettings>(SettingsPath);
            if (existing != null)
            {
                return existing;
            }

            var settings = ScriptableObject.CreateInstance<MapGenerationSettings>();
            settings.SetSeed("12345");
            AssetDatabase.CreateAsset(settings, SettingsPath);
            return settings;
        }

        private static ScenarioDefinition CreateOrLoadScenario()
        {
            var existing = AssetDatabase.LoadAssetAtPath<ScenarioDefinition>(ScenarioPath);
            if (existing == null)
            {
                existing = ScriptableObject.CreateInstance<ScenarioDefinition>();
                AssetDatabase.CreateAsset(existing, ScenarioPath);
            }

            existing.Configure(
                "green_valley",
                "Green Valley",
                0.95f,
                60,
                2,
                100f,
                2f,
                0.25f,
                30f);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        private static ResearchTreeDefinition CreateOrLoadResearch()
        {
            var existing = AssetDatabase.LoadAssetAtPath<ResearchTreeDefinition>(ResearchPath);
            if (existing == null)
            {
                existing = ScriptableObject.CreateInstance<ResearchTreeDefinition>();
                AssetDatabase.CreateAsset(existing, ResearchPath);
            }

            existing.ConfigureGreenValleyPrototype();
            EditorUtility.SetDirty(existing);
            return existing;
        }

        private static BuildingDefinition[] CreateOrLoadBuildings()
        {
            return new[]
            {
                CreateOrUpdateBuilding(
                    "water_wheel", "Water Wheel", "Starter hydro on a stream bank",
                    BuildingCategory.Energy, 80f, 8f, 25f, 8f, 0f, 0f, true, false,
                    new Color(0.25f, 0.55f, 0.95f)),
                CreateOrUpdateBuilding(
                    "small_hydro", "Small Hydro", "High-flow hydro turbine",
                    BuildingCategory.Energy, 220f, 18f, 35f, 20f, 0f, 0f, true, false,
                    new Color(0.2f, 0.45f, 0.85f), buildingEfficiency: 0.85f),
                CreateOrUpdateBuilding(
                    "small_solar", "Small Solar", "Daytime solar array",
                    BuildingCategory.Energy, 120f, 12f, 20f, 0f, 0.45f, 0f, false, true,
                    new Color(0.95f, 0.8f, 0.2f)),
                CreateOrUpdateBuilding(
                    "small_wind", "Small Wind", "Open-area turbine",
                    BuildingCategory.Energy, 150f, 14f, 28f, 0f, 0f, 0.4f, false, true,
                    new Color(0.65f, 0.85f, 0.95f)),
                CreateOrUpdateBuilding(
                    "village", "Village", "Energy demand settlement",
                    BuildingCategory.Settlement, 200f, 0f, 20f, 0f, 0f, 0f, false, true,
                    new Color(0.75f, 0.55f, 0.35f), demand: 10f),
                CreateOrUpdateBuilding(
                    "battery", "Battery", "Stores surplus energy",
                    BuildingCategory.Storage, 180f, 0f, 25f, 0f, 0f, 0f, false, true,
                    new Color(0.35f, 0.85f, 0.45f), capacity: 50f, charge: 15f, discharge: 15f),
                CreateOrUpdateBuilding(
                    "maintenance_depot", "Maintenance Depot", "Repairs nearby producers",
                    BuildingCategory.Service, 160f, 0f, 30f, 0f, 0f, 0f, false, true,
                    new Color(0.85f, 0.55f, 0.25f), linkRange: 5),
                CreateOrUpdateBuilding(
                    "power_line", "Power Line", "Connects nearby energy nodes",
                    BuildingCategory.Network, 40f, 0f, 35f, 0f, 0f, 0f, false, true,
                    new Color(0.9f, 0.9f, 0.4f), linkRange: 5, hub: true, hubLinkCapacity: 40f),
                CreateOrUpdateBuilding(
                    "distribution_hub", "Distribution Hub", "Long-range network center",
                    BuildingCategory.Network, 120f, 0f, 30f, 0f, 0f, 0f, false, true,
                    new Color(0.55f, 0.75f, 0.95f), linkRange: 10, hub: true, hubLinkCapacity: 120f)
            };
        }

        private static BuildingDefinition CreateOrUpdateBuilding(
            string id,
            string displayName,
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
            bool hub = false,
            float buildingEfficiency = 0.8f,
            float hubLinkCapacity = 0f)
        {
            var path = $"{BuildingsFolder}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<BuildingDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<BuildingDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.Configure(
                id, displayName, description, category,
                cost, power, maxSlope, minWater, minSolar, minWind,
                adjacentWater, requireBuildable, color,
                demand, capacity, charge, discharge, linkRange, hub, buildingEfficiency,
                hubLinkCapacity: hubLinkCapacity);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void CreateScene(
            MapGenerationSettings settings,
            BuildingDefinition[] buildings,
            ScenarioDefinition scenario,
            ResearchTreeDefinition research)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var gameRoot = new GameObject("GameRoot");
            var bootstrap = gameRoot.AddComponent<TestTerrainBootstrap>();
            var so = new SerializedObject(bootstrap);
            so.FindProperty("settings").objectReferenceValue = settings;
            so.FindProperty("scenarioDefinition").objectReferenceValue = scenario;
            so.FindProperty("researchTreeDefinition").objectReferenceValue = research;
            so.FindProperty("startingMoney").floatValue = 1000f;
            var buildingsProp = so.FindProperty("buildingDefinitions");
            buildingsProp.arraySize = buildings.Length;
            for (var i = 0; i < buildings.Length; i++)
            {
                buildingsProp.GetArrayElementAtIndex(i).objectReferenceValue = buildings[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            var mapRoot = new GameObject("MapRoot");
            mapRoot.transform.SetParent(gameRoot.transform, false);
            var terrainRoot = new GameObject("TerrainRoot");
            terrainRoot.transform.SetParent(mapRoot.transform, false);
            var buildingRoot = new GameObject("BuildingRoot");
            buildingRoot.transform.SetParent(gameRoot.transform, false);

            var mapGenerator = mapRoot.AddComponent<MapGenerator>();
            var mapSo = new SerializedObject(mapGenerator);
            mapSo.FindProperty("settings").objectReferenceValue = settings;
            mapSo.FindProperty("terrainRoot").objectReferenceValue = terrainRoot.transform;
            mapSo.FindProperty("generateOnStart").boolValue = true;
            mapSo.ApplyModifiedPropertiesWithoutUndo();

            var placementRoot = new GameObject("PlacementRoot");
            placementRoot.transform.SetParent(gameRoot.transform, false);
            var placement = placementRoot.AddComponent<PlacementController>();

            var simRoot = new GameObject("SimulationRoot");
            simRoot.transform.SetParent(gameRoot.transform, false);
            var clock = simRoot.AddComponent<SimulationClock>();
            clock.BindMapGenerator(mapGenerator);
            var network = simRoot.AddComponent<EnergyNetworkService>();
            var maintenance = simRoot.AddComponent<MaintenanceController>();
            var driver = simRoot.AddComponent<EnergySimulationDriver>();
            var scenarioController = simRoot.AddComponent<ScenarioController>();
            var researchController = simRoot.AddComponent<ResearchController>();
            researchController.Configure(research, driver, network, scenarioController, mapGenerator);
            scenarioController.Configure(scenario, driver, network, clock, mapGenerator, researchController);
            placement.Configure(mapGenerator, buildingRoot.transform, buildings, 1000f, researchController.Service);
            network.Configure(placement, mapGenerator, researchController.Service.GetEfficiencyBonus);
            maintenance.Configure(placement);
            driver.Configure(clock, network, placement, maintenance, mapGenerator);

            var debugRoot = new GameObject("DebugRoot");
            debugRoot.transform.SetParent(gameRoot.transform, false);
            var overlayGo = new GameObject("MapDebugOverlay");
            overlayGo.transform.SetParent(debugRoot.transform, false);
            var overlay = overlayGo.AddComponent<MapDebugOverlay>();
            overlay.SetMapGenerator(mapGenerator);
            overlay.SetPlacementController(placement);

            var highlightGo = new GameObject("SelectionHighlight");
            highlightGo.transform.SetParent(overlayGo.transform, false);
            var highlight = highlightGo.AddComponent<SelectionHighlight>();
            highlight.Configure(overlay, mapGenerator);

            var uiGo = new GameObject("MapDebugUI");
            uiGo.transform.SetParent(debugRoot.transform, false);
            var ui = uiGo.AddComponent<MapDebugUI>();
            ui.Configure(mapGenerator, overlay);

            var placementUiGo = new GameObject("BuildingPlacementUI");
            placementUiGo.transform.SetParent(debugRoot.transform, false);
            var placementUi = placementUiGo.AddComponent<BuildingPlacementUI>();
            placementUi.Configure(placement);

            var hudGo = new GameObject("EnergyHudUI");
            hudGo.transform.SetParent(debugRoot.transform, false);
            var hud = hudGo.AddComponent<EnergyHudUI>();
            hud.Configure(driver, clock, placement, researchController, maintenance);

            var inspectionGo = new GameObject("InspectionPanelUI");
            inspectionGo.transform.SetParent(debugRoot.transform, false);
            var inspection = inspectionGo.AddComponent<InspectionPanelUI>();
            inspection.Configure(overlay, mapGenerator, placement, network);

            var notification = simRoot.AddComponent<NotificationController>();
            notification.Configure(driver, researchController, maintenance, network, scenarioController);
            var notificationHudGo = new GameObject("NotificationHudUI");
            notificationHudGo.transform.SetParent(debugRoot.transform, false);
            var notificationHud = notificationHudGo.AddComponent<NotificationHudUI>();
            notificationHud.Configure(notification);

            var scenarioHudGo = new GameObject("ScenarioHudUI");
            scenarioHudGo.transform.SetParent(debugRoot.transform, false);
            var scenarioHud = scenarioHudGo.AddComponent<ScenarioHudUI>();
            scenarioHud.Configure(scenarioController);

            var researchHudGo = new GameObject("ResearchHudUI");
            researchHudGo.transform.SetParent(debugRoot.transform, false);
            var researchHud = researchHudGo.AddComponent<ResearchHudUI>();
            researchHud.Configure(researchController);

            var telemetry = simRoot.AddComponent<TelemetryController>();
            telemetry.Configure(placement, driver, overlay, scenarioController, mapGenerator);
            var telemetryHudGo = new GameObject("TelemetryHudUI");
            telemetryHudGo.transform.SetParent(debugRoot.transform, false);
            var telemetryHud = telemetryHudGo.AddComponent<TelemetryHudUI>();
            telemetryHud.Configure(telemetry);

            var cameraRoot = new GameObject("CameraRoot");
            cameraRoot.transform.SetParent(gameRoot.transform, false);
            var mainCam = Object.FindAnyObjectByType<Camera>();
            if (mainCam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                mainCam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }

            mainCam.transform.SetParent(cameraRoot.transform, true);
            var controller = mainCam.GetComponent<IsometricCameraController>();
            if (controller == null)
            {
                controller = mainCam.gameObject.AddComponent<IsometricCameraController>();
            }

            controller.ConfigureBounds(settings.TerrainWorldSize);

            var tutorialController = simRoot.AddComponent<TutorialController>();
            tutorialController.Configure(
                controller, overlay, placement, researchController, scenarioController, mapGenerator);
            var tutorialHudGo = new GameObject("TutorialHudUI");
            tutorialHudGo.transform.SetParent(debugRoot.transform, false);
            var tutorialHud = tutorialHudGo.AddComponent<TutorialHudUI>();
            tutorialHud.Configure(tutorialController);

            var saveLoad = simRoot.AddComponent<SaveLoadController>();
            saveLoad.Configure(
                mapGenerator, placement, researchController, scenarioController, clock, network, tutorialController, driver);
            var saveHudGo = new GameObject("SaveLoadHudUI");
            saveHudGo.transform.SetParent(debugRoot.transform, false);
            var saveHud = saveHudGo.AddComponent<SaveLoadHudUI>();
            saveHud.Configure(saveLoad);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
        }
    }
}
