using CleanEnergy.DebugTools;
using CleanEnergy.Energy;
using CleanEnergy.Maintenance;
using CleanEnergy.Placement;
using CleanEnergy.Research;
using CleanEnergy.Scenario;
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
        [SerializeField] private ScenarioController scenarioController;

        private string _shortageText = string.Empty;
        private float _shortageTimer;
        private string _repairMessage = string.Empty;
        private string _envMeterText = string.Empty;

        public void Configure(
            EnergySimulationDriver energyDriver,
            SimulationClock simulationClock,
            PlacementController placement,
            ResearchController research = null,
            MaintenanceController maintenance = null,
            ScenarioController scenario = null)
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
            scenarioController = scenario;

            if (driver != null)
            {
                driver.ShortageOccurred += OnShortage;
                driver.BalanceUpdated += OnBalance;
            }

            RefreshEnvMeter();
        }

        /// <summary>Formats RP with optional coverage income suffix.</summary>
        public static string FormatRpLabel(float points, float coverageRpPerTick)
        {
            if (coverageRpPerTick > 0.0001f)
            {
                return $"RP {points:F0} (+{coverageRpPerTick:F0}/tick)";
            }

            return $"RP {points:F0}";
        }

        /// <summary>Surplus sold from the last energy balance (0 when none).</summary>
        public static float ReadSurplusSold(EnergyBalanceResult result) =>
            result != null ? result.SurplusSold : 0f;

        /// <summary>Scenario satisfaction percent; defaults to 100 when state is missing.</summary>
        public static float ReadSatisfaction(ScenarioObjectiveState state) =>
            state != null ? state.Satisfaction : 100f;

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
            GUILayout.BeginArea(new Rect(x, 8f, width, 156f), GUI.skin.box);

            var result = driver != null ? driver.LastResult : null;
            var money = placementController != null ? placementController.Wallet.Money : 0f;
            var rp = researchController?.Service != null ? researchController.Service.Wallet.Points : 0f;
            var prod = result?.Production ?? 0f;
            var delivered = result?.DeliveredProduction ?? prod;
            var demand = result?.Demand ?? 0f;
            var stored = result?.Stored ?? 0f;
            var surplusSold = ReadSurplusSold(result);
            var satisfaction = ReadSatisfaction(scenarioController != null ? scenarioController.State : null);
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
            GUILayout.Label($"Surplus {surplusSold:F1}");
            GUILayout.Label($"Sat {satisfaction:F0}");
            GUILayout.Label($"Money {money:F0}");
            if (debt > 0.0001f)
            {
                GUILayout.Label($"Debt {debt:F0}");
            }

            GUILayout.Label(FormatRpLabel(rp, researchController != null
                ? researchController.LastCoverageRpGranted
                : 0f));
            GUILayout.Label($"Upkeep {upkeep:F0}/tick");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (!string.IsNullOrEmpty(_envMeterText))
            {
                GUILayout.Label(_envMeterText);
            }

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
                RepairUndoService repairUndo = null;
                if (maintenanceController != null)
                {
                    repairUndo = maintenanceController.RepairUndo;
                }

                if (MaintenanceService.TryGlobalRepairAllProducers(
                        placementController.Occupancy.Occupied,
                        placementController.Wallet,
                        repairUndo,
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
            RefreshEnvMeter();
        }

        private void RefreshEnvMeter()
        {
            var occupied = placementController != null ? placementController.Occupancy?.Occupied : null;
            _envMeterText = EnvironmentalImpact.FormatHudMeter(occupied);
        }
    }
}
