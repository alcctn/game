using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using UnityEngine;

namespace CleanEnergy.Placement
{
    /// <summary>
    /// Ghost preview mesh colored by validation state. Shows every footprint cell.
    /// </summary>
    public sealed class PlacementPreview : MonoBehaviour
    {
        private readonly List<GameObject> _ghosts = new List<GameObject>();
        private Material _material;
        private string _ghostDefinitionId;
        private Vector2Int _ghostSize;

        public void Show(
            BuildingDefinition definition,
            GridCoordinate anchor,
            GridService grid,
            bool isValid,
            int rotation = 0)
        {
            if (definition == null || grid == null || !grid.IsInitialized)
            {
                Hide();
                return;
            }

            var size = BuildingFootprint.GetFootprintSize(definition, rotation);
            var cells = BuildingFootprint.GetCells(anchor, size);
            EnsureGhosts(definition, size, cells.Count);

            var color = isValid
                ? new Color(0.2f, 0.85f, 0.35f, 0.55f)
                : new Color(0.9f, 0.2f, 0.15f, 0.55f);
            if (_material != null)
            {
                _material.color = color;
            }

            var half = EstimateHalfHeight(definition);
            var yaw = Quaternion.Euler(0f, rotation * 90f, 0f);
            for (var i = 0; i < _ghosts.Count; i++)
            {
                var ghost = _ghosts[i];
                if (i >= cells.Count || !grid.TryGetCell(cells[i], out var cell))
                {
                    ghost.SetActive(false);
                    continue;
                }

                ghost.SetActive(true);
                ghost.transform.position = new Vector3(
                    cell.WorldPosition.x, cell.WorldPosition.y + half, cell.WorldPosition.z);
                ghost.transform.rotation = yaw;
            }
        }

        /// <summary>Legacy single-cell preview (anchor world position).</summary>
        public void Show(BuildingDefinition definition, Vector3 worldPosition, bool isValid, int rotation = 0)
        {
            EnsureGhosts(definition, Vector2Int.one, 1);
            var ghost = _ghosts[0];
            ghost.SetActive(true);
            for (var i = 1; i < _ghosts.Count; i++)
            {
                _ghosts[i].SetActive(false);
            }

            var half = EstimateHalfHeight(definition);
            ghost.transform.position = new Vector3(worldPosition.x, worldPosition.y + half, worldPosition.z);
            ghost.transform.rotation = Quaternion.Euler(0f, rotation * 90f, 0f);
            var color = isValid
                ? new Color(0.2f, 0.85f, 0.35f, 0.55f)
                : new Color(0.9f, 0.2f, 0.15f, 0.55f);
            if (_material != null)
            {
                _material.color = color;
            }
        }

        public void Hide()
        {
            for (var i = 0; i < _ghosts.Count; i++)
            {
                if (_ghosts[i] != null)
                {
                    _ghosts[i].SetActive(false);
                }
            }
        }

        private void EnsureGhosts(BuildingDefinition definition, Vector2Int size, int count)
        {
            var definitionId = definition != null ? definition.Id : null;
            if (_ghosts.Count == count
                && _ghostDefinitionId == definitionId
                && _ghostSize == size
                && _material != null)
            {
                return;
            }

            ClearGhosts();
            _ghostDefinitionId = definitionId;
            _ghostSize = size;
            if (_material == null)
            {
                _material = new Material(Shader.Find("CleanEnergy/UnlitVertexColor")
                                         ?? Shader.Find("Sprites/Default")
                                         ?? Shader.Find("Universal Render Pipeline/Lit")
                                         ?? Shader.Find("Standard"));
            }

            for (var i = 0; i < count; i++)
            {
                var ghost = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ghost.name = $"PlacementGhost_{i}";
                ghost.transform.SetParent(transform, false);
                ghost.transform.localScale = new Vector3(1.2f, 0.25f, 1.2f);
                Object.Destroy(ghost.GetComponent<Collider>());
                var renderer = ghost.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = _material;
                }

                _ghosts.Add(ghost);
            }
        }

        private void ClearGhosts()
        {
            for (var i = 0; i < _ghosts.Count; i++)
            {
                if (_ghosts[i] != null)
                {
                    Destroy(_ghosts[i]);
                }
            }

            _ghosts.Clear();
        }

        private void OnDestroy()
        {
            ClearGhosts();
            if (_material != null)
            {
                Destroy(_material);
            }
        }

        private static float EstimateHalfHeight(BuildingDefinition definition)
        {
            if (definition == null)
            {
                return 0.3f;
            }

            switch (definition.Id)
            {
                case "small_wind": return 0.5f;
                case "water_wheel": return 0.35f;
                default: return 0.25f;
            }
        }
    }
}
