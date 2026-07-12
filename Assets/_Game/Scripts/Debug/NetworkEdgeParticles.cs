using System.Collections.Generic;
using CleanEnergy.Energy;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using UnityEngine;

namespace CleanEnergy.DebugTools
{
    /// <summary>
    /// Simple particles along network edges while Network debug (F7) is visible.
    /// </summary>
    public sealed class NetworkEdgeParticles : MonoBehaviour
    {
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private EnergyNetworkService networkService;
        [SerializeField] private EnergySimulationDriver energyDriver;
        [SerializeField] private float heightOffset = 0.9f;
        [SerializeField] private float particleSpeed = 1f;

        private readonly List<ParticleSystem> _systems = new List<ParticleSystem>();
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
            Clear();
        }

        public void SetVisible(bool visible)
        {
            _visible = visible;
            if (!visible)
            {
                Clear();
                enabled = false;
                return;
            }

            enabled = true;
            Rebuild();
        }

        public void Rebuild()
        {
            if (!_visible)
            {
                return;
            }

            Clear();
            if (networkService == null || mapGenerator == null || !mapGenerator.Grid.IsInitialized)
            {
                return;
            }

            var graph = networkService.Graph;
            var edgeIndex = 0;
            foreach (var edge in graph.EnumerateUndirectedEdges())
            {
                if (edgeIndex >= EdgeParticleMath.MaxEdges)
                {
                    break;
                }

                if (!graph.Nodes.TryGetValue(edge.A, out var nodeA)
                    || !graph.Nodes.TryGetValue(edge.B, out var nodeB))
                {
                    continue;
                }

                var util = ResolveUtilization(nodeA.Coordinate, nodeB.Coordinate);
                var count = EdgeParticleMath.ParticlesForUtilization(util, particleSpeed);
                if (count <= 0)
                {
                    continue;
                }

                CreateEdgeParticles(
                    WorldOf(nodeA.Coordinate),
                    WorldOf(nodeB.Coordinate),
                    count,
                    util);
                edgeIndex++;
            }
        }

        private float ResolveUtilization(GridCoordinate a, GridCoordinate b)
        {
            float utilA = 0f;
            float utilB = 0f;
            var hasA = energyDriver != null && energyDriver.TryGetHubUtilization(a, out utilA);
            var hasB = energyDriver != null && energyDriver.TryGetHubUtilization(b, out utilB);
            return NetworkEdgeOverlay.ResolveEdgeUtilization(utilA, hasA, utilB, hasB);
        }

        private Vector3 WorldOf(GridCoordinate coordinate)
        {
            if (mapGenerator.Grid.TryGetCell(coordinate, out var cell))
            {
                return cell.WorldPosition + Vector3.up * heightOffset;
            }

            return Vector3.zero;
        }

        private void CreateEdgeParticles(Vector3 from, Vector3 to, int count, float util)
        {
            var go = new GameObject("EdgeParticles");
            go.transform.SetParent(transform, false);
            go.transform.position = from;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1.2f;
            main.startSize = 0.2f;
            main.startColor = NetworkUtilization.ColorForUtilization(util);
            main.maxParticles = EdgeParticleMath.MaxParticlesPerEdge;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.loop = true;

            var emission = ps.emission;
            emission.rateOverTime = count * particleSpeed;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            var length = Mathf.Max(0.1f, (to - from).magnitude);
            shape.scale = new Vector3(0.15f, 0.15f, length);
            go.transform.position = (from + to) * 0.5f;
            go.transform.rotation = Quaternion.LookRotation((to - from).normalized, Vector3.up);

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.z = new ParticleSystem.MinMaxCurve(particleSpeed * (0.5f + util));

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = new Material(
                    Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color"));
            }

            _systems.Add(ps);
        }

        private void Clear()
        {
            for (var i = 0; i < _systems.Count; i++)
            {
                if (_systems[i] != null)
                {
                    Destroy(_systems[i].gameObject);
                }
            }

            _systems.Clear();
        }
    }
}
