using System;
using System.IO;
using UnityEngine;

namespace CleanEnergy.Save
{
    /// <summary>
    /// Lightweight header for main-menu slot rows (no full Apply).
    /// </summary>
    public sealed class SlotSaveSummary
    {
        public string ScenarioId { get; set; } = string.Empty;
        public float Money { get; set; }
        public int TickIndex { get; set; }
    }

    /// <summary>
    /// Reads/writes GameSaveData JSON to slot files (slot1–slot3).
    /// </summary>
    public sealed class SaveGameService
    {
        public const string DefaultSlotFileName = "slot1.json";
        public const int MinSlot = 1;
        public const int MaxSlot = 3;

        public string SlotDirectory { get; }
        public int ActiveSlot { get; private set; } = 1;
        public string SlotPath => GetSlotPath(ActiveSlot);

        public SaveGameService(string slotDirectory = null)
        {
            SlotDirectory = string.IsNullOrEmpty(slotDirectory)
                ? Path.Combine(Application.persistentDataPath, "saves")
                : slotDirectory;
        }

        public static string SlotFileName(int slot)
        {
            var clamped = ClampSlot(slot);
            return $"slot{clamped}.json";
        }

        public static int ClampSlot(int slot)
        {
            return Mathf.Clamp(slot, MinSlot, MaxSlot);
        }

        public string GetSlotPath(int slot)
        {
            return Path.Combine(SlotDirectory, SlotFileName(slot));
        }

        public void SetActiveSlot(int slot)
        {
            ActiveSlot = ClampSlot(slot);
        }

        public string ToJson(GameSaveData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            data.saveVersion = GameSaveData.CurrentVersion;
            return JsonUtility.ToJson(data, true);
        }

        public GameSaveData FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            return JsonUtility.FromJson<GameSaveData>(json);
        }

        public bool SlotExists() => SlotExists(ActiveSlot);

        public bool SlotExists(int slot) => File.Exists(GetSlotPath(slot));

        public bool TryReadSummary(int slot, out SlotSaveSummary summary)
        {
            summary = null;
            var path = GetSlotPath(slot);
            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                var json = File.ReadAllText(path);
                var data = FromJson(json);
                if (data == null)
                {
                    return false;
                }

                summary = new SlotSaveSummary
                {
                    ScenarioId = string.IsNullOrEmpty(data.scenarioId) ? "?" : data.scenarioId,
                    Money = data.money,
                    TickIndex = data.tickIndex
                };
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Save] TryReadSummary failed for slot{ClampSlot(slot)}: {ex.Message}");
                return false;
            }
        }

        public void Write(GameSaveData data) => Write(ActiveSlot, data);

        public void Write(int slot, GameSaveData data)
        {
            Directory.CreateDirectory(SlotDirectory);
            var path = GetSlotPath(slot);
            var json = ToJson(data);
            File.WriteAllText(path, json);
            Debug.Log($"[Save] Wrote {path}");
        }

        public GameSaveData Read() => Read(ActiveSlot);

        public GameSaveData Read(int slot)
        {
            var path = GetSlotPath(slot);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[Save] No save at {path}");
                return null;
            }

            var json = File.ReadAllText(path);
            var data = FromJson(json);
            if (data == null)
            {
                Debug.LogError("[Save] Failed to parse save JSON.");
            }

            return data;
        }

        public bool DeleteSlot(int slot)
        {
            var path = GetSlotPath(slot);
            if (!File.Exists(path))
            {
                return false;
            }

            File.Delete(path);
            Debug.Log($"[Save] Deleted {path}");
            return true;
        }

        public bool DeleteSlot() => DeleteSlot(ActiveSlot);
    }
}
