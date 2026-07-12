using System.Collections.Generic;
using CleanEnergy.Energy;
using CleanEnergy.Map;
using CleanEnergy.Placement;
using CleanEnergy.Research;
using CleanEnergy.Scenario;
using CleanEnergy.Simulation;
using CleanEnergy.Tutorial;
using UnityEngine;

namespace CleanEnergy.Save
{
    /// <summary>
    /// Collects and applies GameSaveData against the live prototype scene.
    /// </summary>
    public sealed class SaveLoadController : MonoBehaviour
    {
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private PlacementController placementController;
        [SerializeField] private ResearchController researchController;
        [SerializeField] private ScenarioController scenarioController;
        [SerializeField] private SimulationClock clock;
        [SerializeField] private EnergyNetworkService networkService;
        [SerializeField] private TutorialController tutorialController;
        [SerializeField] private EnergySimulationDriver energyDriver;

        private readonly SaveGameService _saveService = new SaveGameService();

        public SaveGameService Service => _saveService;
        public string LastMessage { get; private set; } = string.Empty;

        public void Configure(
            MapGenerator map,
            PlacementController placement,
            ResearchController research,
            ScenarioController scenario,
            SimulationClock simulationClock,
            EnergyNetworkService network,
            TutorialController tutorial = null,
            EnergySimulationDriver driver = null)
        {
            mapGenerator = map;
            placementController = placement;
            researchController = research;
            scenarioController = scenario;
            clock = simulationClock;
            networkService = network;
            tutorialController = tutorial;
            energyDriver = driver;
        }

        public bool SaveSlot()
        {
            var data = Collect();
            if (data == null)
            {
                LastMessage = "Save failed: missing systems.";
                return false;
            }

            _saveService.Write(data);
            LastMessage = "Saved slot1.";
            return true;
        }

        public bool LoadSlot()
        {
            var data = _saveService.Read();
            if (data == null)
            {
                LastMessage = "No save found.";
                return false;
            }

            if (!Apply(data))
            {
                LastMessage = "Load failed.";
                return false;
            }

            LastMessage = "Loaded slot1.";
            return true;
        }

        public GameSaveData Collect()
        {
            if (mapGenerator?.Settings == null || placementController == null)
            {
                return null;
            }

            var data = new GameSaveData
            {
                saveVersion = GameSaveData.CurrentVersion,
                seed = mapGenerator.Settings.Seed,
                tickIndex = clock != null ? clock.TickIndex : 0,
                money = placementController.Wallet != null ? placementController.Wallet.Money : 0f,
                researchPoints = researchController?.Service != null
                    ? researchController.Service.Wallet.Points
                    : 0f,
                emergencyCreditUsed = energyDriver != null
                    && energyDriver.EmergencyCredit.HasBeenUsed,
                tutorialStep = tutorialController?.Progress != null
                    ? (int)tutorialController.Progress.CurrentStep
                    : 0
            };

            if (researchController?.Service != null)
            {
                var nodes = researchController.Service.UnlockedNodeIds;
                var list = new List<string>(nodes.Count);
                foreach (var id in nodes)
                {
                    list.Add(id);
                }

                data.unlockedNodeIds = list.ToArray();
            }

            data.buildings = CollectBuildings();
            data.scenario = CollectScenario();
            return data;
        }

