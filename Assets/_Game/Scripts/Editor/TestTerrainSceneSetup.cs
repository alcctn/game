using System.IO;
using CleanEnergy.Buildings;
using CleanEnergy.CameraSystem;
using CleanEnergy.Core;
using CleanEnergy.DebugTools;
using CleanEnergy.Map;
using CleanEnergy.Placement;
using CleanEnergy.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CleanEnergy.Editor
{
    /// <summary>
    /// Creates MapGenerationSettings, building definitions, and Test_Terrain scene hierarchy.
    /// </summary>
    public static class TestTerrainSceneSetup
    {
        private const string SettingsPath = "Assets/_Game/Data/Map/MapGenerationSettings.asset";
        private const string ScenePath = "Assets/_Game/Scenes/Test_Terrain.unity";
        private const string BuildingsFolder = "Assets/_Game/Data/Buildings";

        [MenuItem("Clean Energy/Setup Test Terrain Scene")]
        public static void Setup()
        {
            EnsureFolders();
            var settings = CreateOrLoadSettings();
            var buildings = CreateOrLoadBuildings();
            CreateScene(settings, buildings);
            AssetDatabase.SaveAssets();
            Debug.Log("[Setup] Test_Terrain scene, map settings, and building definitions are ready.");
        }

        [MenuItem("Clean Energy/Create Building Definitions")]
        public static void CreateBuildingDefinitionsMenu()
        {
            EnsureFolders();
            CreateOrLoadBuildings();
            AssetDatabase.SaveAssets();
            Debug.Log("[Setup] Building definitions created/updated.");
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory("Assets/_Game/Data/Map");
            Directory.CreateDirectory(BuildingsFolder);
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

        private static BuildingDefinition[] CreateOrLoadBuildings()
        {
            return new[]
            {
                CreateOrUpdateBuilding(
                    "water_wheel",
                    "Water Wheel",
                    "Starter hydro on a stream bank",
                    80f, 8f, 25f, 8f, 0f, 0f, true, false,
                    new Color(0.25f, 0.55f, 0.95f)),
                CreateOrUpdateBuilding(
                    "small_solar",
                    "Small Solar",
                    "Daytime solar array",
                    120f, 12f, 20f, 0f, 0.45f, 0f, false, true,
                    new Color(0.95f, 0.8f, 0.2f)),
                CreateOrUpdateBuilding(
                    "small_wind",
                    "Small Wind",
                    "Open-area turbine",
                    150f, 14f, 28f, 0f, 0f, 0.4f, false, true,
                    new Color(0.65f, 0.85f, 0.95f))
            };
        }

        private static BuildingDefinition CreateOrUpdateBuilding(
            string id,
            string displayName,
            string description,
            float cost,
            float power,
            float maxSlope,
            float minWater,
            float minSolar,
            float minWind,
            bool adjacentWater,
            bool requireBuildable,
            Color color)
        {
            var path = $"{BuildingsFolder}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<BuildingDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<BuildingDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.Configure(
                id, displayName, description, BuildingCategory.Energy,
                cost, power, maxSlope, minWater, minSolar, minWind,
                adjacentWater, requireBuildable, color);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void CreateScene(MapGenerationSettings settings, BuildingDefinition[] buildings)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var gameRoot = new GameObject("GameRoot");
            var bootstrap = gameRoot.AddComponent<TestTerrainBootstrap>();
            var so = new SerializedObject(bootstrap);
            so.FindProperty("settings").objectReferenceValue = settings;
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
            placement.Configure(mapGenerator, buildingRoot.transform, buildings, 1000f);

            var debugRoot = new GameObject("DebugRoot");
            debugRoot.transform.SetParent(gameRoot.transform, false);
            var overlayGo = new GameObject("MapDebugOverlay");
            overlayGo.transform.SetParent(debugRoot.transform, false);
            var overlay = overlayGo.AddComponent<MapDebugOverlay>();
            overlay.SetMapGenerator(mapGenerator);
            overlay.SetPlacementController(placement);

            var uiGo = new GameObject("MapDebugUI");
            uiGo.transform.SetParent(debugRoot.transform, false);
            var ui = uiGo.AddComponent<MapDebugUI>();
            ui.Configure(mapGenerator, overlay);

            var placementUiGo = new GameObject("BuildingPlacementUI");
            placementUiGo.transform.SetParent(debugRoot.transform, false);
            var placementUi = placementUiGo.AddComponent<BuildingPlacementUI>();
            placementUi.Configure(placement);

            var cameraRoot = new GameObject("CameraRoot");
            cameraRoot.transform.SetParent(gameRoot.transform, false);
            var mainCam = Object.FindFirstObjectByType<Camera>();
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

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
        }
    }
}
