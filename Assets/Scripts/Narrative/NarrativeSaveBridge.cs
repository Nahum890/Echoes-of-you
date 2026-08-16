using System.Collections.Generic;
using UnityEngine;
using Echoes.VN;

namespace Echoes.Narrative
{
    [System.Serializable]
    public class NarrativeSaveData
    {
        public int schemaVersion = 1;
        public List<FlagEntry> flags = new();
        public List<VariableEntry> variables = new();
        public List<string> inspectedInteractables = new();
        public string lastUpdated = "";
    }

    [System.Serializable]
    public class FlagEntry
    {
        public string key;
        public bool value;

        public FlagEntry() { }
        public FlagEntry(string k, bool v) { key = k; value = v; }
    }

    [System.Serializable]
    public class VariableEntry
    {
        public string key;
        public float value;

        public VariableEntry() { }
        public VariableEntry(string k, float v) { key = k; value = v; }
    }

    public static class NarrativeSaveBridge
    {
        const string SaveFileName = "save_narrative.json";

        static string SavePath => System.IO.Path.Combine(Application.persistentDataPath, SaveFileName);
        static string BackupPath => SavePath + ".bak";

        public static bool HasSave()
        {
            return System.IO.File.Exists(SavePath);
        }

        public static void Save()
        {
            var data = CollectRuntimeData();

            try
            {
                string json = JsonUtility.ToJson(data, true);
                System.IO.File.WriteAllText(SavePath, json);
                Debug.Log("[NarrativeSaveBridge] Saved narrative state to " + SavePath);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[NarrativeSaveBridge] Save failed: {e.Message}");
            }
        }

        public static NarrativeSaveData Load()
        {
            if (!System.IO.File.Exists(SavePath))
                return null;

            try
            {
                string json = System.IO.File.ReadAllText(SavePath);
                var data = JsonUtility.FromJson<NarrativeSaveData>(json);
                if (data == null)
                {
                    Debug.LogWarning("[NarrativeSaveBridge] Parsed null data, creating fresh.");
                    return new NarrativeSaveData();
                }
                return data;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[NarrativeSaveBridge] Load failed (corrupt?), backing up: {e.Message}");
                try
                {
                    if (System.IO.File.Exists(BackupPath))
                        System.IO.File.Delete(BackupPath);
                    System.IO.File.Move(SavePath, BackupPath);
                }
                catch { }
                return new NarrativeSaveData();
            }
        }

        public static void ApplyToRuntime(NarrativeSaveData data)
        {
            if (data == null) return;

            var flags = VN_EndingFlags.Instance;
            if (flags != null && data.flags != null)
            {
                for (int i = 0; i < data.flags.Count; i++)
                {
                    if (data.flags[i] != null && !string.IsNullOrEmpty(data.flags[i].key))
                        flags.SetFlag(data.flags[i].key, data.flags[i].value);
                }
            }

            var ctrl = NarrativeStateController.Instance;
            if (ctrl != null && data.variables != null)
            {
                var dict = new Dictionary<string, float>();
                for (int i = 0; i < data.variables.Count; i++)
                {
                    if (data.variables[i] != null && !string.IsNullOrEmpty(data.variables[i].key))
                        dict[data.variables[i].key] = data.variables[i].value;
                }
                ctrl.LoadVariables(dict);
            }

            if (data.inspectedInteractables != null)
            {
                var mem = MemorySystem.Instance;
                if (mem != null)
                    mem.LoadInspected(data.inspectedInteractables);
            }
        }

        public static void ClearSave()
        {
            try
            {
                if (System.IO.File.Exists(SavePath))
                    System.IO.File.Delete(SavePath);
            }
            catch { }
        }

        static NarrativeSaveData CollectRuntimeData()
        {
            var data = new NarrativeSaveData
            {
                lastUpdated = System.DateTime.Now.ToString("o")
            };

            var flags = VN_EndingFlags.Instance;
            if (flags != null)
            {
                var allFlags = flags.Flags;
                foreach (var kv in allFlags)
                    data.flags.Add(new FlagEntry(kv.Key, kv.Value));
            }

            var ctrl = NarrativeStateController.Instance;
            if (ctrl != null)
            {
                foreach (var kv in ctrl.AllVariables)
                    data.variables.Add(new VariableEntry(kv.Key, kv.Value));
            }

            var mem = MemorySystem.Instance;
            if (mem != null)
                data.inspectedInteractables = mem.GetInspectedList();

            return data;
        }
    }
}
