using System.Collections.Generic;
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
        private static int _buildFrame = -1;

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
                _buildFrame = frame;
            }

            if (guiRect.width > 1f && guiRect.height > 1f)
            {
                Building.Add(guiRect);
            }
        }

        public static bool IsPointerOverGui()
        {
            var mouse = GuiMouseFromScreen(Input.mousePosition);
            return Contains(Building, mouse) || Contains(Active, mouse);
        }

        public static Vector2 GuiMouseFromScreen(Vector3 screenMouse)
        {
            return new Vector2(screenMouse.x, Screen.height - screenMouse.y);
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
