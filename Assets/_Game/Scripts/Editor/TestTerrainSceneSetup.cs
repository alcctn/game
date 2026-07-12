using CleanEnergy.Art;
using System.IO;
using CleanEnergy.Audio;
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
        private const string SunRidgeScenarioPath = "Assets/_Game/Data/Scenarios/sun_ridge.asset";
        private const string WindCoastScenarioPath = "Assets/_Game/Data/Scenarios/wind_coast.asset";
        private const string ResearchPath = "Assets/_Game/Data/Research/green_valley_research.asset";
        private const string ScenePath = "Assets/_Game/Scenes/Test_Terrain.unity";
        private const string MainMenuScenePath = "Assets/_Game/Scenes/MainMenu.unity";
        private const string BootstrapScenePath = "Assets/_Game/Scenes/Bootstrap.unity";
        private const string BuildingsFolder = "Assets/_Game/Data/Buildings";
        private const string BuildingPrefabsFolder = "Assets/_Game/Prefabs/Buildings";
        private const string ScenariosFolder = "Assets/_Game/Data/Scenarios";
        private const string ResearchFolder = "Assets/_Game/Data/Research";
        private const string PineBasinScenarioPath = "Assets/_Game/Data/Scenarios/pine_basin.asset";
        private const string AridPlateauScenarioPath = "Assets/_Game/Data/Scenarios/arid_plateau.asset";

        private static readonly string[] StaticPrefabBuildingIds = BuildingPrefabIds.All;

        [MenuItem("Clean Energy/Setup Test Terrain Scene")]
        public static void Setup()
        {
            EnsureFolders();
            var settings = CreateOrLoadSettings();
            var scenario = CreateOrLoadScenario();
            var sunRidge = CreateOrLoadSunRidgeScenario();
            var windCoast = CreateOrLoadWindCoastScenario();
            var pineBasin = CreateOrLoadPineBasinScenario();
            var aridPlateau = CreateOrLoadAridPlateauScenario();
            var research = CreateOrLoadResearch();
            var purePoly = CreateOrLoadPurePolyCatalog();
            var buildings = CreateOrLoadBuildings();
            CreateScene(settings, buildings, scenario, sunRidge, windCoast, pineBasin, aridPlateau, research, purePoly);
            EnsureMainMenuScene();
            EnsureBootstrapScene();
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("[Setup] Test_Terrain scene and data assets are ready.");
        }

        [MenuItem("Clean Energy/Setup Bootstrap Scene")]
        public static void SetupBootstrapMenu()
        {
            EnsureFolders();
            EnsureBootstrapScene();
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("[Setup] Bootstrap scene ready.");
        }

        [MenuItem("Clean Energy/Create Building Placeholder Prefabs")]
        public static void CreateBuildingPlaceholderPrefabsMenu()
        {
            EnsureFolders();
            CreateOrLoadBuildings();
            AssetDatabase.SaveAssets();
            Debug.Log("[Setup] Building placeholder prefabs created/assigned.");
        }

        [MenuItem("Clean Energy/Setup Main Menu Scene")]
        public static void SetupMainMenuMenu()
        {
            EnsureFolders();
            EnsureMainMenuScene();
            EnsureBootstrapScene();
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("[Setup] MainMenu scene ready.");
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
            CreateOrLoadSunRidgeScenario();
            CreateOrLoadWindCoastScenario();
            CreateOrLoadPineBasinScenario();
            CreateOrLoadAridPlateauScenario();
            AssetDatabase.SaveAssets();
            Debug.Log("[Setup] Scenario definitions created/updated.");
        }

        [MenuItem("Clean Energy/Create Research Tree")]
        public static void CreateResearchTreeMenu()
        {
            EnsureFolders();
            CreateOrLoadResearch();
            AssetDatabase.SaveAssets();
            Debug.Log("[Setup] Research tree created/updated.");
        }

        [MenuItem("Clean Energy/Setup Pure Poly Catalog")]
        public static void SetupPurePolyCatalogMenu()
        {
            EnsureFolders();
            CreateOrLoadPurePolyCatalog();
            AssetDatabase.SaveAssets();
            Debug.Log("[Setup] PurePolyCatalog wired from Pure Poly prefabs.");
        }

        /// <summary>Ensures PurePolyCatalog exists and prefab slots are assigned (null-safe).</summary>
        public static PurePolyCatalog EnsurePurePolyCatalog()
        {
            EnsureFolders();
            return CreateOrLoadPurePolyCatalog();
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory("Assets/_Game/Data/Map");
            Directory.CreateDirectory(BuildingsFolder);
            Directory.CreateDirectory(BuildingPrefabsFolder);
            Directory.CreateDirectory(ScenariosFolder);
            Directory.CreateDirectory(ResearchFolder);
            Directory.CreateDirectory("Assets/_Game/Data/Art");
            Directory.CreateDirectory("Assets/_Game/Resources");
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

        private static ScenarioDefinition CreateOrLoadSunRidgeScenario()
        {
            var existing = AssetDatabase.LoadAssetAtPath<ScenarioDefinition>(SunRidgeScenarioPath);
            if (existing == null)
            {
                existing = ScriptableObject.CreateInstance<ScenarioDefinition>();
                AssetDatabase.CreateAsset(existing, SunRidgeScenarioPath);
            }

            existing.Configure(
                "sun_ridge",
                "Sun Ridge",
                0.9f,
                45,
                2,
                100f,
                2.5f,
                0.3f,
                25f,
                researchNodeIds: new[] { "solar_basic" },
                seed: "sun_ridge_42",
                solarOverride: 0.95f,
                streamOverride: 18f);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        private static ScenarioDefinition CreateOrLoadWindCoastScenario()
        {
            var existing = AssetDatabase.LoadAssetAtPath<ScenarioDefinition>(WindCoastScenarioPath);
            if (existing == null)
            {
                existing = ScriptableObject.CreateInstance<ScenarioDefinition>();
                AssetDatabase.CreateAsset(existing, WindCoastScenarioPath);
            }

            existing.Configure(
                "wind_coast",
                "Wind Coast",
                0.9f,
                50,
                2,
                100f,
                2.2f,
                0.28f,
                28f,
                researchNodeIds: new[] { "wind_basic" },
                seed: "wind_coast_77",
                solarOverride: 0.35f,
                streamOverride: 14f,
                population: 100f,
                windOverride: 0.85f);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        private static ScenarioDefinition CreateOrLoadPineBasinScenario()
        {
            var existing = AssetDatabase.LoadAssetAtPath<ScenarioDefinition>(PineBasinScenarioPath);
            if (existing == null)
            {
                existing = ScriptableObject.CreateInstance<ScenarioDefinition>();
                AssetDatabase.CreateAsset(existing, PineBasinScenarioPath);
            }

            existing.Configure(
                "pine_basin",
                "Pine Basin",
                0.9f,
                55,
                2,
                100f,
                2.0f,
                0.3f,
                28f,
                researchNodeIds: new[] { "hydro_turbine" },
                seed: "pine_basin_55",
                solarOverride: 0.55f,
                streamOverride: 6f,
                population: 100f,
                windOverride: 0.2f);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        private static ScenarioDefinition CreateOrLoadAridPlateauScenario()
        {
            var existing = AssetDatabase.LoadAssetAtPath<ScenarioDefinition>(AridPlateauScenarioPath);
            if (existing == null)
            {
                existing = ScriptableObject.CreateInstance<ScenarioDefinition>();
                AssetDatabase.CreateAsset(existing, AridPlateauScenarioPath);
            }

            existing.Configure(
                "arid_plateau",
                "Arid Plateau",
                0.9f,
                55,
                2,
                100f,
                2.0f,
                0.3f,
                28f,
                researchNodeIds: new[] { "solar_panel" },
                seed: "arid_plateau_91",
                solarOverride: 0.98f,
                streamOverride: 22f,
                population: 100f,
                windOverride: 0.15f);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        /// <summary>
        /// Loads or creates the research asset and always re-applies the runtime prototype tree
        /// so disk stays in sync with ConfigureGreenValleyPrototype (storage + tier-3).
        /// </summary>
        private static ResearchTreeDefinition CreateOrLoadResearch()
        {
            var existing = AssetDatabase.LoadAssetAtPath<ResearchTreeDefinition>(ResearchPath);
            if (existing == null)
            {
                existing = ScriptableObject.CreateInstance<ResearchTreeDefinition>();
                AssetDatabase.CreateAsset(existing, ResearchPath);
            }

            // Always sync — never skip when the asset already exists.
            existing.ConfigureGreenValleyPrototype();
            EditorUtility.SetDirty(existing);
            return existing;
        }

        private static PurePolyCatalog CreateOrLoadPurePolyCatalog()
        {
            const string dataPath = PurePolyCatalog.DefaultAssetPath;
            const string resourcesPath = "Assets/_Game/Resources/PurePolyCatalog.asset";

            var catalog = AssetDatabase.LoadAssetAtPath<PurePolyCatalog>(dataPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<PurePolyCatalog>();
                AssetDatabase.CreateAsset(catalog, dataPath);
            }

            catalog.Configure(
                LoadPp("PP_Meadow_07"),
                new[]
                {
                    LoadPp("PP_Tree_02"),
                    LoadPp("PP_Tree_10"),
                    LoadPp("PP_Birch_Tree_05"),
                    LoadPp("PP_Birch_Tree_06")
                },
                LoadPp("PP_Lake_Ground_04"),
                LoadPp("PP_Forest_Mountain_Moss_01") ?? LoadPp("PP_Forest_Mountain_Moss_02"),
                LoadPp("PP_Meadow_Path_05"),
                new[]
                {
                    LoadPp("PP_Rock_Moss_Grown_09"),
                    LoadPp("PP_Rock_Moss_Grown_11"),
                    LoadPp("PP_Rock_Pile_Forest_Moss_05"),
                    LoadPp("PP_Rock_Pile_Forest_Moss_10")
                },
                new[]
                {
                    LoadPp("PP_Daffodil_03"),
                    LoadPp("PP_Hyacinth_04"),
                    LoadPp("PP_Sunflower_04")
                },
                new[]
                {
                    LoadPp("PP_Grass_11"),
                    LoadPp("PP_Grass_15"),
                    LoadPp("PP_Meadow_08")
                },
                new[]
                {
                    LoadPp("PP_Bridge_15_Middle"),
                    LoadPp("PP_Bridge_15_Left"),
                    LoadPp("PP_Bridge_15_Right")
                });
            EditorUtility.SetDirty(catalog);

            // Mirror into Resources for runtime Resolve without scene setup.
            var resourcesCopy = AssetDatabase.LoadAssetAtPath<PurePolyCatalog>(resourcesPath);
            if (resourcesCopy == null)
            {
                AssetDatabase.CopyAsset(dataPath, resourcesPath);
                resourcesCopy = AssetDatabase.LoadAssetAtPath<PurePolyCatalog>(resourcesPath);
            }

            if (resourcesCopy != null && resourcesCopy != catalog)
            {
                resourcesCopy.Configure(
                    catalog.Meadow,
                    catalog.ForestTrees,
                    catalog.LakeGround,
                    catalog.Mountain,
                    catalog.Path,
                    catalog.Rocks,
                    catalog.Flowers,
                    catalog.Grasses,
                    catalog.Bridges);
                EditorUtility.SetDirty(resourcesCopy);
            }

            return catalog;
        }

        private static GameObject LoadPp(string prefabName)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(
                $"{PurePolyCatalog.PrefabFolder}/{prefabName}.prefab");
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
                    new Color(0.95f, 0.8f, 0.2f), footprint: new Vector2Int(2, 1)),
                CreateOrUpdateBuilding(
                    "small_wind", "Small Wind", "Open-area turbine",
                    BuildingCategory.Energy, 150f, 14f, 28f, 0f, 0f, 0.4f, false, true,
                    new Color(0.65f, 0.85f, 0.95f), sameTypeSpacing: 3),
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
            float hubLinkCapacity = 0f,
            int sameTypeSpacing = 0,
            Vector2Int? footprint = null)
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
                hubLinkCapacity: hubLinkCapacity,
                sameTypeSpacing: sameTypeSpacing,
                footprint: footprint);

            if (IsStaticPrefabBuilding(id))
            {
                asset.SetPrefab(CreateOrLoadPlaceholderPrefab(id));
            }

            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static bool IsStaticPrefabBuilding(string id)
        {
            for (var i = 0; i < StaticPrefabBuildingIds.Length; i++)
            {
                if (StaticPrefabBuildingIds[i] == id)
                {
                    return true;
                }
            }

            return false;
        }

        private static GameObject CreateOrLoadPlaceholderPrefab(string id)
        {
            var path = $"{BuildingPrefabsFolder}/{id}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                EnsureSpinChildOnPrefab(existing, id, path);
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = id;
            go.transform.localScale = Vector3.one * 0.8f;
            if (BuildingPrefabIds.NeedsSpinChild(id))
            {
                var spin = new GameObject("Spin");
                spin.transform.SetParent(go.transform, false);
            }

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void EnsureSpinChildOnPrefab(GameObject prefabRoot, string id, string path)
        {
            if (!BuildingPrefabIds.NeedsSpinChild(id) || prefabRoot == null)
            {
                return;
            }

            if (FindChildNamed(prefabRoot.transform, "Spin") != null)
            {
                return;
            }

            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (FindChildNamed(contents.transform, "Spin") == null)
                {
                    var spin = new GameObject("Spin");
                    spin.transform.SetParent(contents.transform, false);
                    PrefabUtility.SaveAsPrefabAsset(contents, path);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static Transform FindChildNamed(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child.name == childName)
                {
                    return child;
                }

                var nested = FindChildNamed(child, childName);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }

        private static void CreateScene(
            MapGenerationSettings settings,
            BuildingDefinition[] buildings,
            ScenarioDefinition scenario,
            ScenarioDefinition sunRidge,
            ScenarioDefinition windCoast,
            ScenarioDefinition pineBasin,
            ScenarioDefinition aridPlateau,
            ResearchTreeDefinition research,
            PurePolyCatalog purePoly = null)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var gameRoot = new GameObject("GameRoot");
            var bootstrap = gameRoot.AddComponent<TestTerrainBootstrap>();
            var so = new SerializedObject(bootstrap);
            so.FindProperty("settings").objectReferenceValue = settings;
            so.FindProperty("scenarioDefinition").objectReferenceValue = scenario;
            so.FindProperty("researchTreeDefinition").objectReferenceValue = research;
            so.FindProperty("purePolyCatalog").objectReferenceValue = purePoly;
            so.FindProperty("startingMoney").floatValue = 1000f;
            var catalogProp = so.FindProperty("scenarioCatalog");
            catalogProp.arraySize = 5;
            catalogProp.GetArrayElementAtIndex(0).objectReferenceValue = scenario;
            catalogProp.GetArrayElementAtIndex(1).objectReferenceValue = sunRidge;
            catalogProp.GetArrayElementAtIndex(2).objectReferenceValue = windCoast;
            catalogProp.GetArrayElementAtIndex(3).objectReferenceValue = pineBasin;
            catalogProp.GetArrayElementAtIndex(4).objectReferenceValue = aridPlateau;
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
            var dayLighting = simRoot.AddComponent<DayCycleLighting>();
            dayLighting.Configure(clock, Object.FindAnyObjectByType<Light>());
            var network = simRoot.AddComponent<EnergyNetworkService>();
            var maintenance = simRoot.AddComponent<MaintenanceController>();
            var driver = simRoot.AddComponent<EnergySimulationDriver>();
            var scenarioController = simRoot.AddComponent<ScenarioController>();
            var researchController = simRoot.AddComponent<ResearchController>();
            researchController.Configure(research, driver, network, scenarioController, mapGenerator);
            scenarioController.Configure(scenario, driver, network, clock, mapGenerator, researchController);
            placement.Configure(mapGenerator, buildingRoot.transform, buildings, 1000f, researchController.Service);
            network.Configure(
                placement,
                mapGenerator,
                researchController.Service.GetEfficiencyBonus,
                () => scenarioController.Settlement.DemandScale,
                researchController.Service.GetStorageCapacityBonus);
            maintenance.Configure(placement);
            driver.Configure(clock, network, placement, maintenance, mapGenerator);

            var debugRoot = new GameObject("DebugRoot");
            debugRoot.transform.SetParent(gameRoot.transform, false);
            var overlayGo = new GameObject("MapDebugOverlay");
            overlayGo.transform.SetParent(debugRoot.transform, false);
            var overlay = overlayGo.AddComponent<MapDebugOverlay>();
            overlay.SetMapGenerator(mapGenerator);
            overlay.SetPlacementController(placement);
            overlay.SetEnergyDriver(driver);

            var edgeGo = new GameObject("NetworkEdgeOverlay");
            edgeGo.transform.SetParent(overlayGo.transform, false);
            var edgeOverlay = edgeGo.AddComponent<NetworkEdgeOverlay>();
            edgeOverlay.Configure(mapGenerator, network, driver);
            overlay.SetNetworkEdgeOverlay(edgeOverlay);

            var particlesGo = new GameObject("NetworkEdgeParticles");
            particlesGo.transform.SetParent(overlayGo.transform, false);
            var edgeParticles = particlesGo.AddComponent<NetworkEdgeParticles>();
            edgeParticles.Configure(mapGenerator, network, driver);
            overlay.SetNetworkEdgeParticles(edgeParticles);

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
            placementUi.Configure(placement, clock, researchController);

            var hudGo = new GameObject("EnergyHudUI");
            hudGo.transform.SetParent(debugRoot.transform, false);
            var hud = hudGo.AddComponent<EnergyHudUI>();
            hud.Configure(driver, clock, placement, researchController, maintenance, scenarioController);

            var inspectionGo = new GameObject("InspectionPanelUI");
            inspectionGo.transform.SetParent(debugRoot.transform, false);
            var inspection = inspectionGo.AddComponent<InspectionPanelUI>();
            inspection.Configure(
                overlay, mapGenerator, placement, network, clock, researchController, scenarioController, driver, maintenance);

            var notification = simRoot.AddComponent<NotificationController>();
            notification.Configure(
                driver, researchController, maintenance, network, scenarioController, clock);
            var sfxGo = new GameObject("SfxService");
            sfxGo.transform.SetParent(debugRoot.transform, false);
            var sfx = sfxGo.AddComponent<SfxService>();
            sfx.Configure(placement, notification);
            var musicGo = new GameObject("MusicService");
            musicGo.transform.SetParent(debugRoot.transform, false);
            var music = musicGo.AddComponent<MusicService>();
            music.Configure(null, clock);
            var notificationHudGo = new GameObject("NotificationHudUI");
            notificationHudGo.transform.SetParent(debugRoot.transform, false);
            var notificationHud = notificationHudGo.AddComponent<NotificationHudUI>();
            notificationHud.Configure(notification, sfx);

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

            var pauseGo = new GameObject("PauseOverlayUI");
            pauseGo.transform.SetParent(debugRoot.transform, false);
            var pauseOverlay = pauseGo.AddComponent<PauseOverlayUI>();

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
            CleanEnergy.UI.SettingsService.ApplyAll(controller);

            var focusGo = new GameObject("SelectionCameraFocus");
            focusGo.transform.SetParent(debugRoot.transform, false);
            var selectionFocus = focusGo.AddComponent<SelectionCameraFocus>();
            selectionFocus.Configure(overlay, mapGenerator, placement, controller);

            var tutorialController = simRoot.AddComponent<TutorialController>();
            tutorialController.Configure(
                controller, overlay, placement, researchController, scenarioController, mapGenerator);
            placementUi.Configure(placement, clock, researchController, tutorialController);
            var tutorialHudGo = new GameObject("TutorialHudUI");
            tutorialHudGo.transform.SetParent(debugRoot.transform, false);
            var tutorialHud = tutorialHudGo.AddComponent<TutorialHudUI>();
            tutorialHud.Configure(tutorialController);

            var saveLoad = simRoot.AddComponent<SaveLoadController>();
            saveLoad.Configure(
                mapGenerator, placement, researchController, scenarioController, clock, network, tutorialController, driver, sfx, maintenance);
            pauseOverlay.Configure(clock, placement, controller, saveLoad);
            var saveHudGo = new GameObject("SaveLoadHudUI");
            saveHudGo.transform.SetParent(debugRoot.transform, false);
            var saveHud = saveHudGo.AddComponent<SaveLoadHudUI>();
            saveHud.Configure(saveLoad);

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        public static void EnsureMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var menuGo = new GameObject("MainMenuUI");
            var menu = menuGo.AddComponent<MainMenuUI>();
            ApplyCanonicalScenarios(menu);
            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
        }

        /// <summary>Writes the locked four-scenario catalog onto a MainMenuUI via SerializedObject.</summary>
        public static void ApplyCanonicalScenarios(MainMenuUI menu)
        {
            if (menu == null)
            {
                return;
            }

            var ids = MainMenuUI.CanonicalScenarioIds;
            var labels = MainMenuUI.CanonicalScenarioLabels;
            menu.ConfigureScenarios(ids, labels);

            var so = new SerializedObject(menu);
            var idsProp = so.FindProperty("scenarioIds");
            var labelsProp = so.FindProperty("scenarioLabels");
            if (idsProp != null && labelsProp != null)
            {
                idsProp.arraySize = ids.Length;
                labelsProp.arraySize = labels.Length;
                for (var i = 0; i < ids.Length; i++)
                {
                    idsProp.GetArrayElementAtIndex(i).stringValue = ids[i];
                    labelsProp.GetArrayElementAtIndex(i).stringValue = labels[i];
                }

                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorUtility.SetDirty(menu);
        }

        public static void EnsureBootstrapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new GameObject("GameServices");
            go.AddComponent<GameServicesBootstrap>();
            go.AddComponent<MusicService>();
            EditorSceneManager.SaveScene(scene, BootstrapScenePath);
        }

        public static void UpdateBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BootstrapScenePath, true),
                new EditorBuildSettingsScene(MainMenuScenePath, true),
                new EditorBuildSettingsScene(ScenePath, true)
            };
        }
    }
}
