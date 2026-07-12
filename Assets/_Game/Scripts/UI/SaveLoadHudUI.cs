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

        public void Configure(SaveLoadController controller)
        {
            saveLoadController = controller;
        }

        private void OnGUI()
        {
            var controller = saveLoadController;
            if (!controller || Event.current == null || GUI.skin == null)
            {
                return;
            }

            try
            {
                GuiScale.Apply();
                var scale = Mathf.Max(0.01f, GuiScale.Current);
                const float width = 220f;
                const float height = 140f;
                var x = Screen.width / scale - width - 12f;
                var y = Screen.height / scale - height - 12f;
                var area = new Rect(x, y, width, height);
                ImguiHitTest.Register(area);

                GUI.Box(area, "Save / Load");

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

                var msg = controller.LastMessage;
                if (!string.IsNullOrEmpty(msg))
                {
                    GUI.Label(new Rect(x + 8f, rowY + 36f, width - 16f, 40f), msg);
                }
            }
            catch (System.Exception)
            {
                enabled = false;
            }
        }
    }
}
