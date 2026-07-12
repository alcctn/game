using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Economy;
using CleanEnergy.Grid;
using CleanEnergy.Scenario;
using CleanEnergy.Settlements;
using CleanEnergy.Workers;
using UnityEngine;
using UnityEngine.Rendering;

namespace CleanEnergy.Placement
{
    /// <summary>
    /// Shows all currently valid placement cells as translucent green quads.
    /// </summary>
    public sealed class PlacementValidityOverlay : MonoBehaviour
    {
        public const int MaxMarkers = 512;

        private MeshFilter _filter;
        private MeshRenderer _renderer;
        private Mesh _mesh;
        private Material _material;
        private int _lastValidCount = -1;

        public int LastValidCount => _lastValidCount;

        public void Rebuild(
            BuildingDefinition definition,
            GridService grid,
            PlacementValidator validator,
            GridOccupancyService occupancy,
            Wallet wallet,
            IBuildingUnlockQuery unlocks,
            int rotation,
            IActiveSettlementQuery settlement = null,
            IWorkerQuery workers = null,
            LevelDefinition level = null)
        {
            if (definition == null || grid == null || !grid.IsInitialized || validator == null)
            {
                Hide();
                return;
            }

            EnsureComponents();
            var positions = new List<Vector3>(128);
            for (var x = 0; x < grid.Width; x++)
            {
                for (var y = 0; y < grid.Height; y++)
                {
                    if (positions.Count >= MaxMarkers)
                    {
                        break;
                    }

                    var coord = new GridCoordinate(x, y);
                    var result = validator.Validate(
                        definition, coord, grid, occupancy, wallet, unlocks, rotation,
                        settlement, workers, level);
                    if (!result.IsValid || !grid.TryGetCell(coord, out var cell))
                    {
                        continue;
                    }

                    positions.Add(cell.WorldPosition);
                }

                if (positions.Count >= MaxMarkers)
                {
                    break;
                }
            }

            _lastValidCount = positions.Count;
            if (positions.Count == 0)
            {
                ClearMesh();
                if (_renderer != null)
                {
                    _renderer.enabled = false;
                }

                return;
            }

            BuildMesh(positions, grid.CellSize * 0.85f);
            if (_renderer != null)
            {
                _renderer.enabled = true;
            }
        }

        public void Hide()
        {
            _lastValidCount = -1;
            ClearMesh();
            if (_renderer != null)
            {
                _renderer.enabled = false;
            }
        }

        private void BuildMesh(List<Vector3> centers, float size)
        {
            var half = size * 0.5f;
            var vertCount = centers.Count * 4;
            var vertices = new Vector3[vertCount];
            var colors = new Color[vertCount];
            var triangles = new int[centers.Count * 6];
            var tint = new Color(0.15f, 0.9f, 0.35f, 0.35f);
            var vi = 0;
            var ti = 0;
            for (var i = 0; i < centers.Count; i++)
            {
                var c = centers[i];
                var y = c.y + 0.2f;
                vertices[vi] = new Vector3(c.x - half, y, c.z - half);
                vertices[vi + 1] = new Vector3(c.x + half, y, c.z - half);
                vertices[vi + 2] = new Vector3(c.x + half, y, c.z + half);
                vertices[vi + 3] = new Vector3(c.x - half, y, c.z + half);
                colors[vi] = colors[vi + 1] = colors[vi + 2] = colors[vi + 3] = tint;
                triangles[ti] = vi;
                triangles[ti + 1] = vi + 2;
                triangles[ti + 2] = vi + 1;
                triangles[ti + 3] = vi;
                triangles[ti + 4] = vi + 3;
                triangles[ti + 5] = vi + 2;
                vi += 4;
                ti += 6;
            }

            if (_mesh == null)
            {
                _mesh = new Mesh { name = "PlacementValidity" };
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
            _mesh.colors = colors;
            _mesh.triangles = triangles;
            _mesh.RecalculateBounds();
            _filter.sharedMesh = _mesh;
        }

        private void EnsureComponents()
        {
            if (_filter == null)
            {
                _filter = gameObject.GetComponent<MeshFilter>();
                if (_filter == null)
                {
                    _filter = gameObject.AddComponent<MeshFilter>();
                }
            }

            if (_renderer == null)
            {
                _renderer = gameObject.GetComponent<MeshRenderer>();
                if (_renderer == null)
                {
                    _renderer = gameObject.AddComponent<MeshRenderer>();
                }

                _renderer.shadowCastingMode = ShadowCastingMode.Off;
                _renderer.receiveShadows = false;
            }

            if (_material == null)
            {
                var shader = Shader.Find("CleanEnergy/UnlitVertexColor")
                             ?? Shader.Find("Sprites/Default")
                             ?? Shader.Find("Universal Render Pipeline/Unlit")
                             ?? Shader.Find("Universal Render Pipeline/Lit");
                _material = new Material(shader) { name = "PlacementValidityMat" };
                if (_material.HasProperty("_BaseColor"))
                {
                    _material.SetColor("_BaseColor", new Color(0.15f, 0.9f, 0.35f, 0.35f));
                }

                if (_material.HasProperty("_Color"))
                {
                    _material.SetColor("_Color", new Color(0.15f, 0.9f, 0.35f, 0.35f));
                }

                _renderer.sharedMaterial = _material;
            }
        }

        private void ClearMesh()
        {
            if (_filter != null)
            {
                _filter.sharedMesh = null;
            }

            if (_mesh != null)
            {
                _mesh.Clear();
            }
        }

        private void OnDestroy()
        {
            if (_mesh != null)
            {
                Destroy(_mesh);
            }

            if (_material != null)
            {
                Destroy(_material);
            }
        }
    }
}
