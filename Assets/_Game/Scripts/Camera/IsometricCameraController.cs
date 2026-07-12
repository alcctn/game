using System;
using UnityEngine;

namespace CleanEnergy.CameraSystem
{
    /// <summary>
    /// Orthographic isometric-style camera with WASD, zoom, and Q/E rotate.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class IsometricCameraController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 40f;
        [SerializeField] private float rotateSpeed = 90f;
        [SerializeField] private float zoomSpeed = 10f;
        [SerializeField] private float minOrthographicSize = 8f;
        [SerializeField] private float maxOrthographicSize = 80f;
        [SerializeField] private float pitchDegrees = 45f;
        [SerializeField] private float initialYawDegrees = 45f;
        [SerializeField] private float focusHeight = 0f;
        [SerializeField] private CameraBounds bounds = new CameraBounds();

        private Camera _camera;
        private Vector3 _focusPoint;
        private float _yaw;

        public CameraBounds Bounds => bounds;
        public event Action CameraInputUsed;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _camera.orthographic = true;
            _yaw = initialYawDegrees;
            _focusPoint = new Vector3(
                (bounds.Min.x + bounds.Max.x) * 0.5f,
                focusHeight,
                (bounds.Min.z + bounds.Max.z) * 0.5f);
            ApplyTransform();
        }

        private void Update()
        {
            var dt = Time.deltaTime;
            var used = false;
            used |= HandleMove(dt);
            used |= HandleRotate(dt);
            used |= HandleZoom();
            if (used)
            {
                CameraInputUsed?.Invoke();
            }

            ApplyTransform();
        }

        public void ConfigureBounds(float worldSize)
        {
            bounds.SetFromMap(worldSize);
            _focusPoint = bounds.Clamp(_focusPoint);
            if (_focusPoint == Vector3.zero)
            {
                _focusPoint = new Vector3(worldSize * 0.5f, focusHeight, worldSize * 0.5f);
            }
        }

        private bool HandleMove(float dt)
        {
            var input = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) input.z += 1f;
            if (Input.GetKey(KeyCode.S)) input.z -= 1f;
            if (Input.GetKey(KeyCode.D)) input.x += 1f;
            if (Input.GetKey(KeyCode.A)) input.x -= 1f;

            if (input.sqrMagnitude < 1e-6f)
            {
                return false;
            }

            input.Normalize();
            var yawRotation = Quaternion.Euler(0f, _yaw, 0f);
            var worldMove = yawRotation * new Vector3(input.x, 0f, input.z);
            var zoomFactor = _camera.orthographicSize / maxOrthographicSize;
            _focusPoint += worldMove * (moveSpeed * Mathf.Max(0.25f, zoomFactor) * dt);
            _focusPoint = bounds.Clamp(_focusPoint);
            return true;
        }

        private bool HandleRotate(float dt)
        {
            var rotate = 0f;
            if (Input.GetKey(KeyCode.Q)) rotate -= 1f;
            if (Input.GetKey(KeyCode.E)) rotate += 1f;
            if (Mathf.Abs(rotate) < 1e-6f)
            {
                return false;
            }

            _yaw += rotate * rotateSpeed * dt;
            return true;
        }

        private bool HandleZoom()
        {
            var scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) < 1e-4f)
            {
                return false;
            }

            _camera.orthographicSize = Mathf.Clamp(
                _camera.orthographicSize - scroll * zoomSpeed,
                minOrthographicSize,
                maxOrthographicSize);
            return true;
        }

        private void ApplyTransform()
        {
            var rotation = Quaternion.Euler(pitchDegrees, _yaw, 0f);
            var distance = _camera.orthographicSize * 3f;
            transform.rotation = rotation;
            transform.position = _focusPoint - rotation * Vector3.forward * distance;
        }
    }
}
