using CleanEnergy.Grid;
using UnityEngine;

namespace CleanEnergy.Buildings
{
    /// <summary>
    /// Creates building GameObjects from definitions (prefab or procedural primitive).
    /// </summary>
    public sealed class BuildingFactory
    {
        private int _nextId = 1;

        public int PeekNextId() => _nextId;

        public void SetNextId(int nextId)
        {
            _nextId = Mathf.Max(1, nextId);
        }

        public void ResetIds()
        {
            _nextId = 1;
        }

        public BuildingInstance Create(
            BuildingDefinition definition,
            GridCoordinate coordinate,
            GridService grid,
            Transform parent,
            int rotation = 0)
        {
            if (definition == null)
            {
                Debug.LogError("[BuildingFactory] Definition is null.");
                return null;
            }

            if (!grid.TryGetCell(coordinate, out var cell))
            {
                Debug.LogError($"[BuildingFactory] Invalid coordinate {coordinate}.");
                return null;
            }

            GameObject go;
            if (definition.Prefab != null)
            {
                go = Object.Instantiate(definition.Prefab, parent);
            }
            else
            {
                go = CreatePrimitive(definition);
                if (parent != null)
                {
                    go.transform.SetParent(parent, false);
                }
            }

            go.name = $"{definition.Id}_{coordinate.X}_{coordinate.Y}";
            var position = cell.WorldPosition;
            var halfHeight = definition.Prefab == null ? EstimateHalfHeight(definition) : 0f;
            go.transform.position = new Vector3(position.x, position.y + halfHeight, position.z);
            go.transform.rotation = Quaternion.Euler(0f, rotation * 90f, 0f);
            AttachRotatingVisual(go, definition);

            var instanceId = $"{definition.Id}_{_nextId++}";
            return new BuildingInstance(instanceId, definition, coordinate, rotation, go);
        }

        private static void AttachRotatingVisual(GameObject go, BuildingDefinition definition)
        {
            var rpm = RotatingVisual.ResolveRpmForBuildingId(definition.Id);
            if (rpm <= 0f)
            {
                return;
            }

            var rotating = go.GetComponent<RotatingVisual>();
            if (rotating == null)
            {
                rotating = go.AddComponent<RotatingVisual>();
            }

            rotating.Configure(rpm);
        }

        private static GameObject CreatePrimitive(BuildingDefinition definition)
        {
            PrimitiveType type;
            Vector3 scale;
            switch (definition.Id)
            {
                case "water_wheel":
                    type = PrimitiveType.Cylinder;
                    scale = new Vector3(0.9f, 0.35f, 0.9f);
                    break;
                case "small_hydro":
                    type = PrimitiveType.Cylinder;
                    scale = new Vector3(1.0f, 0.55f, 1.0f);
                    break;
                case "small_solar":
                    type = PrimitiveType.Cube;
                    scale = new Vector3(2.2f, 0.12f, 1.0f);
                    break;
                case "small_wind":
                    type = PrimitiveType.Cylinder;
                    scale = new Vector3(0.25f, 1.4f, 0.25f);
                    break;
                case "village":
                    type = PrimitiveType.Cube;
                    scale = new Vector3(1.6f, 0.8f, 1.6f);
                    break;
                case "battery":
                    type = PrimitiveType.Cube;
                    scale = new Vector3(0.9f, 0.9f, 0.9f);
                    break;
                case "maintenance_depot":
                    type = PrimitiveType.Cylinder;
                    scale = new Vector3(1.1f, 0.55f, 1.1f);
                    break;
                case "power_line":
                    type = PrimitiveType.Cylinder;
                    scale = new Vector3(0.15f, 1.1f, 0.15f);
                    break;
                case "distribution_hub":
                    type = PrimitiveType.Cube;
                    scale = new Vector3(1.2f, 0.7f, 1.2f);
                    break;
                default:
                    type = PrimitiveType.Cube;
                    scale = Vector3.one * 0.8f;
                    break;
            }

            var go = GameObject.CreatePrimitive(type);
            go.transform.localScale = scale;
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit")
                                                ?? Shader.Find("Standard"))
                {
                    color = definition.GizmoColor
                };
            }

            return go;
        }

        private static float EstimateHalfHeight(BuildingDefinition definition)
        {
            switch (definition.Id)
            {
                case "small_wind": return 1.4f;
                case "water_wheel": return 0.4f;
                case "small_solar": return 0.15f;
                default: return 0.4f;
            }
        }
    }
}
