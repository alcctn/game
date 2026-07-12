using System;

namespace CleanEnergy.Save
{
    [Serializable]
    public sealed class BuildingSaveData
    {
        public string definitionId;
        public int x;
        public int y;
        public int rotation;
        public float storedEnergy;
        public float maintenanceLevel = 1f;
    }

    [Serializable]
    public sealed class ScenarioSaveData
    {
        public int coverageStreakTicks;
        public bool demandObjectiveComplete;
        public bool diversityObjectiveComplete;
        public bool batteryObjectiveComplete;
        public bool researchObjectiveComplete;
        public int activeProducerTypeCount;
        public float coverageRatio;
        public float satisfaction;
        public int shortageStreakTicks;
        public bool isAtRisk;
        public bool hasWon;
        public bool hasLost;
    }

    /// <summary>
    /// JSON-serializable snapshot of prototype runtime state.
    /// </summary>
    [Serializable]
    public sealed class GameSaveData
    {
        public const int CurrentVersion = 1;

        public int saveVersion = CurrentVersion;
        public string seed = "12345";
        public int tickIndex;
        public float money;
        public float researchPoints;
        public bool emergencyCreditUsed;
        public float creditDebt;
        public int creditUses;
        public string[] unlockedNodeIds = Array.Empty<string>();
        public BuildingSaveData[] buildings = Array.Empty<BuildingSaveData>();
        public ScenarioSaveData scenario = new ScenarioSaveData();
        public int tutorialStep;
        public string scenarioId = "";
        public float settlementPopulation = 100f;
    }
}
