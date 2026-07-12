using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// Tracks IMGUI panel rects so world clicks can ignore UI.
    /// Update runs before OnGUI, so hit-tests use the previous frame's registered rects.
    /// </summary>
    public static class ImguiHitTest
    {
        private static readonly List<Rect> Active = new List<Rect>(16);
        private static readonly List<Rect> Building = new List<Rect>(16);
        private static readonly List<string> BuildingNames = new List<string>(16);
        private static int _buildFrame = -1;
        private static bool _loggedLayout;

        public static void Register(Rect guiRect, string name = null)
        {
            var frame = Time.frameCount;
            if (frame != _buildFrame)
            {
                Active.Clear();
                for (var i = 0; i < Building.Count; i++)
                {
                    Active.Add(Building[i]);
                }

                Building.Clear();
                BuildingNames.Clear();
                _buildFrame = frame;
            }

            if (guiRect.width > 1f && guiRect.height > 1f)
            {
                Building.Add(guiRect);
                BuildingNames.Add(name ?? ("p" + Building.Count));
            }
        }

        public static bool IsPointerOverGui()
        {
            var mouse = GuiMouseFromScreen(Input.mousePosition);
            if (Contains(Building, mouse) || Contains(Active, mouse))
            {
                return true;
            }

            return false;
        }

        public static Vector2 GuiMouseFromScreen(Vector3 screenMouse)
        {
            return new Vector2(screenMouse.x, Screen.height - screenMouse.y);
        }

        /// <summary>Call once at end of a frame's GUI from any panel to log overlaps.</summary>
        public static void LogOverlapsIfNeeded()
        {
            // #region agent log
            if (_loggedLayout || Building.Count < 2)
            {
                return;
            }

            _loggedLayout = true;
            var overlaps = 0;
            var sb = new StringBuilder(256);
            sb.Append("{\"count\":").Append(Building.Count).Append(",\"pairs\":[");
            var first = true;
            for (var i = 0; i < Building.Count; i++)
            {
                for (var j = i + 1; j < Building.Count; j++)
                {
                    if (!HudLayout.Overlaps(Building[i], Building[j]))
                    {
                        continue;
                    }

                    overlaps++;
                    if (!first)
                    {
                        sb.Append(',');
                    }

                    first = false;
                    sb.Append("{\"a\":\"").Append(BuildingNames[i])
                        .Append("\",\"b\":\"").Append(BuildingNames[j]).Append("\"}");
                }
            }

            sb.Append("],\"overlapCount\":").Append(overlaps).Append('}');
            CleanEnergy.DebugTools.AgentDebugLog.Write(
                "A",
                "ImguiHitTest.LogOverlaps",
                "layout_pass",
                sb.ToString());
            // #endregion
        }

        private static bool Contains(List<Rect> panels, Vector2 mouse)
        {
            for (var i = 0; i < panels.Count; i++)
            {
                if (panels[i].Contains(mouse))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
