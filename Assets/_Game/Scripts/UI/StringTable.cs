using System.Collections.Generic;

namespace CleanEnergy.UI
{
    /// <summary>
    /// EN default + TR string table for HUD / menu labels.
    /// </summary>
    public static class StringTable
    {
        private static readonly Dictionary<string, string> En = new Dictionary<string, string>
        {
            { StringKeys.Build, "Build" },
            { StringKeys.Research, "Research" },
            { StringKeys.Settings, "Settings" },
            { StringKeys.Pause, "Paused" },
            { StringKeys.Continue, "Continue" },
            { StringKeys.Resume, "Resume (1x)" },
            { StringKeys.MainMenu, "Main Menu" },
            { StringKeys.NewGame, "New Game" },
            { StringKeys.Quit, "Quit" },
            { StringKeys.Back, "Back" },
            { StringKeys.Save, "Save" },
            { StringKeys.DeleteSlot, "Delete Slot" },
            { StringKeys.EscToResume, "Esc to resume" }
        };

        private static readonly Dictionary<string, string> Tr = new Dictionary<string, string>
        {
            { StringKeys.Build, "İnşa" },
            { StringKeys.Research, "Araştırma" },
            { StringKeys.Settings, "Ayarlar" },
            { StringKeys.Pause, "Duraklatıldı" },
            { StringKeys.Continue, "Devam" },
            { StringKeys.Resume, "Devam (1x)" },
            { StringKeys.MainMenu, "Ana Menü" },
            { StringKeys.NewGame, "Yeni Oyun" },
            { StringKeys.Quit, "Çıkış" },
            { StringKeys.Back, "Geri" },
            { StringKeys.Save, "Kaydet" },
            { StringKeys.DeleteSlot, "Yuvayı Sil" },
            { StringKeys.EscToResume, "Devam için Esc" }
        };

        public static string Get(string key)
        {
            return Get(key, SettingsService.Locale);
        }

        public static string Get(string key, string locale)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            var table = SettingsService.NormalizeLocale(locale) == "tr" ? Tr : En;
            if (table.TryGetValue(key, out var value))
            {
                return value;
            }

            if (En.TryGetValue(key, out var fallback))
            {
                return fallback;
            }

            return key;
        }
    }
}
