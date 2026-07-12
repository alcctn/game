using CleanEnergy.Save;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// IMGUI Save / Load / Delete controls for slots 1–3 (absolute GUI, no GUILayout).
    /// </summary>
    public sealed class SaveLoadHudUI : MonoBehaviour
    {
        [SerializeField] private SaveLoadController saveLoadController;
        private int _selectedSlot = 1;
        private static bool _loggedFail;

        public void Configure(SaveLoadController controller)
        {
            saveLoadController = controller;
        }

        private void OnGUI()
        {
            var controller = saveLoadController;
            // Unity fake-null check for destroyed MonoBehaviours
            if (!controller)
            {
                return;
            }

            if (Event.current == null || GUI.skin == null)
            {
                // #region agent log
                if (!_loggedFail)
                {
                    _loggedFail = true;
                    CleanEnergy.DebugTools.AgentDebugLog.Write(
                        "G",
                        "SaveLoadHudUI.OnGUI",
                        "early_exit_skin_or_event",
                        "{\"eventNull\":" + (Event.current == null ? "true" : "false") +
                        ",\"skinNull\":" + (GUI.skin == null ? "true" : "false") + "}");
                }
                // #endregion
                return;
            }

            var step = 0;
            try
            {
                step = 1;
                GuiScale.Apply();
                var scale = Mathf.Max(0.01f, GuiScale.Current);
                const float width = 220f;
                const float height = 140f;
                var x = Screen.width / scale - width - 12f;
                var y = Screen.height / scale - height - 12f;
                var area = new Rect(x, y, width, height);

                step = 2;
                GUI.Box(area, "Save / Load");

                step = 3;
                var slotY = y + 28f;
                var slotW = (width - 24f) / 3f;
                for (var i = 0; i < 3; i++)
                {
                    var slot = i + 1;
                    var rect = new Rect(x + 8f + i * (slotW + 4f), slotY, slotW, 24f);
                    var label = slot == _selectedSlot ? $"[{slot}]" : slot.ToString();
                    if (GUI.Button(rect, label))
                    {
                        _selectedSlot = slot;
                        controller.SetActiveSlot(_selectedSlot);
                    }
                }

                step = 4;
                var rowY = slotY + 32f;
                var btnW = (width - 24f) / 3f;
                if (GUI.Button(new Rect(x + 8f, rowY, btnW, 28f), "Save"))
                {
                    controller.SaveSlot(_selectedSlot);
                }

                if (GUI.Button(new Rect(x + 12f + btnW, rowY, btnW, 28f), "Load"))
                {
                    controller.LoadSlot(_selectedSlot);
                }

                if (GUI.Button(new Rect(x + 16f + 2f * btnW, rowY, btnW, 28f), "Delete"))
                {
                    controller.DeleteSlot(_selectedSlot);
                }

                step = 5;
                var msg = controller.LastMessage;
                if (!string.IsNullOrEmpty(msg))
                {
                    GUI.Label(new Rect(x + 8f, rowY + 36f, width - 16f, 40f), msg);
                }
            }
            catch (System.Exception ex)
            {
                // #region agent log
                if (!_loggedFail)
                {
                    _loggedFail = true;
                    CleanEnergy.DebugTools.AgentDebugLog.Write(
                        "G",
                        "SaveLoadHudUI.OnGUI",
                        "caught_exception",
                        "{\"step\":" + step +
                        ",\"type\":\"" + ex.GetType().Name +
                        "\",\"msg\":\"" + (ex.Message ?? string.Empty).Replace("\"", "'") +
                        "\",\"build\":\"abs-gui-v2\"}");
                }
                // #endregion
                enabled = false;
            }
        }

        private void OnDisable()
        {
            _loggedFail = false;
        }
    }
}
