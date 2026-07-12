using CleanEnergy.CameraSystem;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using CleanEnergy.Placement;
using UnityEngine;

namespace CleanEnergy.DebugTools
{
    /// <summary>
    /// Focuses the isometric camera on selection and placement hover cells.
    /// </summary>
    public sealed class SelectionCameraFocus : MonoBehaviour
    {
        [SerializeField] private MapDebugOverlay debugOverlay;
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private PlacementController placementController;
        [SerializeField] private IsometricCameraController cameraController;
        [SerializeField] private float focusDuration = 0.35f;
        [SerializeField] private float placementFocusDuration = 0.25f;

        private GridCoordinate? _lastPlacementHover;

        public void Configure(
            MapDebugOverlay overlay,
            MapGenerator generator,
            PlacementController placement,
            IsometricCameraController camera,
            float duration = 0.35f)
        {
            Unsubscribe();
            debugOverlay = overlay;
            mapGenerator = generator;
            placementController = placement;
            cameraController = camera;
            focusDuration = duration;
            Subscribe();
        }

        private void OnEnable() => Subscribe();

        private void OnDisable() => Unsubscribe();

        private void Update()
        {
            UpdatePlacementHoverFocus();
            if (Input.GetKeyDown(KeyCode.Home))
            {
                FitHome();
            }
        }

        public void FitHome(float duration = -1f)
        {
            if (cameraController == null)
            {
                return;
            }

            var d = duration < 0f ? focusDuration : duration;
            if (debugOverlay != null
                && debugOverlay.SelectedCell.HasValue
                && mapGenerator != null
                && mapGenerator.Grid.IsInitialized
                && mapGenerator.Grid.TryGetCell(debugOverlay.SelectedCell.Value, out var cell))
            {
                var cellSize = mapGenerator.Grid.CellSize;
                var fit = CameraFitMath.BoundsAroundCell(
                    cell.WorldPosition, cellSize, CameraFitMath.SelectionPaddingCells);
                cameraController.FitToBounds(fit, d);
                return;
            }

            cameraController.FitToMapBounds(d);
        }

        private void Subscribe()
        {
            if (debugOverlay != null)
            {
                debugOverlay.SelectionChanged += OnSelectionChanged;
            }
        }

        private void Unsubscribe()
        {
            if (debugOverlay != null)
            {
                debugOverlay.SelectionChanged -= OnSelectionChanged;
            }
        }

        private void OnSelectionChanged(GridCoordinate? coordinate)
        {
            if (!coordinate.HasValue
                || cameraController == null
                || mapGenerator == null
                || !mapGenerator.Grid.IsInitialized)
            {
                return;
            }

            if (placementController != null && placementController.IsPlacementActive)
            {
                return;
            }

            FocusCell(coordinate.Value, focusDuration);
        }

        private void UpdatePlacementHoverFocus()
        {
            if (placementController == null
                || cameraController == null
                || mapGenerator == null
                || !mapGenerator.Grid.IsInitialized
                || !placementController.IsPlacementActive)
            {
                _lastPlacementHover = null;
                return;
            }

            if (!placementController.HoverCoordinate.HasValue)
            {
                _lastPlacementHover = null;
                return;
            }

            var hover = placementController.HoverCoordinate.Value;
            if (_lastPlacementHover.HasValue && _lastPlacementHover.Value.Equals(hover))
            {
                return;
            }

            _lastPlacementHover = hover;
            FocusCell(hover, placementFocusDuration);
        }

        private void FocusCell(GridCoordinate coordinate, float duration)
        {
            if (!mapGenerator.Grid.TryGetCell(coordinate, out var cell))
            {
                return;
            }

            cameraController.FocusOn(cell.WorldPosition, duration);
        }

        /// <summary>Pure helper for tests: whether a hover change should trigger focus.</summary>
        public static bool ShouldFocusPlacementHover(
            bool placementActive,
            GridCoordinate? hover,
            GridCoordinate? lastHover)
        {
            if (!placementActive || !hover.HasValue)
            {
                return false;
            }

            return !lastHover.HasValue || !lastHover.Value.Equals(hover.Value);
        }
    }
}
