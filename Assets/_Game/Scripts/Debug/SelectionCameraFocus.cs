using System.Collections.Generic;
using CleanEnergy.CameraSystem;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using CleanEnergy.Placement;
using UnityEngine;

namespace CleanEnergy.DebugTools
{
    /// <summary>
    /// Focuses the isometric camera on selection and placement hover cells.
    /// Home / selection focus sets orthographic size from selection bounds (S58 / S110).
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
            if (Input.GetKeyDown(CleanEnergy.UI.KeybindService.Get(CleanEnergy.UI.RemappableAction.Home)))
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
            if (TryBuildSelectionFitBounds(out var fit))
            {
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
                debugOverlay.MultiSelectionChanged += OnMultiSelectionChanged;
            }
        }

        private void Unsubscribe()
        {
            if (debugOverlay != null)
            {
                debugOverlay.SelectionChanged -= OnSelectionChanged;
                debugOverlay.MultiSelectionChanged -= OnMultiSelectionChanged;
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

        private void OnMultiSelectionChanged()
        {
            if (cameraController == null
                || mapGenerator == null
                || !mapGenerator.Grid.IsInitialized
                || placementController != null && placementController.IsPlacementActive)
            {
                return;
            }

            if (TryBuildSelectionFitBounds(out var fit))
            {
                cameraController.FitToBounds(fit, focusDuration);
            }
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
            // Do not pan/zoom camera while placing — hover preview is enough.
            // Continuous FocusOn caused map jitter as the cursor crossed cells.
        }

        private void FocusCell(GridCoordinate coordinate, float duration)
        {
            if (!mapGenerator.Grid.TryGetCell(coordinate, out var cell))
            {
                return;
            }

            var cellSize = mapGenerator.Grid.CellSize;
            var fit = CameraFitMath.BoundsAroundCell(
                cell.WorldPosition, cellSize, CameraFitMath.SelectionPaddingCells);
            cameraController.FitToBounds(fit, duration);
        }

        private bool TryBuildSelectionFitBounds(out Bounds fit)
        {
            fit = default;
            if (debugOverlay == null
                || mapGenerator == null
                || !mapGenerator.Grid.IsInitialized)
            {
                return false;
            }

            var cellSize = mapGenerator.Grid.CellSize;
            var multi = debugOverlay.MultiSelectedCells;
            if (multi != null && multi.Count > 0)
            {
                var positions = new List<Vector3>(multi.Count);
                for (var i = 0; i < multi.Count; i++)
                {
                    if (mapGenerator.Grid.TryGetCell(multi[i], out var cell))
                    {
                        positions.Add(cell.WorldPosition);
                    }
                }

                if (positions.Count > 0)
                {
                    fit = CameraFitMath.BoundsAroundCells(
                        positions, cellSize, CameraFitMath.SelectionPaddingCells);
                    return true;
                }
            }

            if (debugOverlay.SelectedCell.HasValue
                && mapGenerator.Grid.TryGetCell(debugOverlay.SelectedCell.Value, out var selected))
            {
                fit = CameraFitMath.BoundsAroundCell(
                    selected.WorldPosition, cellSize, CameraFitMath.SelectionPaddingCells);
                return true;
            }

            return false;
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

        /// <summary>Pure helper: ortho size for selection fit with clamp.</summary>
        public static float ResolveSelectionOrthoSize(
            Bounds selectionBounds,
            float aspect,
            float minSize,
            float maxSize)
        {
            return CameraFitMath.OrthographicSizeForBounds(
                selectionBounds, aspect, minSize, maxSize);
        }
    }
}
