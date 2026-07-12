using CleanEnergy.Core;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using UnityEngine;

namespace CleanEnergy.Art
{
    /// <summary>
    /// Spawns Pure Poly nature props after map generation (visual only; sim unchanged).
    /// </summary>
    public sealed class NatureVisualSpawner : MonoBehaviour
    {
        public const string NatureRootName = "NatureRoot";

        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private PurePolyCatalog catalog;
        [SerializeField] private Transform natureRoot;
        [SerializeField] private int maxInstances = NatureSpawnMath.MaxInstances;
        [SerializeField] private float propScale = 0.35f;

        private int _spawnedCount;

        public int SpawnedCount => _spawnedCount;
        public int MaxInstances => maxInstances;
        public PurePolyCatalog Catalog => catalog;
        public Transform NatureRoot => natureRoot;

        public void Configure(MapGenerator generator, PurePolyCatalog purePolyCatalog, Transform root = null)
        {
            if (mapGenerator != null)
            {
                mapGenerator.Events.MapGenerated -= OnMapGenerated;
            }

            mapGenerator = generator;
            catalog = purePolyCatalog;
            if (root != null)
            {
                natureRoot = root;
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
        }

        private void OnMapGenerated(MapGeneratedEvent evt)
        {
            Rebuild(evt?.Seed ?? mapGenerator?.Settings?.Seed ?? "0");
        }

        /// <summary>Clears NatureRoot and respawns from current grid + catalog.</summary>
        public void Rebuild(string seed)
        {
            EnsureNatureRoot();
            ClearNatureRoot();
            _spawnedCount = 0;

            if (catalog == null || mapGenerator?.Grid == null || !mapGenerator.Grid.IsInitialized)
            {
                return;
            }

            NatureSpawnMath.AllocateBudget(
                maxInstances,
                out var plainsCap,
                out var forestCap,
                out var hillsCap,
                out var lakeCap,
                out var bridgeCap);

            var plainsUsed = 0;
            var forestUsed = 0;
            var hillsUsed = 0;
            var lakeUsed = 0;
            var bridgeUsed = 0;

            var grid = mapGenerator.Grid;
            var width = grid.Width;
            var height = grid.Height;

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if (_spawnedCount >= maxInstances)
                    {
                        return;
                    }

                    var coord = new GridCoordinate(x, y);
                    if (!grid.TryGetCell(coord, out var cell))
                    {
                        continue;
                    }

                    switch (cell.Biome)
                    {
                        case BiomeType.Plains:
                            if (plainsUsed < plainsCap
                                && NatureSpawnMath.Roll(seed, x, y, NatureSpawnMath.PlainsDensity)
                                && TrySpawnPlains(cell, NatureSpawnMath.Hash(seed, x, y)))
                            {
                                plainsUsed++;
                            }

                            break;
                        case BiomeType.Forest:
                            if (forestUsed < forestCap
                                && NatureSpawnMath.Roll(seed, x, y, NatureSpawnMath.ForestDensity)
                                && TrySpawnForest(cell, NatureSpawnMath.Hash(seed, x, y)))
                            {
                                forestUsed++;
                            }

                            break;
                        case BiomeType.Hills:
                            if (hillsUsed < hillsCap
                                && NatureSpawnMath.Roll(seed, x, y, NatureSpawnMath.HillsDensity)
                                && TrySpawnHills(cell, NatureSpawnMath.Hash(seed, x, y), preferMountain: false))
                            {
                                hillsUsed++;
                            }

                            break;
                        case BiomeType.Ridge:
                            if (hillsUsed < hillsCap
                                && NatureSpawnMath.Roll(seed, x, y, NatureSpawnMath.HillsDensity)
                                && TrySpawnHills(cell, NatureSpawnMath.Hash(seed, x, y), preferMountain: true))
                            {
                                hillsUsed++;
                            }

                            break;
                        case BiomeType.Lake:
                            if (lakeUsed < lakeCap
                                && NatureSpawnMath.Roll(seed, x, y, 0.15f)
                                && TrySpawnPrefab(catalog.LakeGround, cell, NatureSpawnMath.Hash(seed, x, y)))
                            {
                                lakeUsed++;
                            }

                            break;
                        case BiomeType.River:
                            if (bridgeUsed < bridgeCap
                                && IsRiverCorridor(grid, coord)
                                && NatureSpawnMath.Roll(seed, x, y, 0.04f)
                                && TrySpawnPrefab(
                                    PurePolyCatalog.Pick(catalog.Bridges, NatureSpawnMath.Hash(seed, x, y)),
                                    cell,
                                    NatureSpawnMath.Hash(seed, x, y)))
                            {
                                bridgeUsed++;
                            }

                            break;
                    }
                }
            }
        }

