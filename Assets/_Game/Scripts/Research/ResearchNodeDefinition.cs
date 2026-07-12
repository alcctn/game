using System;
using UnityEngine;

namespace CleanEnergy.Research
{
    /// <summary>
    /// One node in a research tree (serialized inside ResearchTreeDefinition).
    /// </summary>
    [Serializable]
    public sealed class ResearchNodeDefinition
    {
        [SerializeField] private string id = "node";
        [SerializeField] private string displayName = "Node";
        [SerializeField] private string description = "";
        [SerializeField] private string prerequisiteNodeId = "";
        [SerializeField] private float researchPointCost;
        [SerializeField] private bool unlockedByDefault;
        [SerializeField] private string[] unlockBuildingIds = Array.Empty<string>();
        [SerializeField] private string efficiencyTargetBuildingId = "";
        [SerializeField] private float efficiencyBonus;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public string PrerequisiteNodeId => prerequisiteNodeId;
        public float ResearchPointCost => researchPointCost;
        public bool UnlockedByDefault => unlockedByDefault;
        public string[] UnlockBuildingIds => unlockBuildingIds ?? Array.Empty<string>();
        public string EfficiencyTargetBuildingId => efficiencyTargetBuildingId;
        public float EfficiencyBonus => efficiencyBonus;

        public void Configure(
            string nodeId,
            string name,
            string desc,
            string prereq,
            float cost,
            bool unlockedDefault,
            string[] buildings,
            string efficiencyTarget,
            float bonus)
        {
            id = nodeId;
            displayName = name;
            description = desc;
            prerequisiteNodeId = prereq ?? string.Empty;
            researchPointCost = cost;
            unlockedByDefault = unlockedDefault;
            unlockBuildingIds = buildings ?? Array.Empty<string>();
            efficiencyTargetBuildingId = efficiencyTarget ?? string.Empty;
            efficiencyBonus = bonus;
        }
    }
}
