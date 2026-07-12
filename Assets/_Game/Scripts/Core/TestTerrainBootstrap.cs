using CleanEnergy.Buildings;
using CleanEnergy.CameraSystem;
using CleanEnergy.DebugTools;
using CleanEnergy.Map;
using CleanEnergy.Placement;
using CleanEnergy.UI;
using UnityEngine;

namespace CleanEnergy.Core
{
    /// <summary>
    /// Wires Sprint 01–03 test scene objects at runtime if references are missing.
    /// </summary>
    public sealed class TestTerrainBootstrap : MonoBehaviour
    {
        [SerializeField] private MapGenerationSettings settings;
        [SerializeField] private BuildingDefinition[] buildingDefinitions;
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
        }

        private BuildingDefinition[] ResolveBuildings()
        {
            if (buildingDefinitions != null && buildingDefinitions.Length > 0)
            {
                return buildingDefinitions;
            }

            return new[]
            {
                CreateRuntimeBuilding(
                    "water_wheel", "Water Wheel", "Starter hydro",
                    80f, 8f, 25f, 8f, 0f, 0f, true, false,
                    new Color(0.25f, 0.55f, 0.95f)),
                CreateRuntimeBuilding(
                    "small_solar", "Small Solar", "Daytime solar array",
                    120f, 12f, 20f, 0f, 0.45f, 0f, false, true,
                    new Color(0.95f, 0.8f, 0.2f)),
                CreateRuntimeBuilding(
                    "small_wind", "Small Wind", "Open-area turbine",
                    150f, 14f, 28f, 0f, 0f, 0.4f, false, true,
                    new Color(0.65f, 0.85f, 0.95f))
            };
        }

        private static BuildingDefinition CreateRuntimeBuilding(
            string id,
            string name,
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
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.name = id;
            def.Configure(
                id, name, description, BuildingCategory.Energy,
                cost, power, maxSlope, minWater, minSolar, minWind,
                adjacentWater, requireBuildable, color);
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
