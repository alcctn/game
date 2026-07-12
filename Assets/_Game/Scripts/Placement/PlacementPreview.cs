using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using UnityEngine;

namespace CleanEnergy.Placement
{
    /// <summary>
    /// Ghost preview mesh colored by validation state.
    /// </summary>
    public sealed class PlacementPreview : MonoBehaviour
    {
        private GameObject _ghost;
        private Renderer _renderer;
        private Material _material;

        public void Show(BuildingDefinition definition, Vector3 worldPosition, bool isValid, int rotation = 0)
        {
            EnsureGhost(definition);
            _ghost.SetActive(true);
            var half = EstimateHalfHeight(definition);
            _ghost.transform.position = new Vector3(worldPosition.x, worldPosition.y + half, worldPosition.z);
            _ghost.transform.rotation = Quaternion.Euler(0f, rotation * 90f, 0f);
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
            if (_ghost != null)
            {
                _ghost.SetActive(false);
            }
        }

        private void EnsureGhost(BuildingDefinition definition)
        {
            if (_ghost != null)
            {
                return;
            }

            _ghost = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _ghost.name = "PlacementGhost";
            _ghost.transform.SetParent(transform, false);
            _ghost.transform.localScale = new Vector3(1.2f, 0.25f, 1.2f);
            Object.Destroy(_ghost.GetComponent<Collider>());
            _renderer = _ghost.GetComponent<Renderer>();
            _material = new Material(Shader.Find("CleanEnergy/UnlitVertexColor")
                                     ?? Shader.Find("Sprites/Default")
                                     ?? Shader.Find("Universal Render Pipeline/Lit")
                                     ?? Shader.Find("Standard"));
            _renderer.sharedMaterial = _material;
        }

        private void OnDestroy()
        {
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
