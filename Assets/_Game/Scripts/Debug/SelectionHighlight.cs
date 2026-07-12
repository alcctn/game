using CleanEnergy.Grid;
using CleanEnergy.Map;
using UnityEngine;

namespace CleanEnergy.DebugTools
{
    /// <summary>
    /// Pure helpers for selection highlight placement.
    /// </summary>
    public static class SelectionHighlightGeometry
    {
        public static readonly Color HighlightColor = new Color(1f, 0.75f, 0.15f, 0.55f);
        public const float ExtraHeightOffset = 0.15f;

        public static Vector3 GetWorldCenter(GridCellData cell, float baseOverlayHeightOffset)
        {
            if (cell == null)
            {
                return Vector3.zero;
            }

            return cell.WorldPosition + Vector3.up * (baseOverlayHeightOffset + ExtraHeightOffset);
        }

        public static float GetHalfExtent(float cellSize)
        {
            return Mathf.Max(0.01f, cellSize * 0.48f);
        }
    }

    /// <summary>
    /// Single-cell amber quad shown for the current map selection.
    /// </summary>
    public sealed class SelectionHighlight : MonoBehaviour
    {
        [SerializeField] private MapDebugOverlay debugOverlay;
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private float baseOverlayHeightOffset = 0.5f;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private Mesh _mesh;
        private Material _material;
        private GridCoordinate? _coordinate;

        public GridCoordinate? Coordinate => _coordinate;
        public bool IsVisible => _meshRenderer != null && _meshRenderer.enabled;

        public void Configure(MapDebugOverlay overlay, MapGenerator generator, float overlayHeightOffset = 0.5f)
        {
            Unsubscribe();
            debugOverlay = overlay;
            mapGenerator = generator;
            baseOverlayHeightOffset = overlayHeightOffset;
            Subscribe();
            RefreshFromOverlay();
        }

        private void OnEnable() => Subscribe();

        private void OnDisable() => Unsubscribe();

        private void OnDestroy()
        {
            Unsubscribe();
            if (_material != null)
            {
                Destroy(_material);
            }
        }

        private void Subscribe()
        {
            if (debugOverlay != null)
            {
                debugOverlay.SelectionChanged += OnSelectionChanged;
            }
        }

        private void Unsubscribe()
        {
            if (debugOverlay != null)
            {
                debugOverlay.SelectionChanged -= OnSelectionChanged;
            }
        }

        private void OnSelectionChanged(GridCoordinate? coordinate)
        {
            SetSelection(coordinate);
        }

        private void RefreshFromOverlay()
        {
            SetSelection(debugOverlay != null ? debugOverlay.SelectedCell : null);
        }

        /// <summary>
        /// Applies selection for tests and event handlers.
        /// </summary>
        public void SetSelection(GridCoordinate? coordinate)
        {
            _coordinate = coordinate;
            Rebuild();
        }

        public void Clear()
        {
            SetSelection(null);
        }

        private void Rebuild()
        {
            EnsureComponents();

            if (!_coordinate.HasValue
                || mapGenerator == null
                || !mapGenerator.Grid.IsInitialized
                || !mapGenerator.Grid.TryGetCell(_coordinate.Value, out var cell))
            {
                _mesh.Clear();
                _meshRenderer.enabled = false;
                return;
            }

            var center = SelectionHighlightGeometry.GetWorldCenter(cell, baseOverlayHeightOffset);
            var half = SelectionHighlightGeometry.GetHalfExtent(mapGenerator.Grid.CellSize);
            var color = SelectionHighlightGeometry.HighlightColor;

            var vertices = new[]
            {
                center + new Vector3(-half, 0f, -half),
                center + new Vector3(-half, 0f, half),
                center + new Vector3(half, 0f, half),
                center + new Vector3(half, 0f, -half)
            };
            var colors = new[] { color, color, color, color };
            var triangles = new[] { 0, 1, 2, 0, 2, 3 };

            _mesh.Clear();
            _mesh.vertices = vertices;
            _mesh.colors = colors;
            _mesh.triangles = triangles;
            _mesh.RecalculateBounds();
            _meshRenderer.enabled = true;
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
                _mesh = new Mesh { name = "SelectionHighlightMesh" };
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
                    _material.SetColor("_Color", SelectionHighlightGeometry.HighlightColor);
                }
            }

            _meshRenderer.sharedMaterial = _material;
            _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _meshRenderer.receiveShadows = false;
        }
    }
}
