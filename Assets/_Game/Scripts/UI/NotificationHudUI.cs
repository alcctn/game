using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// Bottom-center IMGUI feed for short gameplay notifications.
    /// </summary>
    public sealed class NotificationHudUI : MonoBehaviour
    {
        [SerializeField] private NotificationController notificationController;

        public void Configure(NotificationController controller)
        {
            notificationController = controller;
        }

        private void OnGUI()
        {
            var service = notificationController != null ? notificationController.Service : null;
            if (service == null || service.Active.Count == 0)
            {
                return;
            }

            service.Prune(Time.unscaledTime);
            if (service.Active.Count == 0)
            {
                return;
            }

            const float width = 420f;
            var height = 24f + service.Active.Count * 22f;
            var x = (Screen.width - width) * 0.5f;
            var y = Screen.height - height - 16f;
            GUILayout.BeginArea(new Rect(x, y, width, height), GUI.skin.box);
            for (var i = 0; i < service.Active.Count; i++)
            {
                GUILayout.Label(service.Active[i].Message);
            }

            GUILayout.EndArea();
        }
    }
}
