using System.Collections.Generic;
using CleanEnergy.Energy;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using CleanEnergy.Placement;
using UnityEngine;

namespace CleanEnergy.DebugTools
{
    /// <summary>
    /// Draws colored quads above cells for resource debug visualization.
    /// </summary>
    public sealed class MapDebugOverlay : MonoBehaviour
    {
        public const int MaxMultiSelection = 8;

        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private PlacementController placementController;
        [SerializeField] private EnergySimulationDriver energyDriver;
        [SerializeField] private NetworkEdgeOverlay edgeOverlay;
        [SerializeField] private NetworkEdgeParticles edgeParticles;
        [SerializeField] private float overlayHeightOffset = 0.5f;
        [SerializeField] private float maxSlopeForColor = 45f;
        [SerializeField] private float maxWaterFlowForColor = 64f;

        private DebugViewMode _mode = DebugViewMode.Normal;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private Mesh _mesh;
        private Material _material;
        private GridCoordinate? _selectedCell;
        private readonly List<GridCoordinate> _multiSelection = new List<GridCoordinate>();

        public DebugViewMode Mode => _mode;
        public GridCoordinate? SelectedCell => _selectedCell;
        public IReadOnlyList<GridCoordinate> MultiSelectedCells => _multiSelection;

        public event System.Action<DebugViewMode> ModeChanged;
        public event System.Action<GridCoordinate?> SelectionChanged;
        public event System.Action MultiSelectionChanged;

        private void Awake()
        {
            EnsureComponents();
            if (mapGenerator != null)
            {
                mapGenerator.Events.MapGenerated += OnMapGenerated;
            }
        }

        private void OnDestroy()
        {
            if (mapGenerator != null)
            {
                mapGenerator.Events.MapGenerated -= OnMapGenerated;
            }

            if (energyDriver != null)
            {
                energyDriver.BalanceUpdated -= OnBalanceUpdated;
            }

            if (_material != null)
            {
                Destroy(_material);
            }
        }

        private void Update()
        {
            if (placementController != null && placementController.IsPlacementActive)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                var overGui = IsPointerOverGui();
                // #region agent log
                CleanEnergy.DebugTools.AgentDebugLog.Write(
                    "A",
                    "MapDebugOverlay.Update",
                    "click",
                    "{\"overGui\":" + (overGui ? "true" : "false") +
                    ",\"willSelect\":" + (!overGui ? "true" : "false") +
                    ",\"mx\":" + Input.mousePosition.x.ToString("F0") +
                    ",\"my\":" + Input.mousePosition.y.ToString("F0") +
                    ",\"sw\":" + Screen.width +
                    ",\"sh\":" + Screen.height + "}");
                // #endregion
                if (!overGui)
                {
                    TrySelectCellUnderCursor();
                }
                else
                {
                    // #region agent log
                    CleanEnergy.DebugTools.AgentDebugLog.Write(
                        "A",
                        "MapDebugOverlay.Update",
                        "blocked_by_gui",
                        "{}");
                    // #endregion
                }
            }
        }

        public void SetMapGenerator(MapGenerator generator)
        {
            if (mapGenerator != null)
            {
                mapGenerator.Events.MapGenerated -= OnMapGenerated;
            }

            mapGenerator = generator;
            if (mapGenerator != null)
            {
                mapGenerator.Events.MapGenerated += OnMapGenerated;
            }
        }

        public void SetPlacementController(PlacementController controller)
        {
            placementController = controller;
        }

        public void SetEnergyDriver(EnergySimulationDriver driver)
        {
            if (energyDriver != null)
            {
                energyDriver.BalanceUpdated -= OnBalanceUpdated;
            }

            energyDriver = driver;
            if (energyDriver != null)
            {
                energyDriver.BalanceUpdated += OnBalanceUpdated;
            }

            SyncEdgeOverlay();
        }

        public void SetNetworkEdgeOverlay(NetworkEdgeOverlay overlay)
        {
            edgeOverlay = overlay;
            SyncEdgeOverlay();
        }

        public void SetNetworkEdgeParticles(NetworkEdgeParticles particles)
        {
            edgeParticles = particles;
            SyncEdgeOverlay();
        }

        private void OnBalanceUpdated(EnergyBalanceResult _)
        {
            if (_mode == DebugViewMode.Network
                || _mode == DebugViewMode.Production
                || _mode == DebugViewMode.Demand
                || _mode == DebugViewMode.Environmental)
            {
                Rebuild();
            }

            if (_mode == DebugViewMode.Network)
            {
                edgeOverlay?.Rebuild();
                edgeParticles?.Rebuild();
            }
        }

        public void SetMode(DebugViewMode mode)
        {
            if (_mode == mode)
            {
                return;
            }

            _mode = mode;
            Rebuild();
            SyncEdgeOverlay();
            ModeChanged?.Invoke(_mode);
        }

