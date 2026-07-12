using UnityEngine;
using UnityEngine.SceneManagement;

namespace CleanEnergy.Core
{
    /// <summary>
    /// Scene name constants and load helpers for menu / play flow.
    /// </summary>
    public static class SceneFlow
    {
        public const string BootstrapSceneName = "Bootstrap";
        public const string MainMenuSceneName = "MainMenu";
        public const string PlaySceneName = "Test_Terrain";

        public static void LoadBootstrap()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(BootstrapSceneName);
        }

        public static void LoadMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(MainMenuSceneName);
        }

        public static void LoadPlayScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(PlaySceneName);
        }

        public static void QuitGame()
        {
            Debug.Log("[SceneFlow] Quit requested.");
            Application.Quit();
        }

        /// <summary>Pure helper: build-order scene names.</summary>
        public static string[] BuildOrderSceneNames()
        {
            return new[] { BootstrapSceneName, MainMenuSceneName, PlaySceneName };
        }

        /// <summary>Pure helper: whether a scene name is the bootstrap entry.</summary>
        public static bool IsBootstrapScene(string sceneName)
        {
            return string.Equals(sceneName, BootstrapSceneName, System.StringComparison.Ordinal);
        }
    }
}
