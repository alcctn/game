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
            if (saveLoadController == null)
            {
                return;
            }

            const float width = 220f;
            GUILayout.BeginArea(new Rect(Screen.width - width - 12f, Screen.height - 150f, width, 140f), GUI.skin.box);
            GUILayout.Label("Save / Load");
            _selectedSlot = GUILayout.SelectionGrid(_selectedSlot - 1, SlotLabels, 3) + 1;
            saveLoadController.SetActiveSlot(_selectedSlot);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
            {
                saveLoadController.SaveSlot(_selectedSlot);
            }

            if (GUILayout.Button("Load"))
            {
                saveLoadController.LoadSlot(_selectedSlot);
            }

            GUILayout.EndHorizontal();
            if (GUILayout.Button("Delete"))
            {
                saveLoadController.DeleteSlot(_selectedSlot);
            }

            if (!string.IsNullOrEmpty(saveLoadController.LastMessage))
            {
                GUILayout.Label(saveLoadController.LastMessage);
            }

            GUILayout.EndArea();
        }
    }
}
