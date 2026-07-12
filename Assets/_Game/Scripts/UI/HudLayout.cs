using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// Non-overlapping screen-space slots for play-mode IMGUI HUDs.
    /// Panels that call <see cref="GuiScale.Apply"/> must use <see cref="ToScaled"/>.
    /// </summary>
    public static class HudLayout
    {
        public const float Margin = 12f;
        public const float Gap = 8f;
        public const float LeftColW = 300f;
        public const float RightColW = 280f;

        public static Rect TerrainDebug()
        {
            return new Rect(Margin, Margin, LeftColW, 336f);
        }

        public static Rect Tutorial()
        {
            var above = TerrainDebug();
            return new Rect(Margin, above.yMax + Gap, LeftColW, 128f);
        }

        /// <summary>Scenario checklist — right of terrain debug, below top energy bar.</summary>
        public static Rect ScenarioChecklist()
        {
            return new Rect(TerrainDebug().xMax + Gap, 172f, 260f, 148f);
        }

        public static Rect Inspection()
        {
            var h = Mathf.Clamp(Screen.height * 0.40f, 280f, 380f);
            return new Rect(Screen.width - RightColW - Margin, Margin, RightColW, h);
        }

        public static Rect Building()
        {
            var insp = Inspection();
            var y = insp.yMax + Gap;
            var saveReserve = 160f;
            var h = Mathf.Max(220f, Screen.height - y - saveReserve);
            h = Mathf.Min(h, 440f);
            return new Rect(insp.x, y, RightColW, h);
        }

        public static Rect Research()
        {
            const float h = 300f;
            var y = Screen.height - h - Margin;
            var tutBottom = Tutorial().yMax + Gap;
            if (y < tutBottom)
            {
                y = tutBottom;
            }

            var height = Screen.height - Margin - y;
            return new Rect(Margin, y, LeftColW, Mathf.Max(160f, height));
        }

        public static Rect Telemetry()
        {
            const float w = 260f;
            const float h = 190f;
            return new Rect(Margin + LeftColW + Gap, Screen.height - h - Margin, w, h);
        }

        public static Rect SaveLoad()
        {
            const float w = 220f;
            const float h = 140f;
            return new Rect(Screen.width - w - Margin, Screen.height - h - Margin, w, h);
        }

        public static Rect MuteSfx()
        {
            const float w = 120f;
            const float h = 28f;
            var save = SaveLoad();
            return new Rect(save.x - w - Gap, Screen.height - h - Margin, w, h);
        }

        /// <summary>Convert screen-pixel rect into GuiScale logical space for BeginArea.</summary>
        public static Rect ToScaled(Rect screenRect)
        {
            var s = Mathf.Max(0.01f, GuiScale.Current);
            return new Rect(
                screenRect.x / s,
                screenRect.y / s,
                screenRect.width / s,
                screenRect.height / s);
        }

        public static bool Overlaps(Rect a, Rect b)
        {
            return a.xMin < b.xMax && a.xMax > b.xMin && a.yMin < b.yMax && a.yMax > b.yMin;
        }
    }
}
