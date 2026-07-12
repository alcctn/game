using System;
using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Core;
using CleanEnergy.Economy;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using UnityEngine;

namespace CleanEnergy.Placement
{
    public sealed class BuildingPlacedEvent
    {
        public BuildingInstance Instance { get; }

        public BuildingPlacedEvent(BuildingInstance instance)
        {
            Instance = instance;
        }
    }

    /// <summary>
    /// Selects a building, previews validity, and commits placement.
    /// </summary>
    public sealed class PlacementController : MonoBehaviour
    {
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private Transform buildingRoot;
        [SerializeField] private PlacementPreview preview;
        [SerializeField] private float startingMoney = 1000f;
        [SerializeField] private BuildingDefinition[] availableBuildings;

        private readonly BuildingFactory _factory = new BuildingFactory();
        private readonly PlacementValidator _validator = new PlacementValidator();
        private readonly GridOccupancyService _occupancy = new GridOccupancyService();
        private Wallet _wallet;
        private IBuildingUnlockQuery _buildingUnlocks;
        private BuildingDefinition _selected;
        private IReadOnlyList<string> _lastFailures = Array.Empty<string>();
        private bool _placementArmed;
        private int _rotation;

        public Wallet Wallet => _wallet;
        public IBuildingUnlockQuery BuildingUnlocks => _buildingUnlocks;
        public GridOccupancyService Occupancy => _occupancy;
        public BuildingDefinition SelectedBuilding => _selected;
        public IReadOnlyList<string> LastFailureReasons => _lastFailures;
        public bool IsPlacementActive => _placementArmed && _selected != null;
        public int PlacementRotation => _rotation;
        public IReadOnlyList<BuildingDefinition> AvailableBuildings => availableBuildings;

        public event Action<BuildingPlacedEvent> BuildingPlaced;
        public event Action PlacementRejected;

        private void Awake()
        {
            _wallet = new Wallet(startingMoney);
            if (preview == null)
            {
                preview = gameObject.AddComponent<PlacementPreview>();
            }
        }

        private void OnEnable()
        {
            if (mapGenerator != null)
            {
                mapGenerator.Events.MapGenerated += OnMapGenerated;
            }
        }

        private void OnDisable()
        {
            if (mapGenerator != null)
            {
                mapGenerator.Events.MapGenerated -= OnMapGenerated;
            }
        }

        private void Update()
        {
            if (!IsPlacementActive || mapGenerator == null || !mapGenerator.Grid.IsInitialized)
            {
                preview?.Hide();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
                return;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                _rotation = (_rotation + 1) % 4;
            }

            if (!TryGetHoveredCoordinate(out var coordinate))
            {
                preview?.Hide();
                return;
            }

            var result = _validator.Validate(
                _selected,
                coordinate,
                mapGenerator.Grid,
                _occupancy,
                _wallet,
                _buildingUnlocks);
            _lastFailures = result.FailureReasons;

            if (mapGenerator.Grid.TryGetCell(coordinate, out var cell))
            {
                preview.Show(_selected, cell.WorldPosition, result.IsValid, _rotation);
            }

            if (Input.GetMouseButtonDown(0) && !IsPointerOverImgui())
            {
                TryPlace(coordinate);
            }
        }

        public void Configure(
            MapGenerator generator,
            Transform buildingsParent,
            BuildingDefinition[] buildings,
            float money,
            IBuildingUnlockQuery buildingUnlocks = null)
        {
            if (mapGenerator != null)
            {
                mapGenerator.Events.MapGenerated -= OnMapGenerated;
            }

            mapGenerator = generator;
            buildingRoot = buildingsParent;
            availableBuildings = buildings;
            startingMoney = money;
            _wallet = new Wallet(money);
            _buildingUnlocks = buildingUnlocks;
            if (mapGenerator != null)
            {
                mapGenerator.Events.MapGenerated += OnMapGenerated;
            }
        }

        public void SetBuildingUnlockQuery(IBuildingUnlockQuery buildingUnlocks)
        {
            _buildingUnlocks = buildingUnlocks;
        }

        public void SelectBuilding(BuildingDefinition definition)
        {
            _selected = definition;
            _placementArmed = definition != null;
            _rotation = 0;
            _lastFailures = Array.Empty<string>();
            if (definition == null)
            {
                preview?.Hide();
            }
        }

        public void CancelPlacement()
        {
            _placementArmed = false;
            _selected = null;
            _rotation = 0;
            _lastFailures = Array.Empty<string>();
            preview?.Hide();
        }

        /// <summary>Cycles placement yaw for tests and input.</summary>
        public void CycleRotation()
        {
            _rotation = (_rotation + 1) % 4;
        }