        private void SyncEdgeOverlay()
        {
            var networkVisible = _mode == DebugViewMode.Network;
            edgeOverlay?.SetVisible(networkVisible);
            edgeParticles?.SetVisible(networkVisible);
        }

        public void SetSelection(GridCoordinate? coordinate)
        {
            if (_selectedCell.HasValue == coordinate.HasValue
                && (!_selectedCell.HasValue || _selectedCell.Value.Equals(coordinate.Value)))
            {
                return;
            }

            _selectedCell = coordinate;
            SelectionChanged?.Invoke(_selectedCell);
        }

        public void ClearSelection()
        {
            SetSelection(null);
            ClearMultiSelection();
        }

        public void ClearMultiSelection()
        {
            if (_multiSelection.Count == 0)
            {
                return;
            }

            _multiSelection.Clear();
            MultiSelectionChanged?.Invoke();
        }

        /// <summary>Adds or removes a cell from the multi-select set (max 8).</summary>
        public bool ToggleMultiSelect(GridCoordinate coordinate)
        {
            for (var i = 0; i < _multiSelection.Count; i++)
            {
                if (_multiSelection[i].Equals(coordinate))
                {
                    _multiSelection.RemoveAt(i);
                    MultiSelectionChanged?.Invoke();
                    return true;
                }
            }

            if (_multiSelection.Count >= MaxMultiSelection)
            {
                return false;
            }

            _multiSelection.Add(coordinate);
            MultiSelectionChanged?.Invoke();
            return true;
        }

        public static bool ShouldToggleMultiSelect(bool shiftHeld) => shiftHeld;

        public bool TryGetSelectedCellData(out GridCellData cell)
        {
            cell = null;
            if (!_selectedCell.HasValue || mapGenerator == null || !mapGenerator.Grid.IsInitialized)
            {
                return false;
            }

            return mapGenerator.Grid.TryGetCell(_selectedCell.Value, out cell);
        }

        private void OnMapGenerated(Core.MapGeneratedEvent _)
        {
            ClearSelection();
            Rebuild();
        }

        private void EnsureComponents()
        {
            _meshFilter = gameObject.GetComponent<MeshFilter>();
            if (_meshFilter == null)
            {
                _meshFilter = gameObject.AddComponent<MeshFilter>();
            }

            _meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (_meshRenderer == null)
            {
                _meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }

            if (_mesh == null)
            {
                _mesh = new Mesh { name = "MapDebugOverlayMesh" };
            }

            _meshFilter.sharedMesh = _mesh;

            if (_material == null)
            {
                var shader = Shader.Find("CleanEnergy/UnlitVertexColor")
                             ?? Shader.Find("Sprites/Default")
                             ?? Shader.Find("Unlit/Color");
                _material = new Material(shader);
                if (_material.HasProperty("_Color"))
                {
                    _material.SetColor("_Color", new Color(1f, 1f, 1f, 0.65f));
                }
            }

            _meshRenderer.sharedMaterial = _material;
            _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _meshRenderer.receiveShadows = false;
        }

