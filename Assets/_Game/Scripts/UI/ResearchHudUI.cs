using CleanEnergy.Research;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// IMGUI panel for research points and unlocking tree nodes.
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

            const float width = 280f;
            GUILayout.BeginArea(new Rect(12f, Screen.height - 250f, width, 238f), GUI.skin.box);
            GUILayout.Label(service.Tree != null ? service.Tree.DisplayName : "Research");
            GUILayout.Label($"RP: {service.Wallet.Points:F0}");

            var nodes = service.Tree.Nodes;
            for (var i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                if (node == null)
                {
                    continue;
                }

                if (service.IsNodeUnlocked(node.Id))
                {
                    GUILayout.Label($"[x] {node.DisplayName}");
                    continue;
                }

                var can = service.CanUnlock(node.Id, out var reason);
                var label = can
                    ? $"Unlock {node.DisplayName} ({node.ResearchPointCost:F0} RP)"
                    : $"{node.DisplayName} — {reason}";
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
