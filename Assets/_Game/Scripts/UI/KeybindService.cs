using UnityEngine;

namespace CleanEnergy.UI
{
    public enum RemappableAction
    {
        Pause,
        Speed1,
        Speed2,
        Speed3,
        Undo,
        Home
    }

    /// <summary>
    /// PlayerPrefs-backed remappable gameplay keybinds (F1–F10 forbidden).
    /// </summary>
    public static class KeybindService
    {
        public const string PrefPrefix = "ce_keybind_";

        public static KeyCode DefaultOf(RemappableAction action)
        {
            switch (action)
            {
                case RemappableAction.Pause: return KeyCode.Space;
                case RemappableAction.Speed1: return KeyCode.Alpha1;
                case RemappableAction.Speed2: return KeyCode.Alpha2;
                case RemappableAction.Speed3: return KeyCode.Alpha3;
                case RemappableAction.Undo: return KeyCode.Z;
                case RemappableAction.Home: return KeyCode.Home;
                default: return KeyCode.None;
            }
        }

        public static string PrefKey(RemappableAction action) => PrefPrefix + action;

        public static bool IsForbiddenDebugKey(KeyCode key)
        {
            return key >= KeyCode.F1 && key <= KeyCode.F10;
        }

        public static KeyCode ParseOrDefault(string stored, KeyCode fallback)
        {
            if (string.IsNullOrEmpty(stored))
            {
                return fallback;
            }

            if (!System.Enum.TryParse(stored, true, out KeyCode parsed)
                || parsed == KeyCode.None
                || IsForbiddenDebugKey(parsed))
            {
                return fallback;
            }

            return parsed;
        }

        public static KeyCode Get(RemappableAction action)
        {
            var fallback = DefaultOf(action);
            var stored = PlayerPrefs.GetString(PrefKey(action), fallback.ToString());
            return ParseOrDefault(stored, fallback);
        }

        public static bool TrySet(RemappableAction action, KeyCode key)
        {
            if (key == KeyCode.None || IsForbiddenDebugKey(key))
            {
                return false;
            }

            PlayerPrefs.SetString(PrefKey(action), key.ToString());
            PlayerPrefs.Save();
            return true;
        }

        public static void ClearPrefs()
        {
            foreach (RemappableAction action in System.Enum.GetValues(typeof(RemappableAction)))
            {
                PlayerPrefs.DeleteKey(PrefKey(action));
            }

            PlayerPrefs.Save();
        }
    }
}
