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
            var halfHeight = EstimateHalfHeight(definition);
            go.transform.position = new Vector3(position.x, position.y + halfHeight, position.z);
            go.transform.rotation = Quaternion.Euler(0f, rotation * 90f, 0f);

            var instanceId = $"{definition.Id}_{_nextId++}";
            return new BuildingInstance(instanceId, definition, coordinate, rotation, go);
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
                case "small_solar":
                    type = PrimitiveType.Cube;
                    scale = new Vector3(1.4f, 0.12f, 1.0f);
                    break;
                case "small_wind":
                    type = PrimitiveType.Cylinder;
                    scale = new Vector3(0.25f, 1.4f, 0.25f);
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
