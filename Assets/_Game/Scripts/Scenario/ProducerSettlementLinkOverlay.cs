using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using CleanEnergy.Settlements;
using UnityEngine;
using UnityEngine.Rendering;

namespace CleanEnergy.Scenario
{
    /// <summary>
    /// Persistent per-producer colored lines to the active settlement until cleared.
    /// </summary>
    public sealed class ProducerSettlementLinkOverlay : MonoBehaviour
    {
        private readonly Dictionary<string, LineRenderer> _lines =
            new Dictionary<string, LineRenderer>();

        private MapGenerator _map;
        private IActiveSettlementQuery _settlement;
        private Material _sharedMaterial;
        private bool _visible = true;

        public void Configure(MapGenerator map, IActiveSettlementQuery settlement)
        {
            _map = map;
            _settlement = settlement;
        }

        public void SetVisible(bool visible)
        {
            _visible = visible;
            foreach (var pair in _lines)
            {
                if (pair.Value != null)
                {
                    pair.Value.enabled = visible;
                }
            }
        }

        public void UpsertProducerLink(BuildingInstance instance)
        {
            if (!_visible
                || instance?.Definition == null
                || !instance.Definition.IsProducer
                || _settlement == null
                || !_settlement.HasActiveSettlement
                || _map == null
                || !_map.Grid.IsInitialized)
            {
                return;
            }

            if (!_map.Grid.TryGetCell(instance.Coordinate, out var fromCell)
                || !_map.Grid.TryGetCell(_settlement.Coordinate, out var toCell))
            {
                return;
            }

            var line = EnsureLine(instance.InstanceId);
            var color = instance.Definition.GizmoColor;
            color.a = 0.85f;
            line.startColor = color;
            line.endColor = color;
            line.positionCount = 2;
            line.SetPosition(0, fromCell.WorldPosition + Vector3.up * 0.5f);
            line.SetPosition(1, toCell.WorldPosition + Vector3.up * 0.5f);
            line.enabled = true;
        }

        public void RemoveLink(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId) || !_lines.TryGetValue(instanceId, out var line))
            {
                return;
            }

            _lines.Remove(instanceId);
            if (line != null)
            {
                Destroy(line.gameObject);
            }
        }

        public void ClearAll()
        {
            foreach (var pair in _lines)
            {
                if (pair.Value != null)
                {
                    Destroy(pair.Value.gameObject);
                }
            }

            _lines.Clear();
        }

        private LineRenderer EnsureLine(string instanceId)
        {
            if (_lines.TryGetValue(instanceId, out var existing) && existing != null)
            {
                return existing;
            }

            var go = new GameObject("Link_" + instanceId);
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.widthMultiplier = 0.18f;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.material = SharedMaterial();
            line.useWorldSpace = true;
            _lines[instanceId] = line;
            return line;
        }

        private Material SharedMaterial()
        {
            if (_sharedMaterial != null)
            {
                return _sharedMaterial;
            }

            var shader = Shader.Find("Sprites/Default")
                         ?? Shader.Find("Universal Render Pipeline/Unlit")
                         ?? Shader.Find("Unlit/Color");
            _sharedMaterial = new Material(shader) { name = "ProducerSettlementLinkMat" };
            return _sharedMaterial;
        }

        private void OnDestroy()
        {
            ClearAll();
            if (_sharedMaterial != null)
            {
                Destroy(_sharedMaterial);
            }
        }
    }
}
