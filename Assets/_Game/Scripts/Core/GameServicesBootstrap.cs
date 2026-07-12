using CleanEnergy.Audio;
using CleanEnergy.Save;
using CleanEnergy.UI;
using UnityEngine;

namespace CleanEnergy.Core
{
    /// <summary>
    /// DDOL bootstrap: applies settings/music, ensures save path, loads MainMenu.
    /// </summary>
    public sealed class GameServicesBootstrap : MonoBehaviour
    {
        [SerializeField] private bool loadMainMenuOnStart = true;

        private static GameServicesBootstrap _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            ApplyServices();
        }

        private void Start()
        {
            if (loadMainMenuOnStart
                && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
                == SceneFlow.BootstrapSceneName)
            {
                SceneFlow.LoadMainMenu();
            }
        }

        public static void ApplyServices()
        {
            if (_instance != null && _instance.GetComponent<MusicService>() == null)
            {
                _instance.gameObject.AddComponent<MusicService>();
            }

            GameServices.RegisterCloudSaveStore(new LocalCloudSaveStore());
            SettingsService.ApplyAll();
            EnsureSaveDirectory();
        }

        private static void EnsureSaveDirectory()
        {
            var localRoot = System.IO.Path.Combine(
                UnityEngine.Application.persistentDataPath, "saves");
            var resolved = GameServices.CloudSaveStore.ResolveSaveDirectory(localRoot);
            var service = new SaveGameService(resolved);
            if (!System.IO.Directory.Exists(service.SlotDirectory))
            {
                System.IO.Directory.CreateDirectory(service.SlotDirectory);
            }
        }
    }
}
