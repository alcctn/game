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
            { StringKeys.EscToResume, "Esc to resume" },
            { StringKeys.Tutorial, "Tutorial" },
            { StringKeys.TutorialCompleteTitle, "Tutorial complete" },
            { StringKeys.TutorialCompleteHint, "Keep supplying the village." },
            { StringKeys.TutorialCameraTitle, "Move the camera" },
            { StringKeys.TutorialCameraHint, "Use WASD, Q/E, or scroll wheel." },
            { StringKeys.TutorialOpenWaterTitle, "Open Water layer" },
            { StringKeys.TutorialOpenWaterHint, "Select Water in Terrain Debug view modes." },
            { StringKeys.TutorialPlaceWaterWheelTitle, "Build a Water Wheel" },
            { StringKeys.TutorialPlaceWaterWheelHint, "Place water_wheel next to a stream." },
            { StringKeys.TutorialPlacePowerLineTitle, "Connect with Power Line" },
            { StringKeys.TutorialPlacePowerLineHint, "Place a power_line near your buildings." },
            { StringKeys.TutorialOpenSolarTitle, "Open Solar layer" },
            { StringKeys.TutorialOpenSolarHint, "Select Solar in Terrain Debug view modes." },
            { StringKeys.TutorialUnlockSolarTitle, "Research Basic Solar" },
            { StringKeys.TutorialUnlockSolarHint, "Spend RP to unlock solar_basic." },
            { StringKeys.TutorialPlaceSolarTitle, "Build Small Solar" },
            { StringKeys.TutorialPlaceSolarHint, "Place a small_solar panel." },
            { StringKeys.TutorialPlaceBatteryTitle, "Unlock Storage & Battery" },
            { StringKeys.TutorialPlaceBatteryHint, "Research storage_basic, then place a battery near your network." },
            { StringKeys.TutorialMeetDemandTitle, "Sustain village demand" },
            { StringKeys.TutorialMeetDemandHint, "Keep coverage high for 10 ticks." }
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
            { StringKeys.EscToResume, "Devam için Esc" },
            { StringKeys.Tutorial, "Eğitim" },
            { StringKeys.TutorialCompleteTitle, "Eğitim tamam" },
            { StringKeys.TutorialCompleteHint, "Köye enerji vermeye devam et." },
            { StringKeys.TutorialCameraTitle, "Kamerayı hareket ettir" },
            { StringKeys.TutorialCameraHint, "WASD, Q/E veya kaydırma tekerleğini kullan." },
            { StringKeys.TutorialOpenWaterTitle, "Su katmanını aç" },
            { StringKeys.TutorialOpenWaterHint, "Arazi hata ayıklama görünümlerinde Su'yu seç." },
            { StringKeys.TutorialPlaceWaterWheelTitle, "Su çarkı kur" },
            { StringKeys.TutorialPlaceWaterWheelHint, "water_wheel'i akarsu yanına yerleştir." },
            { StringKeys.TutorialPlacePowerLineTitle, "Elektrik hattı bağla" },
            { StringKeys.TutorialPlacePowerLineHint, "Binaların yakınına power_line yerleştir." },
            { StringKeys.TutorialOpenSolarTitle, "Güneş katmanını aç" },
            { StringKeys.TutorialOpenSolarHint, "Arazi hata ayıklama görünümlerinde Güneş'i seç." },
            { StringKeys.TutorialUnlockSolarTitle, "Temel güneş araştır" },
            { StringKeys.TutorialUnlockSolarHint, "solar_basic açmak için AP harca." },
            { StringKeys.TutorialPlaceSolarTitle, "Küçük güneş paneli kur" },
            { StringKeys.TutorialPlaceSolarHint, "Bir small_solar paneli yerleştir." },
            { StringKeys.TutorialPlaceBatteryTitle, "Depolama ve batarya" },
            { StringKeys.TutorialPlaceBatteryHint, "storage_basic araştır, sonra ağa yakın batarya kur." },
            { StringKeys.TutorialMeetDemandTitle, "Köy talebini sürdür" },
            { StringKeys.TutorialMeetDemandHint, "Kapsamı 10 tick yüksek tut." }
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