        public void ClearNatureRoot()
        {
            EnsureNatureRoot();
            for (var i = natureRoot.childCount - 1; i >= 0; i--)
            {
                var child = natureRoot.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }

            _spawnedCount = 0;
        }

        private bool TrySpawnPlains(GridCellData cell, int hash)
        {
            var pick = PurePolyCatalog.Pick(catalog.Grasses, hash)
                       ?? PurePolyCatalog.Pick(catalog.Flowers, hash)
                       ?? catalog.Meadow;
            return TrySpawnPrefab(pick, cell, hash);
        }

        private bool TrySpawnForest(GridCellData cell, int hash)
        {
            var pick = PurePolyCatalog.Pick(catalog.ForestTrees, hash);
            return TrySpawnPrefab(pick, cell, hash);
        }

        private bool TrySpawnHills(GridCellData cell, int hash, bool preferMountain)
        {
            GameObject pick;
            if (preferMountain)
            {
                pick = catalog.Mountain ?? PurePolyCatalog.Pick(catalog.Rocks, hash);
            }
            else
            {
                pick = PurePolyCatalog.Pick(catalog.Rocks, hash) ?? catalog.Mountain;
            }

            return TrySpawnPrefab(pick, cell, hash);
        }

        private bool TrySpawnPrefab(GameObject prefab, GridCellData cell, int hash)
        {
            if (prefab == null || natureRoot == null || _spawnedCount >= maxInstances)
            {
                return false;
            }

            var instance = Instantiate(prefab, natureRoot);
            instance.name = prefab.name;
            instance.transform.position = cell.WorldPosition;
            var yaw = (hash & 360);
            instance.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            instance.transform.localScale = Vector3.one * propScale;
            DisableColliders(instance);
            _spawnedCount++;
            return true;
        }

        /// <summary>Disables all colliders so nature props never block placement rays.</summary>
        public static void DisableColliders(GameObject go)
        {
            if (go == null)
            {
                return;
            }

            var colliders = go.GetComponentsInChildren<Collider>(true);
            for (var i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }
        }

        private static bool IsRiverCorridor(GridService grid, GridCoordinate coord)
        {
            // Soft: at least one orthogonal neighbour is also River.
            var neighbours = new[]
            {
                new GridCoordinate(coord.X + 1, coord.Y),
                new GridCoordinate(coord.X - 1, coord.Y),
                new GridCoordinate(coord.X, coord.Y + 1),
                new GridCoordinate(coord.X, coord.Y - 1)
            };

            for (var i = 0; i < neighbours.Length; i++)
            {
                if (grid.TryGetCell(neighbours[i], out var n) && n.Biome == BiomeType.River)
                {
                    return true;
                }
            }

            return false;
        }

        private void EnsureNatureRoot()
        {
            if (natureRoot != null)
            {
                return;
            }

            var existing = transform.Find(NatureRootName);
            if (existing != null)
            {
                natureRoot = existing;
                return;
            }

            var go = new GameObject(NatureRootName);
            go.transform.SetParent(transform, false);
            natureRoot = go.transform;
        }
    }
}