        public BuildingDefinition FindDefinition(string definitionId)
        {
            if (availableBuildings == null || string.IsNullOrEmpty(definitionId))
            {
                return null;
            }

            for (var i = 0; i < availableBuildings.Length; i++)
            {
                var def = availableBuildings[i];
                if (def != null && def.Id == definitionId)
                {
                    return def;
                }
            }

            return null;
        }

        public bool TryPlaceFromSave(
            string definitionId,
            GridCoordinate coordinate,
            int rotation,
            float storedEnergy,
            float maintenanceLevel = 1f)
        {
            var definition = FindDefinition(definitionId);
            if (definition == null || mapGenerator == null || !mapGenerator.Grid.IsInitialized)
            {
                return false;
            }

            if (_occupancy.IsOccupied(coordinate))
            {
                return false;
            }

            var parent = buildingRoot != null ? buildingRoot : transform;
            var instance = _factory.Create(definition, coordinate, mapGenerator.Grid, parent, rotation);
            if (instance == null)
            {
                return false;
            }

            instance.StoredEnergy = Mathf.Max(0f, storedEnergy);
            instance.MaintenanceLevel = Mathf.Clamp(maintenanceLevel, 0.4f, 1f);
            if (!_occupancy.TryOccupy(instance))
            {
                if (instance.GameObject != null)
                {
                    Destroy(instance.GameObject);
                }

                return false;
            }

            mapGenerator.Grid.SetOccupyingBuildingId(coordinate, instance.InstanceId);
            BuildingPlaced?.Invoke(new BuildingPlacedEvent(instance));
            return true;
        }

        public void ResetFactoryIds()
        {
            _factory.ResetIds();
        }

        public PlacementValidationResult TryPlace(GridCoordinate coordinate)
        {
            if (_selected == null || mapGenerator == null)
            {
                var fail = PlacementValidationResult.Failure(new[] { "No building selected." });
                _lastFailures = fail.FailureReasons;
                return fail;
            }

            var result = _validator.Validate(
                _selected,
                coordinate,
                mapGenerator.Grid,
                _occupancy,
                _wallet,
                _buildingUnlocks);
            _lastFailures = result.FailureReasons;
            if (!result.IsValid)
            {
                PlacementRejected?.Invoke();
                Debug.Log(
                    $"[Placement] Building '{_selected.Id}' could not be placed at {coordinate}: {string.Join("; ", result.FailureReasons)}");
                return result;
            }

            var parent = buildingRoot != null ? buildingRoot : transform;
            var instance = _factory.Create(_selected, coordinate, mapGenerator.Grid, parent, _rotation);
            if (instance == null)
            {
                return PlacementValidationResult.Failure(new[] { "Factory failed to create building." });
            }

            if (!_occupancy.TryOccupy(instance))
            {
                if (instance.GameObject != null)
                {
                    Destroy(instance.GameObject);
                }

                return PlacementValidationResult.Failure(new[] { "Cell became occupied." });
            }

            if (!_wallet.TrySpend(_selected.Cost))
            {
                _occupancy.Release(coordinate);
                if (instance.GameObject != null)
                {
                    Destroy(instance.GameObject);
                }

                return PlacementValidationResult.Failure(new[] { "Payment failed." });
            }

            mapGenerator.Grid.SetOccupyingBuildingId(coordinate, instance.InstanceId);
            BuildingPlaced?.Invoke(new BuildingPlacedEvent(instance));
            Debug.Log($"[Placement] Placed '{_selected.Id}' at {coordinate}. Money={_wallet.Money:F0}");
            return PlacementValidationResult.Success();
        }

        private void OnMapGenerated(MapGeneratedEvent _)
        {
            _occupancy.Clear();
            preview?.Hide();
        }

        private bool TryGetHoveredCoordinate(out GridCoordinate coordinate)
        {
            coordinate = default;
            var cam = Camera.main;
            if (cam == null || mapGenerator == null)
            {
                return false;
            }

            var ray = cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, 5000f))
            {
                var plane = new Plane(Vector3.up, Vector3.zero);
                if (!plane.Raycast(ray, out var enter))
                {
                    return false;
                }

                hit.point = ray.GetPoint(enter);
            }

            return mapGenerator.Grid.TryWorldToGrid(hit.point, out coordinate);
        }

        private static bool IsPointerOverImgui()
        {
            // Rough guard: left UI panels and right building menu.
            return Input.mousePosition.x < 320f
                   || Input.mousePosition.x > Screen.width - 320f
                   || (Input.mousePosition.x > Screen.width - 180f && Input.mousePosition.y < 100f);
        }
    }
}
