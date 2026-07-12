using System;
using CleanEnergy.UI;
using UnityEngine;

namespace CleanEnergy.CameraSystem
{
    /// <summary>
    /// Orthographic isometric-style camera with WASD, drag pan, zoom, and Q/E rotate.
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
        private Vector3 _focusTweenFrom;
        private Vector3 _focusTweenTo;
        private float _focusTweenDuration;
        private float _focusTweenElapsed;
        private bool _focusTweenActive;
        private float _zoomTweenFrom;
        private float _zoomTweenTo;
        private bool _zoomTweenActive;
        private Vector3 _dragLastScreen;
        private bool _dragActive;

        public CameraBounds Bounds => bounds;
        public Vector3 FocusPoint => _focusPoint;
        public float OrthographicSize => _camera != null ? _camera.orthographicSize : maxOrthographicSize;
        public float MinOrthographicSize => minOrthographicSize;
        public float MaxOrthographicSize => maxOrthographicSize;
        public float ZoomSpeed
        {
            get => zoomSpeed;
            set => zoomSpeed = Mathf.Max(0.1f, value);
        }
        public bool IsFocusTweenActive => _focusTweenActive;
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
            used |= HandleDragPan();
            if (used)
            {
                CancelFocusTween();
                CameraInputUsed?.Invoke();
            }
            else
            {
                AdvanceFocusTween(dt);
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

        /// <summary>
        /// Smoothly moves the camera focus to a world point.
        /// </summary>
        public void FocusOn(Vector3 worldPoint, float duration = 0.35f)
        {
            var target = bounds.Clamp(new Vector3(worldPoint.x, focusHeight, worldPoint.z));
            if (duration <= 0.0001f)
            {
                _focusPoint = target;
                CancelFocusTween();
                ApplyTransform();
                return;
            }

            _focusTweenFrom = _focusPoint;
            _focusTweenTo = target;
            _focusTweenDuration = duration;
            _focusTweenElapsed = 0f;
            _focusTweenActive = true;
            _zoomTweenActive = false;
        }

        /// <summary>
        /// Fits orthographic size and focus to a world AABB (XZ).
        /// </summary>
        public void FitToBounds(Bounds worldBounds, float duration = 0.35f)
        {
            if (_camera == null)
            {
                _camera = GetComponent<Camera>();
            }

            var center = new Vector3(worldBounds.center.x, focusHeight, worldBounds.center.z);
            var targetFocus = bounds.Clamp(center);
            var size = CameraFitMath.OrthographicSizeForBounds(
                worldBounds, _camera.aspect, minOrthographicSize, maxOrthographicSize);

            if (duration <= 0.0001f)
            {
                _focusPoint = targetFocus;
                _camera.orthographicSize = size;
                CancelFocusTween();
                ApplyTransform();
                return;
            }

            _focusTweenFrom = _focusPoint;
            _focusTweenTo = targetFocus;
            _zoomTweenFrom = _camera.orthographicSize;
            _zoomTweenTo = size;
            _focusTweenDuration = duration;
            _focusTweenElapsed = 0f;
            _focusTweenActive = true;
            _zoomTweenActive = true;
        }

        public void FitToMapBounds(float duration = 0.35f)
        {
            var mapBounds = new Bounds();
            mapBounds.SetMinMax(
                new Vector3(bounds.Min.x, 0f, bounds.Min.z),
                new Vector3(bounds.Max.x, 0f, bounds.Max.z));
            FitToBounds(mapBounds, duration);
        }

        public void CancelFocusTween()
        {
            _focusTweenActive = false;
            _zoomTweenActive = false;
            _focusTweenElapsed = 0f;
        }

        private void AdvanceFocusTween(float dt)
        {
            if (!_focusTweenActive)
            {
                return;
            }

            _focusTweenElapsed += dt;
            var t = _focusTweenDuration <= 0f ? 1f : _focusTweenElapsed / _focusTweenDuration;
            _focusPoint = CameraFocusMath.LerpFocus(_focusTweenFrom, _focusTweenTo, t);
            if (_zoomTweenActive && _camera != null)
            {
                var u = CameraFocusMath.SmoothStep01(Mathf.Clamp01(t));
                _camera.orthographicSize = Mathf.Lerp(_zoomTweenFrom, _zoomTweenTo, u);
            }

            if (t >= 1f)
            {
                _focusPoint = _focusTweenTo;
                if (_zoomTweenActive && _camera != null)
                {
                    _camera.orthographicSize = _zoomTweenTo;
                }

                CancelFocusTween();
            }
        }

        private bool HandleMove(float dt)
        {
            var input = Vector3.zero;
            if (Input.GetKey(KeybindService.Get(RemappableAction.CamForward)))
            {
                input.z += 1f;
            }

            if (Input.GetKey(KeybindService.Get(RemappableAction.CamBack)))
            {
                input.z -= 1f;
            }

            if (Input.GetKey(KeybindService.Get(RemappableAction.CamRight)))
            {
                input.x += 1f;
            }

            if (Input.GetKey(KeybindService.Get(RemappableAction.CamLeft)))
            {
                input.x -= 1f;
            }

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
            if (Input.GetKey(KeybindService.Get(RemappableAction.CamRotateLeft)))
            {
                rotate -= 1f;
            }

            if (Input.GetKey(KeybindService.Get(RemappableAction.CamRotateRight)))
            {
                rotate += 1f;
            }

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

        /// <summary>
        /// Right or middle mouse drag pans the focus (grab-map style). GDD: middle; RMB also supported.
        /// </summary>
        private bool HandleDragPan()
        {
            var held = Input.GetMouseButton(1) || Input.GetMouseButton(2);
            if (!held)
            {
                _dragActive = false;
                return false;
            }

            var screen = Input.mousePosition;
            if (!_dragActive)
            {
                _dragActive = true;
                _dragLastScreen = screen;
                return true;
            }

            var delta = screen - _dragLastScreen;
            _dragLastScreen = screen;
            if (delta.sqrMagnitude < 1e-4f)
            {
                return true;
            }

            var unitsPerPixel = (2f * _camera.orthographicSize) / Mathf.Max(1f, Screen.height);
            var yawRotation = Quaternion.Euler(0f, _yaw, 0f);
            var right = yawRotation * Vector3.right;
            var forward = yawRotation * Vector3.forward;
            // Grab-the-map: drag right moves the world with the cursor (focus opposite).
            _focusPoint -= (right * delta.x + forward * delta.y) * unitsPerPixel;
            _focusPoint = bounds.Clamp(_focusPoint);
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
