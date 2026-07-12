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
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private PlacementController placementController;
        [SerializeField] private float overlayHeightOffset = 0.5f;
        [SerializeField] private float maxSlopeForColor = 45f;
        [SerializeField] private float maxWaterFlowForColor = 64f;

        private DebugViewMode _mode = DebugViewMode.Normal;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private Mesh _mesh;
        private Material _material;
        private GridCoordinate? _selectedCell;

        public DebugViewMode Mode => _mode;
        public GridCoordinate? SelectedCell => _selectedCell;

        public event System.Action<DebugViewMode> ModeChanged;
        public event System.Action<GridCoordinate?> SelectionChanged;

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

            if (Input.GetMouseButtonDown(0) && !IsPointerOverGui())
            {
                TrySelectCellUnderCursor();
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

        public void SetMode(DebugViewMode mode)
        {
            if (_mode == mode)
            {
                return;
            }

            _mode = mode;
            Rebuild();
            ModeChanged?.Invoke(_mode);
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
        }

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
                SetSelection(coordinate);
            }
        }

        private static bool IsPointerOverGui()
        {
            return false;
        }
    }
}
