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
            { StringKeys.TutorialCameraHint, "Use WASD, right/middle-drag, Q/E, or scroll." },
            { StringKeys.TutorialOpenWaterTitle, "Open Water layer" },
            { StringKeys.TutorialOpenWaterHint, "Select Water in Terrain Debug view modes." },
            { StringKeys.TutorialPlaceWaterWheelTitle, "Build a Water Wheel" },
            { StringKeys.TutorialPlaceWaterWheelHint, "Place water_wheel next to a stream (auto-connects)." },
            { StringKeys.TutorialHireEngineerTitle, "Hire an Engineer" },
            { StringKeys.TutorialHireEngineerHint, "Use Hire Engineer on the Level panel." },
            { StringKeys.TutorialHireTechnicianTitle, "Hire a Technician" },
            { StringKeys.TutorialHireTechnicianHint, "Earn from water power, then hire a Technician." },
            { StringKeys.TutorialPlaceWindTitle, "Build Small Wind" },
            { StringKeys.TutorialPlaceWindHint, "Place small_wind (needs Engineer + Technician)." },
            { StringKeys.TutorialMeetDemandTitle, "Sustain village demand" },
            { StringKeys.TutorialMeetDemandHint, "Keep coverage high until Level progress completes." },
            { StringKeys.Level01Title, "Level 1: Light the Village" },
            { StringKeys.LevelProgress, "Progress" },
            { StringKeys.LevelObjEngineer, "Hire 1 Engineer" },
            { StringKeys.LevelObjWater, "Build water production" },
            { StringKeys.LevelObjTechnician, "Hire 1 Technician" },
            { StringKeys.LevelObjWind, "Build Small Wind" },
            { StringKeys.LevelObjCoverage, "Meet 95% village demand" },
            { StringKeys.HireEngineer, "Hire Engineer" },
            { StringKeys.HireTechnician, "Hire Technician" },
            { StringKeys.HireTechnicianRequiresWater, "Build water production first." },
            { StringKeys.BuildCost, "Build Cost" },
            { StringKeys.AutoConnectionCost, "Auto Grid Connection" },
            { StringKeys.TotalCost, "Total" }
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
            { StringKeys.TutorialCameraHint, "WASD, sağ/orta sürükle, Q/E veya tekerlek." },
            { StringKeys.TutorialOpenWaterTitle, "Su katmanını aç" },
            { StringKeys.TutorialOpenWaterHint, "Arazi hata ayıklama görünümlerinde Su'yu seç." },
            { StringKeys.TutorialPlaceWaterWheelTitle, "Su çarkı kur" },
            { StringKeys.TutorialPlaceWaterWheelHint, "water_wheel'i akarsu yanına yerleştir (otomatik bağlanır)." },
            { StringKeys.TutorialHireEngineerTitle, "Mühendis tut" },
            { StringKeys.TutorialHireEngineerHint, "Seviye panelinden Mühendis tut." },
            { StringKeys.TutorialHireTechnicianTitle, "Teknisyen tut" },
            { StringKeys.TutorialHireTechnicianHint, "Su enerjisinden gelir kazan, sonra Teknisyen tut." },
            { StringKeys.TutorialPlaceWindTitle, "Küçük rüzgar kur" },
            { StringKeys.TutorialPlaceWindHint, "small_wind yerleştir (Mühendis + Teknisyen gerekir)." },
            { StringKeys.TutorialMeetDemandTitle, "Köy talebini sürdür" },
            { StringKeys.TutorialMeetDemandHint, "Seviye ilerlemesi dolana kadar kapsamı yüksek tut." },
            { StringKeys.Level01Title, "Seviye 1: Köyü Aydınlat" },
            { StringKeys.LevelProgress, "İlerleme" },
            { StringKeys.LevelObjEngineer, "1 Mühendis oluştur" },
            { StringKeys.LevelObjWater, "Su üretimi kur" },
            { StringKeys.LevelObjTechnician, "1 Teknisyen oluştur" },
            { StringKeys.LevelObjWind, "Küçük rüzgar türbini kur" },
            { StringKeys.LevelObjCoverage, "Köy talebinin %95'ini karşıla" },
            { StringKeys.HireEngineer, "Mühendis tut" },
            { StringKeys.HireTechnician, "Teknisyen tut" },
            { StringKeys.HireTechnicianRequiresWater, "Önce su üretimi kur." },
            { StringKeys.BuildCost, "İnşa Maliyeti" },
            { StringKeys.AutoConnectionCost, "Otomatik Şebeke Bağlantısı" },
            { StringKeys.TotalCost, "Toplam" }
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
