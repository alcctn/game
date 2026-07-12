using CleanEnergy.Research;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// IMGUI panel for research points and unlocking tree nodes (branch + depth layout).
    /// </summary>
    public sealed class ResearchHudUI : MonoBehaviour
    {
        [SerializeField] private ResearchController researchController;

        public void Configure(ResearchController controller)
        {
            researchController = controller;
        }

        private void OnGUI()
        {
            var service = researchController != null ? researchController.Service : null;
            if (service == null)
            {
                return;
            }

            GuiScale.Apply();

            const float width = 320f;
            GUILayout.BeginArea(new Rect(12f, Screen.height / GuiScale.Current - 320f, width, 308f), GUI.skin.box);
            GUILayout.Label(service.Tree != null ? service.Tree.DisplayName : StringTable.Get(StringKeys.Research));
            GUILayout.Label($"RP: {service.Wallet.Points:F0}");

            var nodes = service.Tree != null ? service.Tree.Nodes : null;
            var ordered = ResearchGraphLayout.OrderForGraph(nodes);
            var byId = ResearchGraphLayout.IndexById(nodes);
            string lastBranch = null;

            for (var i = 0; i < ordered.Count; i++)
            {
                var node = ordered[i];
                var branch = ResearchGraphLayout.ResolveBranchRootId(node, byId);
                if (branch != lastBranch)
                {
                    lastBranch = branch;
                    var rootName = byId.TryGetValue(branch, out var root) && root != null
                        ? root.DisplayName
                        : branch;
                    GUILayout.Space(4f);
                    GUILayout.Label($"— {rootName} —");
                }

                var depth = ResearchGraphLayout.PrerequisiteDepth(node, byId);
                var indent = ResearchGraphLayout.IndentPrefix(depth);

                if (service.IsNodeUnlocked(node.Id))
                {
                    GUILayout.Label($"{indent}[x] {node.DisplayName}");
                    continue;
                }

                var can = service.CanUnlock(node.Id, out var reason);
                var label = can
                    ? $"{indent}Unlock {node.DisplayName} ({node.ResearchPointCost:F0} RP)"
                    : $"{indent}{node.DisplayName} — {reason}";
                GUI.enabled = can;
                if (GUILayout.Button(label))
                {
                    service.TryUnlock(node.Id);
                }

                GUI.enabled = true;
            }

            GUILayout.EndArea();
        }
    }
}