        private void Rebuild()
        {
            EnsureComponents();

            if (_mode == DebugViewMode.Normal || mapGenerator == null || !mapGenerator.Grid.IsInitialized)
            {
                _mesh.Clear();
                _meshRenderer.enabled = false;
                return;
            }

            _meshRenderer.enabled = true;
            var grid = mapGenerator.Grid;
            var width = grid.Width;
            var height = grid.Height;
            var cellCount = width * height;
            var vertices = new Vector3[cellCount * 4];
            var colors = new Color[cellCount * 4];
            var triangles = new int[cellCount * 6];
            var half = grid.CellSize * 0.45f;
            var index = 0;

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var cell = grid.GetCell(new GridCoordinate(x, y));
                    var color = SampleColor(cell);
                    var center = cell.WorldPosition + Vector3.up * overlayHeightOffset;
                    var v = index * 4;
                    vertices[v] = center + new Vector3(-half, 0f, -half);
                    vertices[v + 1] = center + new Vector3(-half, 0f, half);
                    vertices[v + 2] = center + new Vector3(half, 0f, half);
                    vertices[v + 3] = center + new Vector3(half, 0f, -half);
                    colors[v] = color;
                    colors[v + 1] = color;
                    colors[v + 2] = color;
                    colors[v + 3] = color;
                    var t = index * 6;
                    triangles[t] = v;
                    triangles[t + 1] = v + 1;
                    triangles[t + 2] = v + 2;
                    triangles[t + 3] = v;
                    triangles[t + 4] = v + 2;
                    triangles[t + 5] = v + 3;
                    index++;
                }
            }

            _mesh.Clear();
            _mesh.vertices = vertices;
            _mesh.colors = colors;
            _mesh.triangles = triangles;
            _mesh.RecalculateBounds();
        }

        private Color SampleColor(GridCellData cell)
        {
            switch (_mode)
            {
                case DebugViewMode.Height:
                {
                    var maxHeight = mapGenerator.Settings != null ? mapGenerator.Settings.MaxHeight : 40f;
                    var t = maxHeight > 0f ? Mathf.Clamp01(cell.Elevation / maxHeight) : 0f;
                    return Color.Lerp(new Color(0.1f, 0.25f, 0.55f), new Color(0.95f, 0.9f, 0.7f), t);
                }
                case DebugViewMode.Slope:
                {
                    var slopeT = Mathf.Clamp01(cell.Slope / Mathf.Max(1f, maxSlopeForColor));
                    return Color.Lerp(new Color(0.2f, 0.7f, 0.25f), new Color(0.85f, 0.15f, 0.1f), slopeT);
                }
                case DebugViewMode.Water:
                {
                    if (cell.IsWater)
                    {
                        return new Color(0.05f, 0.35f, 0.85f, 0.85f);
                    }

                    var t = Mathf.Clamp01(cell.WaterFlow / Mathf.Max(1f, maxWaterFlowForColor));
                    return Color.Lerp(new Color(0.85f, 0.8f, 0.6f), new Color(0.15f, 0.55f, 0.95f), t);
                }
                case DebugViewMode.Solar:
                    return Color.Lerp(new Color(0.15f, 0.15f, 0.2f), new Color(1f, 0.85f, 0.2f), Mathf.Clamp01(cell.SolarPotential));
                case DebugViewMode.Wind:
                    return Color.Lerp(new Color(0.2f, 0.25f, 0.3f), new Color(0.55f, 0.85f, 1f), Mathf.Clamp01(cell.WindPotential));
                case DebugViewMode.Network:
                {
                    var coordinate = new GridCoordinate(cell.X, cell.Y);
                    if (energyDriver != null && energyDriver.TryGetHubUtilization(coordinate, out var util))
                    {
                        return NetworkUtilization.ColorForUtilization(util);
                    }

                    if (energyDriver != null && energyDriver.IsEnergyNetworkCell(coordinate))
                    {
                        return NetworkUtilization.EnergyNodeColor;
                    }

                    return NetworkUtilization.EmptyCellColor;
                }
                case DebugViewMode.Production:
                {
                    var coordinate = new GridCoordinate(cell.X, cell.Y);
                    if (energyDriver != null && energyDriver.TryGetProductionRatio(coordinate, out var ratio))
                    {
                        return ProductionUtilization.ColorForRatio(ratio);
                    }

                    if (energyDriver != null && energyDriver.IsOccupiedNonProducer(coordinate))
                    {
                        return ProductionUtilization.NeutralCellColor;
                    }

                    return ProductionUtilization.EmptyCellColor;
                }
                case DebugViewMode.Demand:
                {
                    var coordinate = new GridCoordinate(cell.X, cell.Y);
                    if (energyDriver != null && energyDriver.TryGetDemandRatio(coordinate, out var ratio))
                    {
                        return DemandUtilization.ColorForRatio(ratio);
                    }

                    if (energyDriver != null && energyDriver.IsProducerCell(coordinate))
                    {
                        return DemandUtilization.NeutralCellColor;
                    }

                    if (energyDriver != null && energyDriver.IsOccupiedNonProducer(coordinate))
                    {
                        return DemandUtilization.NeutralCellColor;
                    }

                    return DemandUtilization.EmptyCellColor;
                }
                case DebugViewMode.Environmental:
                {
                    var coordinate = new GridCoordinate(cell.X, cell.Y);
                    var occupancy = placementController != null ? placementController.Occupancy : null;
                    var score = EnvironmentalImpact.ScoreAt(coordinate, occupancy);
                    return EnvironmentalImpact.ColorForScore(score);
                }
                default:
                    return Color.white;
            }
        }

        private void TrySelectCellUnderCursor()
        {
            if (mapGenerator == null || !mapGenerator.Grid.IsInitialized)
            {
                return;
            }

            var cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            var ray = cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, 5000f))
            {
                var plane = new Plane(Vector3.up, Vector3.zero);
                if (!plane.Raycast(ray, out var enter))
                {
                    return;
                }

                hit.point = ray.GetPoint(enter);
            }

            if (mapGenerator.Grid.TryWorldToGrid(hit.point, out var coordinate))
            {
                var shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                if (ShouldToggleMultiSelect(shift))
                {
                    ToggleMultiSelect(coordinate);
                    SetSelection(coordinate);
                }
                else
                {
                    ClearMultiSelection();
                    SetSelection(coordinate);
                }
            }
        }

        private static bool IsPointerOverGui()
        {
            return CleanEnergy.UI.ImguiHitTest.IsPointerOverGui();
        }
    }
}
