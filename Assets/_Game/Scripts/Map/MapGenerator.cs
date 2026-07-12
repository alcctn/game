using CleanEnergy.Core;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using CleanEnergy.TerrainGeneration;
using UnityEngine;

namespace CleanEnergy.Map
{
    /// <summary>
    /// Orchestrates height map, terrain, grid, slope and resource-layer generation.
    /// </summary>
    public sealed class MapGenerator : MonoBehaviour
    {
        [SerializeField] private MapGenerationSettings settings;
        [SerializeField] private Transform terrainRoot;
        [SerializeField] private bool generateOnStart = true;

        private readonly HeightMapGenerator _heightMapGenerator = new HeightMapGenerator();
        private readonly TerrainBuilder _terrainBuilder = new TerrainBuilder();
        private readonly SlopeCalculator _slopeCalculator = new SlopeCalculator();
        private readonly WaterFlowCalculator _waterFlowCalculator = new WaterFlowCalculator();
        private readonly SolarPotentialCalculator _solarPotentialCalculator = new SolarPotentialCalculator();
        private readonly WindPotentialCalculator _windPotentialCalculator = new WindPotentialCalculator();
        private readonly BiomeGenerator _biomeGenerator = new BiomeGenerator();
        private readonly BuildabilityCalculator _buildabilityCalculator = new BuildabilityCalculator();
        private readonly GridService _gridService = new GridService();
        private readonly EventBus _eventBus = new EventBus();

        private float[,] _lastHeightMap;

        public MapGenerationSettings Settings => settings;
        public GridService Grid => _gridService;
        public EventBus Events => _eventBus;
        public float[,] LastHeightMap => _lastHeightMap;
        public TerrainBuilder TerrainBuilder => _terrainBuilder;

        private void Start()
        {
            if (generateOnStart)
            {
                Generate();
            }
        }

        public void SetSettings(MapGenerationSettings newSettings)
        {
            settings = newSettings;
        }

        public void SetTerrainRoot(Transform root)
        {
            terrainRoot = root;
        }

        public void SetSeed(string seed)
        {
            if (settings == null)
            {
                Debug.LogError("[Map] Cannot set seed: settings are missing.");
                return;
            }

            settings.SetSeed(seed);
        }

        public bool Generate()
        {
            if (settings == null)
            {
                Debug.LogError("[Map] MapGenerationSettings is not assigned.");
                return false;
            }

            if (!settings.Validate(out var error))
            {
                Debug.LogError(error);
                return false;
            }

            _lastHeightMap = _heightMapGenerator.Generate(settings);

            var parent = terrainRoot != null ? terrainRoot : transform;
            _terrainBuilder.BuildOrUpdate(settings, _lastHeightMap, parent);

            _gridService.Create(
                settings.GridWidth,
                settings.GridHeight,
                settings.CellSize,
                Vector3.zero);

            for (var x = 0; x < settings.GridWidth; x++)
            {
                for (var y = 0; y < settings.GridHeight; y++)
                {
                    var elevation = _lastHeightMap[x, y] * settings.MaxHeight;
                    _gridService.SetElevation(new GridCoordinate(x, y), elevation);
                }
            }

            _slopeCalculator.Calculate(
                _lastHeightMap,
                _gridService,
                settings.MaxHeight,
                settings.CellSize,
                settings.MaxBuildableSlopeDegrees);

            _waterFlowCalculator.Calculate(_lastHeightMap, _gridService, settings);
            _solarPotentialCalculator.Calculate(_gridService, settings);
            _windPotentialCalculator.Calculate(_lastHeightMap, _gridService, settings);
            _biomeGenerator.Calculate(_gridService, settings);
            _buildabilityCalculator.Calculate(_gridService, settings);

            _eventBus.Publish(new MapGeneratedEvent(settings.Seed, settings.GridWidth, settings.GridHeight));
            return true;
        }
    }
}
