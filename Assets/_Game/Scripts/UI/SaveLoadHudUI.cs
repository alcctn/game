using CleanEnergy.Save;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// IMGUI Save / Load controls for the single prototype slot.
    /// </summary>
    public sealed class SaveLoadHudUI : MonoBehaviour
    {
        [SerializeField] private SaveLoadController saveLoadController;

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

            const float width = 160f;
            GUILayout.BeginArea(new Rect(Screen.width - width - 12f, Screen.height - 90f, width, 80f), GUI.skin.box);
            GUILayout.Label("Save / Load");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
            {
                saveLoadController.SaveSlot();
            }

            if (GUILayout.Button("Load"))
            {
                saveLoadController.LoadSlot();
            }

            GUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty(saveLoadController.LastMessage))
            {
                GUILayout.Label(saveLoadController.LastMessage);
            }

            GUILayout.EndArea();
        }
    }
}
