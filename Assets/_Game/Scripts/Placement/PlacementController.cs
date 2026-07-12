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
    /// One-step demolish undo snapshot (S55).
    /// </summary>
    public sealed class DemolishUndoSnapshot
    {
        public string DefinitionId { get; }
        public GridCoordinate Coordinate { get; }
        public int Rotation { get; }
        public float StoredEnergy { get; }
        public float MaintenanceLevel { get; }
        public float RefundAmount { get; }

        public DemolishUndoSnapshot(
            string definitionId,
            GridCoordinate coordinate,
            int rotation,
            float storedEnergy,
            float maintenanceLevel,
            float refundAmount)
        {
            DefinitionId = definitionId;
            Coordinate = coordinate;
            Rotation = rotation;
            StoredEnergy = storedEnergy;
            MaintenanceLevel = maintenanceLevel;
            RefundAmount = refundAmount;
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
        private GridCoordinate? _hoverCoordinate;
        private bool _hoverValid;
        private readonly List<DemolishUndoSnapshot> _demolishUndoGroup = new List<DemolishUndoSnapshot>();

        public Wallet Wallet => _wallet;
        public IBuildingUnlockQuery BuildingUnlocks => _buildingUnlocks;
        public GridOccupancyService Occupancy => _occupancy;
        public BuildingDefinition SelectedBuilding => _selected;
        public IReadOnlyList<string> LastFailureReasons => _lastFailures;
        public bool IsPlacementActive => _placementArmed && _selected != null;
        public int PlacementRotation => _rotation;
        public GridCoordinate? HoverCoordinate => _hoverCoordinate;
        public bool IsHoverValid => _hoverValid;
        public IReadOnlyList<BuildingDefinition> AvailableBuildings => availableBuildings;
        public MapGenerator MapGenerator => mapGenerator;
        public bool HasDemolishUndo => _demolishUndoGroup.Count > 0;
        public int DemolishUndoCount => _demolishUndoGroup.Count;
        public IReadOnlyList<DemolishUndoSnapshot> DemolishUndoGroup => _demolishUndoGroup;

        public event Action<BuildingPlacedEvent> BuildingPlaced;
        public event Action<BuildingPlacedEvent> BuildingRemoved;
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
                _hoverCoordinate = null;
                _hoverValid = false;
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
                _hoverCoordinate = null;
                _hoverValid = false;
                return;
            }

            var result = _validator.Validate(
                _selected,
                coordinate,
                mapGenerator.Grid,
                _occupancy,
                _wallet,
                _buildingUnlocks,
                _rotation);
            _lastFailures = result.FailureReasons;
            _hoverCoordinate = coordinate;
            _hoverValid = result.IsValid;

            preview.Show(_selected, coordinate, mapGenerator.Grid, result.IsValid, _rotation);

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
            _hoverCoordinate = null;
            _hoverValid = false;
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

            SetFootprintOccupyingIds(instance, instance.InstanceId);
            BuildingPlaced?.Invoke(new BuildingPlacedEvent(instance));
            return true;
        }

        public void ResetFactoryIds()
        {
            _factory.ResetIds();
        }

        public float GetHoverEffectiveCost()
        {
            if (_selected == null || !_hoverCoordinate.HasValue)
            {
                return _selected != null ? _selected.Cost : 0f;
            }

            return PowerLinePlacementCost.ComputeEffectiveCost(
                _selected, _hoverCoordinate.Value, _occupancy);
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
                _buildingUnlocks,
                _rotation);
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

            if (!_wallet.TrySpend(PowerLinePlacementCost.ComputeEffectiveCost(
                    _selected, coordinate, _occupancy)))
            {
                _occupancy.ReleaseInstance(instance);
                if (instance.GameObject != null)
                {
                    Destroy(instance.GameObject);
                }

                return PlacementValidationResult.Failure(new[] { "Payment failed." });
            }

            SetFootprintOccupyingIds(instance, instance.InstanceId);
            ClearDemolishUndo();
            BuildingPlaced?.Invoke(new BuildingPlacedEvent(instance));
            Debug.Log($"[Placement] Placed '{_selected.Id}' at {coordinate}. Money={_wallet.Money:F0}");
            return PlacementValidationResult.Success();
        }

        public void ClearDemolishUndo()
        {
            _demolishUndoGroup.Clear();
        }

        public bool TryUndoLastDemolish()
        {
            if (_demolishUndoGroup.Count == 0 || _wallet == null)
            {
                return false;
            }

            var totalRefund = 0f;
            for (var i = 0; i < _demolishUndoGroup.Count; i++)
            {
                totalRefund += _demolishUndoGroup[i].RefundAmount;
            }

            if (!_wallet.TrySpend(totalRefund))
            {
                return false;
            }

            var snaps = new List<DemolishUndoSnapshot>(_demolishUndoGroup);
            _demolishUndoGroup.Clear();

            for (var i = 0; i < snaps.Count; i++)
            {
                var snap = snaps[i];
                if (!TryPlaceFromSave(
                        snap.DefinitionId,
                        snap.Coordinate,
                        snap.Rotation,
                        snap.StoredEnergy,
                        snap.MaintenanceLevel))
                {
                    // Restore remaining refund for buildings that did not come back.
                    var restored = 0f;
                    for (var j = 0; j < i; j++)
                    {
                        restored += snaps[j].RefundAmount;
                    }

                    _wallet.Add(totalRefund - restored);
                    _demolishUndoGroup.Clear();
                    _demolishUndoGroup.AddRange(snaps);
                    return false;
                }
            }

            Debug.Log($"[Placement] Undo demolish group ({snaps.Count}).");
            return true;
        }

        public bool TryDemolish(GridCoordinate coordinate, out float refund)
        {
            return TryDemolishMany(new[] { coordinate }, out refund);
        }

        /// <summary>
        /// Demolishes buildings at the given cells as one undo group (max 8).
        /// </summary>
        public bool TryDemolishMany(IReadOnlyList<GridCoordinate> coordinates, out float refund)
        {
            refund = 0f;
            if (coordinates == null || coordinates.Count == 0)
            {
                return false;
            }

            var targets = new List<BuildingInstance>();
            var seen = new HashSet<string>();
            var limit = Mathf.Min(coordinates.Count, 8);
            for (var i = 0; i < limit; i++)
            {
                if (!_occupancy.TryGet(coordinates[i], out var building)
                    || building?.Definition == null
                    || !seen.Add(building.InstanceId))
                {
                    continue;
                }

                targets.Add(building);
            }

            if (targets.Count == 0)
            {
                return false;
            }

            _demolishUndoGroup.Clear();
            for (var i = 0; i < targets.Count; i++)
            {
                var building = targets[i];
                var amount = Mathf.Max(0f, building.Definition.Cost * 0.5f);
                _demolishUndoGroup.Add(new DemolishUndoSnapshot(
                    building.Definition.Id,
                    building.Coordinate,
                    building.Rotation,
                    building.StoredEnergy,
                    building.MaintenanceLevel,
                    amount));
                refund += amount;
                _wallet?.Add(amount);
                _occupancy.ReleaseInstance(building);
                SetFootprintOccupyingIds(building, null);

                if (building.GameObject != null)
                {
                    Destroy(building.GameObject);
                }

                BuildingRemoved?.Invoke(new BuildingPlacedEvent(building));
            }

            Debug.Log($"[Placement] Demolished group ({targets.Count}). Refund={refund:F0}");
            return true;
        }

        private void SetFootprintOccupyingIds(BuildingInstance instance, string occupyingId)
        {
            if (instance?.Definition == null
                || mapGenerator == null
                || !mapGenerator.Grid.IsInitialized)
            {
                return;
            }

            var cells = BuildingFootprint.GetCells(
                instance.Definition, instance.Coordinate, instance.Rotation);
            for (var i = 0; i < cells.Count; i++)
            {
                if (mapGenerator.Grid.InBounds(cells[i]))
                {
                    mapGenerator.Grid.SetOccupyingBuildingId(cells[i], occupyingId);
                }
            }
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