        public bool Apply(GameSaveData data)
        {
            if (data == null || mapGenerator == null || placementController == null)
            {
                return false;
            }

            if (tutorialController != null)
            {
                tutorialController.SuppressEvents = true;
            }

            mapGenerator.SetSeed(string.IsNullOrEmpty(data.seed) ? "12345" : data.seed);
            if (!mapGenerator.Generate())
            {
                if (tutorialController != null)
                {
                    tutorialController.SuppressEvents = false;
                }

                return false;
            }

            if (researchController?.Service != null)
            {
                researchController.Service.Restore(data.unlockedNodeIds, data.researchPoints);
                var types = data.scenario != null ? data.scenario.activeProducerTypeCount : 0;
                researchController.NotifyAfterLoad(types);
            }

            placementController.ResetFactoryIds();
            placementController.CancelPlacement();
            if (data.buildings != null)
            {
                for (var i = 0; i < data.buildings.Length; i++)
                {
                    var b = data.buildings[i];
                    if (b == null || string.IsNullOrEmpty(b.definitionId))
                    {
                        continue;
                    }

                    placementController.TryPlaceFromSave(
                        b.definitionId,
                        new Grid.GridCoordinate(b.x, b.y),
                        b.rotation,
                        b.storedEnergy,
                        b.maintenanceLevel > 0.001f ? b.maintenanceLevel : 1f);
                }
            }

            if (placementController.Wallet != null)
            {
                placementController.Wallet.SetMoney(data.money);
            }

            if (energyDriver != null)
            {
                // Generate already reset the flag via MapGenerated; restore from save.
                energyDriver.EmergencyCredit.Restore(data.emergencyCreditUsed);
            }

            if (scenarioController != null && data.scenario != null)
            {
                scenarioController.RestoreProgress(ToScenarioState(data.scenario));
            }

            if (clock != null)
            {
                clock.RestoreTick(data.tickIndex);
            }

            if (tutorialController != null)
            {
                var step = (TutorialStepId)Mathf.Clamp(
                    data.tutorialStep,
                    (int)TutorialStepId.Camera,
                    (int)TutorialStepId.Completed);
                // Clamp covers legacy saves if enum order changes.
                tutorialController.RestoreTutorial(step);
                tutorialController.SuppressEvents = false;
            }

            networkService?.MarkDirty();
            networkService?.Rebuild();
            return true;
        }

        private BuildingSaveData[] CollectBuildings()
        {
            var seen = new HashSet<string>();
            var list = new List<BuildingSaveData>();
            foreach (var pair in placementController.Occupancy.Occupied)
            {
                var instance = pair.Value;
                if (instance?.Definition == null || !seen.Add(instance.InstanceId))
                {
                    continue;
                }

                list.Add(new BuildingSaveData
                {
                    definitionId = instance.Definition.Id,
                    x = instance.Coordinate.X,
                    y = instance.Coordinate.Y,
                    rotation = instance.Rotation,
                    storedEnergy = instance.StoredEnergy,
                    maintenanceLevel = instance.MaintenanceLevel
                });
            }

            return list.ToArray();
        }

        private ScenarioSaveData CollectScenario()
        {
            var state = scenarioController != null ? scenarioController.State : null;
            if (state == null)
            {
                return new ScenarioSaveData();
            }

            return new ScenarioSaveData
            {
                coverageStreakTicks = state.CoverageStreakTicks,
                demandObjectiveComplete = state.DemandObjectiveComplete,
                diversityObjectiveComplete = state.DiversityObjectiveComplete,
                batteryObjectiveComplete = state.BatteryObjectiveComplete,
                researchObjectiveComplete = state.ResearchObjectiveComplete,
                activeProducerTypeCount = state.ActiveProducerTypeCount,
                coverageRatio = state.CoverageRatio,
                satisfaction = state.Satisfaction,
                shortageStreakTicks = state.ShortageStreakTicks,
                isAtRisk = state.IsAtRisk,
                hasWon = state.HasWon,
                hasLost = state.HasLost
            };
        }

        private static ScenarioObjectiveState ToScenarioState(ScenarioSaveData data)
        {
            return new ScenarioObjectiveState
            {
                CoverageStreakTicks = data.coverageStreakTicks,
                DemandObjectiveComplete = data.demandObjectiveComplete,
                DiversityObjectiveComplete = data.diversityObjectiveComplete,
                BatteryObjectiveComplete = data.batteryObjectiveComplete,
                ResearchObjectiveComplete = data.researchObjectiveComplete,
                ActiveProducerTypeCount = data.activeProducerTypeCount,
                CoverageRatio = data.coverageRatio,
                Satisfaction = data.satisfaction,
                ShortageStreakTicks = data.shortageStreakTicks,
                IsAtRisk = data.isAtRisk,
                HasWon = data.hasWon,
                HasLost = data.hasLost
            };
        }
    }
}
