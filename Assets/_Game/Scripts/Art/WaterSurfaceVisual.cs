using CleanEnergy.Core;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using UnityEngine;
using UnityEngine.Rendering;

namespace CleanEnergy.Art
{
    /// <summary>
    /// Builds a combined water surface mesh over River / Lake cells.
    /// </summary>
    public sealed class WaterSurfaceVisual : MonoBehaviour
    {
        public const string WaterRootName = "WaterRoot";
        public const float HeightOffset = 0.12f;
        public const float QuadScale = 0.95f;

        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private TerrainArtCatalog catalog;
        [SerializeField] private Transform waterRoot;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private Mesh _mesh;
        private int _quadCount;

        public int QuadCount => _quadCount;
        public Transform WaterRoot => waterRoot;

        public void Configure(MapGenerator generator, TerrainArtCatalog artCatalog, Transform root = null)
        {
            if (mapGenerator != null)
            {
                mapGenerator.Events.MapGenerated -= OnMapGenerated;
            }

            mapGenerator = generator;
            catalog = artCatalog;
            if (root != null)
            {
                waterRoot = root;
            }

            if (mapGenerator != null)
            {
                mapGenerator.Events.MapGenerated += OnMapGenerated;
            }
        }

        private void Awake()
        {
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

            if (_mesh != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(_mesh);
                }
                else
                {
                    DestroyImmediate(_mesh);
                }
            }
        }

        private void OnMapGenerated(MapGeneratedEvent evt)
        {
            Rebuild();
        }

        public void Rebuild()
        {
            EnsureWaterRoot();
            _quadCount = 0;

            if (mapGenerator?.Grid == null || !mapGenerator.Grid.IsInitialized)
            {
                ClearMesh();
                return;
            }

            var grid = mapGenerator.Grid;
            var waterCells = CountWaterCells(grid);
            if (waterCells == 0)
            {
                ClearMesh();
                return;
            }

            EnsureMeshComponents();
            BuildCombinedMesh(grid, waterCells);
            ApplyMaterial();
        }

        public static int CountWaterCells(GridService grid)
        {
            if (grid == null || !grid.IsInitialized)
            {
                return 0;
            }

            var n = 0;
            for (var x = 0; x < grid.Width; x++)
            {
                for (var y = 0; y < grid.Height; y++)
                {
                    if (!grid.TryGetCell(new GridCoordinate(x, y), out var cell))
                    {
                        continue;
                    }

                    if (cell.Biome == BiomeType.River || cell.Biome == BiomeType.Lake)
                    {
                        n++;
                    }
                }
            }

            return n;
        }

        private void BuildCombinedMesh(GridService grid, int waterCells)
        {
            var vertCount = waterCells * 4;
            var triCount = waterCells * 6;
            var vertices = new Vector3[vertCount];
            var normals = new Vector3[vertCount];
            var uvs = new Vector2[vertCount];
            var triangles = new int[triCount];
            var half = grid.CellSize * QuadScale * 0.5f;
            var vi = 0;
            var ti = 0;

            for (var x = 0; x < grid.Width; x++)
            {
                for (var y = 0; y < grid.Height; y++)
                {
                    if (!grid.TryGetCell(new GridCoordinate(x, y), out var cell))
                    {
                        continue;
                    }

                    if (cell.Biome != BiomeType.River && cell.Biome != BiomeType.Lake)
                    {
                        continue;
                    }

                    var c = cell.WorldPosition;
                    var yPos = c.y + HeightOffset;
                    vertices[vi] = new Vector3(c.x - half, yPos, c.z - half);
                    vertices[vi + 1] = new Vector3(c.x + half, yPos, c.z - half);
                    vertices[vi + 2] = new Vector3(c.x + half, yPos, c.z + half);
                    vertices[vi + 3] = new Vector3(c.x - half, yPos, c.z + half);
                    normals[vi] = normals[vi + 1] = normals[vi + 2] = normals[vi + 3] = Vector3.up;
                    uvs[vi] = new Vector2(0f, 0f);
                    uvs[vi + 1] = new Vector2(1f, 0f);
                    uvs[vi + 2] = new Vector2(1f, 1f);
                    uvs[vi + 3] = new Vector2(0f, 1f);
                    triangles[ti] = vi;
                    triangles[ti + 1] = vi + 2;
                    triangles[ti + 2] = vi + 1;
                    triangles[ti + 3] = vi;
                    triangles[ti + 4] = vi + 3;
                    triangles[ti + 5] = vi + 2;
                    vi += 4;
                    ti += 6;
                    _quadCount++;
                }
            }

            if (_mesh == null)
            {
                _mesh = new Mesh { name = "WaterSurface" };
            }
            else
            {
                _mesh.Clear();
            }

            if (vertCount > 65000)
            {
                _mesh.indexFormat = IndexFormat.UInt32;
            }

            _mesh.vertices = vertices;
            _mesh.normals = normals;
            _mesh.uv = uvs;
            _mesh.triangles = triangles;
            _mesh.RecalculateBounds();
            _meshFilter.sharedMesh = _mesh;
        }

        private void ApplyMaterial()
        {
            if (_meshRenderer == null)
            {
                return;
            }

            var mat = catalog != null ? catalog.WaterMaterial : null;
            if (mat == null)
            {
                mat = CreateFallbackWaterMaterial();
            }

            _meshRenderer.sharedMaterial = mat;
            _meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            _meshRenderer.receiveShadows = false;
        }

        private static Material CreateFallbackWaterMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit")
                         ?? Shader.Find("Standard");
            var mat = new Material(shader) { name = "FallbackWater" };
            var blue = new Color(0.2f, 0.45f, 0.75f, 0.65f);
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", blue);
            }

            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", blue);
            }

            if (mat.HasProperty("_Smoothness"))
            {
                mat.SetFloat("_Smoothness", 0.35f);
            }

            return mat;
        }

        private void EnsureWaterRoot()
        {
            if (waterRoot != null)
            {
                return;
            }

            var existing = transform.Find(WaterRootName);
            if (existing != null)
            {
                waterRoot = existing;
                return;
            }

            var go = new GameObject(WaterRootName);
            go.transform.SetParent(transform, false);
            waterRoot = go.transform;
        }

        private void EnsureMeshComponents()
        {
            EnsureWaterRoot();
            _meshFilter = waterRoot.GetComponent<MeshFilter>();
            if (_meshFilter == null)
            {
                _meshFilter = waterRoot.gameObject.AddComponent<MeshFilter>();
            }

            _meshRenderer = waterRoot.GetComponent<MeshRenderer>();
            if (_meshRenderer == null)
            {
                _meshRenderer = waterRoot.gameObject.AddComponent<MeshRenderer>();
            }

            var col = waterRoot.GetComponent<Collider>();
            if (col != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(col);
                }
                else
                {
                    DestroyImmediate(col);
                }
            }
        }

        private void ClearMesh()
        {
            if (_meshFilter != null)
            {
                _meshFilter.sharedMesh = null;
            }

            if (_mesh != null)
            {
                _mesh.Clear();
            }
        }
    }
}
