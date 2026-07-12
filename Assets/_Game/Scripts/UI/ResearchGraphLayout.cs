using System;
using System.Collections.Generic;
using CleanEnergy.Research;

namespace CleanEnergy.UI
{
    /// <summary>
    /// Pure layout helpers for research tree graph UI (branch + prerequisite depth).
    /// </summary>
    public static class ResearchGraphLayout
    {
        /// <summary>
        /// Number of prerequisite links from this node up to a root (empty prereq).
        /// </summary>
        public static int PrerequisiteDepth(
            ResearchNodeDefinition node,
            IReadOnlyDictionary<string, ResearchNodeDefinition> byId)
        {
            if (node == null)
            {
                return 0;
            }

            var depth = 0;
            var current = node;
            var guard = 0;
            while (!string.IsNullOrEmpty(current.PrerequisiteNodeId) && guard++ < 64)
            {
                depth++;
                if (byId == null || !byId.TryGetValue(current.PrerequisiteNodeId, out current) || current == null)
                {
                    break;
                }
            }

            return depth;
        }

        /// <summary>Walks prerequisites to the branch root id (node with empty prereq).</summary>
        public static string ResolveBranchRootId(
            ResearchNodeDefinition node,
            IReadOnlyDictionary<string, ResearchNodeDefinition> byId)
        {
            if (node == null)
            {
                return string.Empty;
            }

            var current = node;
            var guard = 0;
            while (!string.IsNullOrEmpty(current.PrerequisiteNodeId) && guard++ < 64)
            {
                if (byId == null || !byId.TryGetValue(current.PrerequisiteNodeId, out var parent) || parent == null)
                {
                    break;
                }

                current = parent;
            }

            return current.Id ?? string.Empty;
        }

        public static Dictionary<string, ResearchNodeDefinition> IndexById(ResearchNodeDefinition[] nodes)
        {
            var map = new Dictionary<string, ResearchNodeDefinition>(StringComparer.Ordinal);
            if (nodes == null)
            {
                return map;
            }

            for (var i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                if (node == null || string.IsNullOrEmpty(node.Id) || map.ContainsKey(node.Id))
                {
                    continue;
                }

                map[node.Id] = node;
            }

            return map;
        }

        /// <summary>
        /// Stable order: branch root id, then depth, then display name.
        /// </summary>
        public static List<ResearchNodeDefinition> OrderForGraph(ResearchNodeDefinition[] nodes)
        {
            var list = new List<ResearchNodeDefinition>();
            if (nodes == null)
            {
                return list;
            }

            var byId = IndexById(nodes);
            for (var i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] != null)
                {
                    list.Add(nodes[i]);
                }
            }

            list.Sort((a, b) =>
            {
                var branchA = ResolveBranchRootId(a, byId);
                var branchB = ResolveBranchRootId(b, byId);
                var c = string.CompareOrdinal(branchA, branchB);
                if (c != 0)
                {
                    return c;
                }

                var depthA = PrerequisiteDepth(a, byId);
                var depthB = PrerequisiteDepth(b, byId);
                if (depthA != depthB)
                {
                    return depthA.CompareTo(depthB);
                }

                return string.CompareOrdinal(a.DisplayName, b.DisplayName);
            });

            return list;
        }

        public static string IndentPrefix(int depth)
        {
            if (depth <= 0)
            {
                return string.Empty;
            }

            return new string(' ', depth * 2);
        }
    }
}
