using CleanEnergy.Save;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// IMGUI Save / Load / Delete controls for slots 1–3.
    /// </summary>
    public sealed class SaveLoadHudUI : MonoBehaviour
    {
        [SerializeField] private SaveLoadController saveLoadController;
        private int _selectedSlot = 1;
        private static readonly string[] SlotLabels = { "1", "2", "3" };

        public void Configure(SaveLoadController controller)
        {
            saveLoadController = controller;
        }

        private void OnGUI()
        {
            var controller = saveLoadController;
            if (controller == null)
            {
                return;
            }

            // #region agent log
            // One-shot per play session when drawing succeeds after prior NRE risk.
            // #endregion

            GuiScale.Apply();
            const float width = 220f;
            var x = Screen.width / GuiScale.Current - width - 12f;
            var y = Screen.height / GuiScale.Current - 150f;
            var began = false;
            try
            {
                GUILayout.BeginArea(new Rect(x, y, width, 140f), GUI.skin != null ? GUI.skin.box : GUIStyle.none);
                began = true;
                GUILayout.Label("Save / Load");
                var selectedIndex = Mathf.Clamp(_selectedSlot - 1, 0, SlotLabels.Length - 1);
                selectedIndex = GUILayout.SelectionGrid(selectedIndex, SlotLabels, 3);
                _selectedSlot = selectedIndex + 1;
                controller.SetActiveSlot(_selectedSlot);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Save"))
                {
                    controller.SaveSlot(_selectedSlot);
                }

                if (GUILayout.Button("Load"))
                {
                    controller.LoadSlot(_selectedSlot);
                }

                GUILayout.EndHorizontal();
                if (GUILayout.Button("Delete"))
                {
                    controller.DeleteSlot(_selectedSlot);
                }

                var msg = controller.LastMessage;
                if (!string.IsNullOrEmpty(msg))
                {
                    GUILayout.Label(msg);
                }
            }
            catch (System.Exception ex)
            {
                // #region agent log
                CleanEnergy.DebugTools.AgentDebugLog.Write(
                    "G",
                    "SaveLoadHudUI.OnGUI",
                    "caught_exception",
                    "{\"type\":\"" + ex.GetType().Name +
                    "\",\"msg\":\"" + (ex.Message ?? string.Empty).Replace("\"", "'") + "\"}");
                // #endregion
            }
            finally
            {
                if (began)
                {
                    GUILayout.EndArea();
                }
            }
        }
    }
}
