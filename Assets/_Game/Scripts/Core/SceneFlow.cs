using UnityEngine;
using UnityEngine.SceneManagement;

namespace CleanEnergy.Core
{
    /// <summary>
    /// Scene name constants and load helpers for menu / play flow.
    /// </summary>
    public static class SceneFlow
    {
        public const string MainMenuSceneName = "MainMenu";
        public const string PlaySceneName = "Test_Terrain";

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
    }
}
