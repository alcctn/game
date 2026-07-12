using CleanEnergy.Placement;
using UnityEngine;

namespace CleanEnergy.Maintenance
{
    /// <summary>
    /// Holds maintenance service state for HUD and energy tick integration.
    /// </summary>
    public sealed class MaintenanceController : MonoBehaviour
    {
        [SerializeField] private PlacementController placementController;

        private readonly MaintenanceService _service = new MaintenanceService();

        public MaintenanceService Service => _service;
        public int LowMaintenanceCount => _service.LowMaintenanceCount;

        public void Configure(PlacementController placement)
        {
            placementController = placement;
        }

        public void ProcessTick()
        {
            if (placementController == null)
            {
                return;
            }

            _service.ProcessTick(placementController.Occupancy.Occupied);
        }
    }
}
