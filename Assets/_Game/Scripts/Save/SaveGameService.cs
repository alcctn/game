using System;
using System.IO;
using UnityEngine;

namespace CleanEnergy.Save
{
    /// <summary>
    /// Reads/writes GameSaveData JSON to a single slot file.
    /// </summary>
    public sealed class SaveGameService
    {
        public const string DefaultSlotFileName = "slot1.json";

        public string SlotDirectory { get; }
        public string SlotPath => Path.Combine(SlotDirectory, DefaultSlotFileName);

        public SaveGameService(string slotDirectory = null)
        {
            SlotDirectory = string.IsNullOrEmpty(slotDirectory)
                ? Path.Combine(Application.persistentDataPath, "saves")
                : slotDirectory;
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

        public bool SlotExists() => File.Exists(SlotPath);

        public void Write(GameSaveData data)
        {
            Directory.CreateDirectory(SlotDirectory);
            var json = ToJson(data);
            File.WriteAllText(SlotPath, json);
            Debug.Log($"[Save] Wrote {SlotPath}");
        }

        public GameSaveData Read()
        {
            if (!SlotExists())
            {
                Debug.LogWarning($"[Save] No save at {SlotPath}");
                return null;
            }

            var json = File.ReadAllText(SlotPath);
            var data = FromJson(json);
            if (data == null)
            {
                Debug.LogError("[Save] Failed to parse save JSON.");
            }

            return data;
        }
    }
}
