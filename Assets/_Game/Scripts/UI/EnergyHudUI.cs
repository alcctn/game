using CleanEnergy.Energy;
using CleanEnergy.Maintenance;
using CleanEnergy.Placement;
using CleanEnergy.Research;
using CleanEnergy.Simulation;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// Top HUD for production, demand, storage, money, RP and simulation speed.
    /// </summary>
    public sealed class EnergyHudUI : MonoBehaviour
    {
        [SerializeField] private EnergySimulationDriver driver;
        [SerializeField] private SimulationClock clock;
        [SerializeField] private PlacementController placementController;
        [SerializeField] private ResearchController researchController;
        [SerializeField] private MaintenanceController maintenanceController;

        private string _shortageText = string.Empty;
        private float _shortageTimer;
        private string _repairMessage = string.Empty;

        public void Configure(
            EnergySimulationDriver energyDriver,
            SimulationClock simulationClock,
            PlacementController placement,
            ResearchController research = null,
            MaintenanceController maintenance = null)
        {
            if (driver != null)
            {
                driver.ShortageOccurred -= OnShortage;
                driver.BalanceUpdated -= OnBalance;
            }

            driver = energyDriver;
            clock = simulationClock;
            placementController = placement;
            researchController = research;
            maintenanceController = maintenance;

            if (driver != null)
            {
                driver.ShortageOccurred += OnShortage;
                driver.BalanceUpdated += OnBalance;
            }
        }

        private void OnDestroy()
        {
            if (driver != null)
            {
                driver.ShortageOccurred -= OnShortage;
                driver.BalanceUpdated -= OnBalance;
            }
        }

        private void Update()
        {
            if (_shortageTimer > 0f)
            {
                _shortageTimer -= Time.deltaTime;
                if (_shortageTimer <= 0f)
                {
                    _shortageText = string.Empty;
                }
            }
        }

        private void OnGUI()
        {
            GuiScale.Apply();

            const float width = 560f;
            var x = (Screen.width / GuiScale.Current - width) * 0.5f;
            GUILayout.BeginArea(new Rect(x, 8f, width, 132f), GUI.skin.box);

            var result = driver != null ? driver.LastResult : null;
            var money = placementController != null ? placementController.Wallet.Money : 0f;
            var rp = researchController?.Service != null ? researchController.Service.Wallet.Points : 0f;
            var prod = result?.Production ?? 0f;
            var delivered = result?.DeliveredProduction ?? prod;
            var demand = result?.Demand ?? 0f;
            var stored = result?.Stored ?? 0f;
            var phase = clock != null ? clock.DayCycle.Phase : DayPhase.Noon;
            var demandMul = DayCycleService.GetDemandMultiplier(phase);
            var windMul = DayCycleService.GetWindFactor(phase);
            var lowMaint = maintenanceController != null ? maintenanceController.LowMaintenanceCount : 0;
            var upkeep = driver != null ? driver.LastUpkeepTotal : 0f;
            var upkeepBroke = driver != null && driver.CouldNotAffordFullUpkeep;
            var congested = result != null && result.IsCongested;
            var debt = driver != null ? driver.EmergencyCredit.RemainingDebt : 0f;
            var weather = clock != null ? clock.Weather.ActiveKind : WeatherEventKind.None;
            var season = clock != null ? clock.Seasons.Current : SeasonKind.Spring;

            GUILayout.BeginHorizontal();
            GUILayout.Label(congested
                ? $"Prod {prod:F1}→{delivered:F1}"
                : $"Prod {prod:F1}");
            GUILayout.Label($"Demand {demand:F1}");
            GUILayout.Label($"Stored {stored:F1}");
            GUILayout.Label($"Money {money:F0}");
            if (debt > 0.0001f)
            {
                GUILayout.Label($"Debt {debt:F0}");
            }

            GUILayout.Label($"RP {rp:F0}");
            GUILayout.Label($"Upkeep {upkeep:F0}/tick");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"{phase} (x{demandMul:F2}) Wind x{windMul:F2}");
            GUILayout.Label(SeasonService.DisplayName(season));
            if (weather != WeatherEventKind.None)
            {
                GUILayout.Label($"{weather} ({clock.Weather.RemainingTicks})");
            }

            DrawSpeedButton(SimulationSpeed.Paused, "||");
            DrawSpeedButton(SimulationSpeed.One, "1x");
            DrawSpeedButton(SimulationSpeed.Two, "2x");
            DrawSpeedButton(SimulationSpeed.Four, "4x");
            if (clock != null && clock.Speed == SimulationSpeed.Paused)
            {
                GUILayout.Label(StringTable.Get(StringKeys.Pause));
            }
            if (lowMaint > 0)
            {
                GUILayout.Label($"Maintenance low ({lowMaint})");
            }

            if (upkeepBroke)
            {
                GUILayout.Label("Can't afford upkeep");
            }

            if (congested)
            {
                GUILayout.Label("Congested");
            }

            if (!string.IsNullOrEmpty(_shortageText))
            {
                GUILayout.Label(_shortageText);
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (placementController != null && GUILayout.Button("Repair All", GUILayout.Width(90f)))
            {
                if (MaintenanceService.TryGlobalRepairAllProducers(
                        placementController.Occupancy.Occupied,
                        placementController.Wallet,
                        out var count,
                        out var cost,
                        out var fail))
                {
                    _repairMessage = $"Repaired {count} for {cost:F0}.";
                }
                else
                {
                    _repairMessage = fail;
                }
            }

            if (!string.IsNullOrEmpty(_repairMessage))
            {
                GUILayout.Label(_repairMessage);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawSpeedButton(SimulationSpeed speed, string label)
        {
            var selected = clock != null && clock.Speed == speed;
            if (GUILayout.Toggle(selected, label, "Button", GUILayout.Width(40f)) && !selected && clock != null)
            {
                clock.SetSpeed(speed);
            }
        }

        private void OnShortage(EnergyShortageEvent evt)
        {
            _shortageText = $"Shortage {evt.Shortage:F1}";
            _shortageTimer = 2f;
        }

        private void OnBalance(EnergyBalanceResult _)
        {
        }
    }
}
