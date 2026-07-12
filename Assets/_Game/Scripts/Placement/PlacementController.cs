using System;
using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Core;
using CleanEnergy.Economy;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using CleanEnergy.Scenario;
using CleanEnergy.Settlements;
using CleanEnergy.UI;
using CleanEnergy.Workers;
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
    /// Demolish undo snapshot for ring-stack groups (S55 / S75 / S89).
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
        [SerializeField] private PlacementValidityOverlay validityOverlay;
        [SerializeField] private float startingMoney = 1000f;
        [SerializeField] private BuildingDefinition[] availableBuildings;

        private readonly BuildingFactory _factory = new BuildingFactory();
        private PlacementValidator _validator = new PlacementValidator();
        private readonly GridOccupancyService _occupancy = new GridOccupancyService();
        private Wallet _wallet;
        private IBuildingUnlockQuery _buildingUnlocks;
        private IActiveSettlementQuery _settlement;
        private IWorkerQuery _workers;
        private LevelDefinition _level;
        private BuildingDefinition _selected;
        private IReadOnlyList<string> _lastFailures = Array.Empty<string>();
        private bool _placementArmed;
        private int _rotation;
        private GridCoordinate? _hoverCoordinate;
        private bool _hoverValid;
        private readonly List<List<DemolishUndoSnapshot>> _demolishUndoStack =
            new List<List<DemolishUndoSnapshot>>();

        public const int MaxDemolishUndoGroups = 3;

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
        public LevelDefinition Level => _level;
        public bool RestrictBuildMenuToEnergy =>
            _level != null && _level.RestrictBuildMenuToEnergy;
        public bool HasDemolishUndo => _demolishUndoStack.Count > 0;
        public int DemolishUndoStackDepth => _demolishUndoStack.Count;
        public int DemolishUndoCount =>
            _demolishUndoStack.Count > 0 ? _demolishUndoStack[_demolishUndoStack.Count - 1].Count : 0;
        public IReadOnlyList<DemolishUndoSnapshot> DemolishUndoGroup =>
            _demolishUndoStack.Count > 0
                ? _demolishUndoStack[_demolishUndoStack.Count - 1]
                : (IReadOnlyList<DemolishUndoSnapshot>)System.Array.Empty<DemolishUndoSnapshot>();

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

            if (validityOverlay == null)
            {
                var go = new GameObject("PlacementValidityOverlay");
                go.transform.SetParent(transform, false);
                validityOverlay = go.AddComponent<PlacementValidityOverlay>();
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
                RefreshValidityOverlay();
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
                _rotation,
                _settlement,
                _workers,
                _level);

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

        public void SetLevelServices(
            LevelDefinition level,
            IActiveSettlementQuery settlement,
            IWorkerQuery workers)
        {
            _level = level;
            _settlement = settlement;
            _workers = workers;
            _validator = PlacementValidator.CreateForLevel(settlement, workers);
        }

        public float GetBuildCost(BuildingDefinition definition, GridCoordinate coordinate)
        {
            return PowerLinePlacementCost.ComputeEffectiveCost(definition, coordinate, _occupancy);
        }

        public float GetAutoConnectCost(BuildingDefinition definition, GridCoordinate coordinate)
        {
            if (_level == null)
            {
                return 0f;
            }

            return AutoConnectionCost.Compute(
                definition,
                coordinate,
                _settlement,
                _level.ConnectionCostPerCell,
                _level.AutoConnectEnabled);
        }

        public float GetTotalPlacementCost(BuildingDefinition definition, GridCoordinate coordinate)
        {
            return GetBuildCost(definition, coordinate) + GetAutoConnectCost(definition, coordinate);
        }

        public void SelectBuilding(BuildingDefinition definition)
        {
            if (definition != null
                && RestrictBuildMenuToEnergy
                && definition.Category != BuildingCategory.Energy)
            {
                return;
            }

            _selected = definition;
            _placementArmed = definition != null;
            _rotation = 0;
            _lastFailures = Array.Empty<string>();
            if (definition == null)
            {
                preview?.Hide();
                validityOverlay?.Hide();
            }
            else
            {
                RefreshValidityOverlay();
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
            validityOverlay?.Hide();
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

            return GetTotalPlacementCost(_selected, _hoverCoordinate.Value);
        }

        public float GetHoverBuildCost()
        {
            if (_selected == null || !_hoverCoordinate.HasValue)
            {
                return _selected != null ? _selected.Cost : 0f;
            }

            return GetBuildCost(_selected, _hoverCoordinate.Value);
        }

        public float GetHoverAutoConnectCost()
        {
            if (_selected == null || !_hoverCoordinate.HasValue)
            {
                return 0f;
            }

            return GetAutoConnectCost(_selected, _hoverCoordinate.Value);
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
                _rotation,
                _settlement,
                _workers,
                _level);
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

            if (!_wallet.TrySpend(GetTotalPlacementCost(_selected, coordinate)))
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
            CancelPlacement();
            return PlacementValidationResult.Success();
        }

        private void RefreshValidityOverlay()
        {
            if (validityOverlay == null || _selected == null || mapGenerator == null)
            {
                validityOverlay?.Hide();
                return;
            }

            validityOverlay.Rebuild(
                _selected,
                mapGenerator.Grid,
                _validator,
                _occupancy,
                _wallet,
                _buildingUnlocks,
                _rotation,
                _settlement,
                _workers,
                _level);
        }

        public void ClearDemolishUndo()
        {
            _demolishUndoStack.Clear();
        }

        public bool TryUndoLastDemolish()
        {
            if (_demolishUndoStack.Count == 0 || _wallet == null)
            {
                return false;
            }

            var group = _demolishUndoStack[_demolishUndoStack.Count - 1];
            var totalRefund = 0f;
            for (var i = 0; i < group.Count; i++)
            {
                totalRefund += group[i].RefundAmount;
            }

            if (!_wallet.TrySpend(totalRefund))
            {
                return false;
            }

            _demolishUndoStack.RemoveAt(_demolishUndoStack.Count - 1);
            var snaps = new List<DemolishUndoSnapshot>(group);

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
                    var restored = 0f;
                    for (var j = 0; j < i; j++)
                    {
                        restored += snaps[j].RefundAmount;
                    }

                    _wallet.Add(totalRefund - restored);
                    _demolishUndoStack.Add(snaps);
                    TrimDemolishUndoStack();
                    return false;
                }
            }

            Debug.Log($"[Placement] Undo demolish group ({snaps.Count}). Stack={_demolishUndoStack.Count}");
            return true;
        }

        public bool TryDemolish(GridCoordinate coordinate, out float refund)
        {
            return TryDemolishMany(new[] { coordinate }, out refund);
        }

        /// <summary>
        /// Demolishes buildings at the given cells as one undo group (max 8).
        /// Pushes onto a LIFO stack of up to <see cref="MaxDemolishUndoGroups"/> groups.
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

            var group = new List<DemolishUndoSnapshot>(targets.Count);
            for (var i = 0; i < targets.Count; i++)
            {
                var building = targets[i];
                var amount = Mathf.Max(0f, building.Definition.Cost * 0.5f);
                group.Add(new DemolishUndoSnapshot(
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

            _demolishUndoStack.Add(group);
            TrimDemolishUndoStack();
            Debug.Log($"[Placement] Demolished group ({targets.Count}). Refund={refund:F0} Stack={_demolishUndoStack.Count}");
            return true;
        }

        private void TrimDemolishUndoStack()
        {
            while (_demolishUndoStack.Count > MaxDemolishUndoGroups)
            {
                _demolishUndoStack.RemoveAt(0);
            }
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
            if (ImguiHitTest.IsPointerOverGui())
            {
                return true;
            }

            // Fallback bands if HUD has not registered yet this session.
            return Input.mousePosition.x < 320f
                   || Input.mousePosition.x > Screen.width - 320f
                   || (Input.mousePosition.x > Screen.width - 180f && Input.mousePosition.y < 100f);
        }
    }
}
