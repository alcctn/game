using System.Collections.Generic;
using CleanEnergy.Energy;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using UnityEngine;

namespace CleanEnergy.DebugTools
{
    /// <summary>
    /// Draws network edges as LineRenderers while Network debug mode is active.
    /// </summary>
    public sealed class NetworkEdgeOverlay : MonoBehaviour
    {
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private EnergyNetworkService networkService;
        [SerializeField] private EnergySimulationDriver energyDriver;
        [SerializeField] private float heightOffset = 0.75f;
        [SerializeField] private float lineWidth = 0.35f;

        private readonly List<LineRenderer> _lines = new List<LineRenderer>();
        private Material _lineMaterial;
        private bool _visible;

        public void Configure(
            MapGenerator generator,
            EnergyNetworkService network,
            EnergySimulationDriver driver)
        {
            mapGenerator = generator;
            networkService = network;
            energyDriver = driver;
        }

        private void OnDestroy()
        {
            ClearLines();
            if (_lineMaterial != null)
            {
                Destroy(_lineMaterial);
            }
        }

        public void SetVisible(bool visible)
        {
            _visible = visible;
            if (!visible)
            {
                ClearLines();
                return;
            }

            Rebuild();
        }

        public void Rebuild()
        {
            if (!_visible)
            {
                return;
            }

            ClearLines();
            if (networkService == null || mapGenerator == null || !mapGenerator.Grid.IsInitialized)
            {
                return;
            }

            var graph = networkService.Graph;
            EnsureMaterial();

            foreach (var edge in graph.EnumerateUndirectedEdges())
            {
                if (!graph.Nodes.TryGetValue(edge.A, out var nodeA)
                    || !graph.Nodes.TryGetValue(edge.B, out var nodeB))
                {
                    continue;
                }

                var util = ResolveUtilization(nodeA.Coordinate, nodeB.Coordinate);
                var color = NetworkUtilization.ColorForUtilization(util);
                CreateLine(WorldOf(nodeA.Coordinate), WorldOf(nodeB.Coordinate), color);
            }
        }

        public static float ResolveEdgeUtilization(
            float utilA,
            bool hasA,
            float utilB,
            bool hasB)
        {
            if (hasA && hasB)
            {
                return Mathf.Max(utilA, utilB);
            }

            if (hasA)
            {
                return utilA;
            }

            if (hasB)
            {
                return utilB;
            }

            return 0f;
        }

        private float ResolveUtilization(GridCoordinate a, GridCoordinate b)
        {
            float utilA = 0f;
            float utilB = 0f;
            var hasA = energyDriver != null && energyDriver.TryGetHubUtilization(a, out utilA);
            var hasB = energyDriver != null && energyDriver.TryGetHubUtilization(b, out utilB);
            return ResolveEdgeUtilization(utilA, hasA, utilB, hasB);
        }

        private Vector3 WorldOf(GridCoordinate coordinate)
        {
            if (mapGenerator.Grid.TryGetCell(coordinate, out var cell))
            {
                return cell.WorldPosition + Vector3.up * heightOffset;
            }

            return Vector3.zero;
        }

        private void CreateLine(Vector3 from, Vector3 to, Color color)
        {
            var go = new GameObject("NetworkEdge");
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = _lineMaterial;
            line.positionCount = 2;
            line.SetPosition(0, from);
            line.SetPosition(1, to);
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.startColor = color;
            line.endColor = color;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.useWorldSpace = true;
            _lines.Add(line);
        }

        private void EnsureMaterial()
        {
            if (_lineMaterial != null)
            {
                return;
            }

            var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
            _lineMaterial = new Material(shader);
        }

        private void ClearLines()
        {
            for (var i = 0; i < _lines.Count; i++)
            {
                if (_lines[i] != null)
                {
                    Destroy(_lines[i].gameObject);
                }
            }

            _lines.Clear();
        }
    }
}
