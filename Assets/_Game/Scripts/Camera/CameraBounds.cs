using UnityEngine;

namespace CleanEnergy.CameraSystem
{
    /// <summary>
    /// Axis-aligned world bounds used to clamp the camera focus point.
    /// </summary>
    [System.Serializable]
    public sealed class CameraBounds
    {
        [SerializeField] private Vector3 min = Vector3.zero;
        [SerializeField] private Vector3 max = new Vector3(256f, 0f, 256f);

        public Vector3 Min => min;
        public Vector3 Max => max;

        public void SetFromMap(float worldSize)
        {
            min = Vector3.zero;
            max = new Vector3(worldSize, 0f, worldSize);
        }

        public Vector3 Clamp(Vector3 position)
        {
            return new Vector3(
                Mathf.Clamp(position.x, min.x, max.x),
                position.y,
                Mathf.Clamp(position.z, min.z, max.z));
        }
    }
}
