using CleanEnergy.CameraSystem;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using CleanEnergy.Placement;
using UnityEngine;

namespace CleanEnergy.DebugTools
{
    /// <summary>
    /// Focuses the isometric camera when a map cell is selected.
    /// </summary>
    public sealed class SelectionCameraFocus : MonoBehaviour
    {
        [SerializeField] private MapDebugOverlay debugOverlay;
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private PlacementController placementController;
        [SerializeField] private IsometricCameraController cameraController;
        [SerializeField] private float focusDuration = 0.35f;

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

            if (!mapGenerator.Grid.TryGetCell(coordinate.Value, out var cell))
            {
                return;
            }

            cameraController.FocusOn(cell.WorldPosition, focusDuration);
        }
    }
}
