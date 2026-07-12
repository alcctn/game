using System.IO;
using CleanEnergy.CameraSystem;
using CleanEnergy.Core;
using CleanEnergy.DebugTools;
using CleanEnergy.Map;
using CleanEnergy.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CleanEnergy.Editor
{
    /// <summary>
    /// Creates MapGenerationSettings asset and Test_Terrain scene hierarchy.
    /// </summary>
    public static class TestTerrainSceneSetup
    {
        private const string SettingsPath = "Assets/_Game/Data/Map/MapGenerationSettings.asset";
        private const string ScenePath = "Assets/_Game/Scenes/Test_Terrain.unity";

        [MenuItem("Clean Energy/Setup Test Terrain Scene")]
        public static void Setup()
        {
            EnsureFolders();
            var settings = CreateOrLoadSettings();
            CreateScene(settings);
            AssetDatabase.SaveAssets();
            Debug.Log("[Setup] Test_Terrain scene and MapGenerationSettings are ready.");
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory("Assets/_Game/Data/Map");
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

        private static void CreateScene(MapGenerationSettings settings)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var gameRoot = new GameObject("GameRoot");
            var bootstrap = gameRoot.AddComponent<TestTerrainBootstrap>();
            var so = new SerializedObject(bootstrap);
            so.FindProperty("settings").objectReferenceValue = settings;
            so.ApplyModifiedPropertiesWithoutUndo();

            var mapRoot = new GameObject("MapRoot");
            mapRoot.transform.SetParent(gameRoot.transform, false);
            var terrainRoot = new GameObject("TerrainRoot");
            terrainRoot.transform.SetParent(mapRoot.transform, false);

            var mapGenerator = mapRoot.AddComponent<MapGenerator>();
            var mapSo = new SerializedObject(mapGenerator);
            mapSo.FindProperty("settings").objectReferenceValue = settings;
            mapSo.FindProperty("terrainRoot").objectReferenceValue = terrainRoot.transform;
            mapSo.FindProperty("generateOnStart").boolValue = true;
            mapSo.ApplyModifiedPropertiesWithoutUndo();

            var debugRoot = new GameObject("DebugRoot");
            debugRoot.transform.SetParent(gameRoot.transform, false);
            var overlayGo = new GameObject("MapDebugOverlay");
            overlayGo.transform.SetParent(debugRoot.transform, false);
            var overlay = overlayGo.AddComponent<MapDebugOverlay>();
            overlay.SetMapGenerator(mapGenerator);

            var uiGo = new GameObject("MapDebugUI");
            uiGo.transform.SetParent(debugRoot.transform, false);
            var ui = uiGo.AddComponent<MapDebugUI>();
            ui.Configure(mapGenerator, overlay);

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
