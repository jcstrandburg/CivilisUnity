using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class StatManager : MonoBehaviour {

    public struct StatProfile {
        public string name;
        public bool persist;
        public bool monotonic;
    }

    private Dictionary<string, GameStat> stats = new Dictionary<string, GameStat>();
    private IStatPersistor persistor;
    private static StatManager instance;

    public static StatManager Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<StatManager>();
            }
            return instance;
        }
    }

    public static IStatPersistor DummyPersistor {
        get {
            return new StreamStatPersistor(Stream.Null);
        }
    }

    private IStatPersistor Persistor {
        set {
            SetPersistor(value);
        }
        get {
            if (persistor == null) {
                var path = Application.persistentDataPath + "/Saved Games/stats";
                var stream = new FileStream(path, FileMode.Create);
                SetPersistor(new StreamStatPersistor(stream));
            }
            return persistor;
        }
    }

    public void SetPersistor(IStatPersistor p) {
        persistor = p;
        foreach (var kvp in stats) {
            kvp.Value.SetPersistor(p);
        }
    }

    public GameStat Stat(string name) {
        if (stats.ContainsKey(name)) {
            return stats[name];
        }
        return null;
    }

    public void SetStats(IEnumerable<StatProfile> statProfiles) {
        stats.Clear();
        foreach (var profile in statProfiles) {
            var s = new GameStat(profile.name, profile.persist, profile.monotonic);
            s.SetPersistor(Persistor);
            stats[profile.name] = s;
        }
    }

    void Start() {
        TextAsset statsAsset = (TextAsset)Resources.Load("stats", typeof(TextAsset));
        var profiles = JsonUtility.FromJson<StatProfile[]>(statsAsset.text);
        SetStats(profiles);
    }
}
