using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

public class StatManager : MonoBehaviour {

    [SerializeField]
    private Dictionary<string, GameStat> stats = new Dictionary<string, GameStat>();
    [DontSaveField]
    [NonSerialized]
    private IStatPersistor persistor;


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
            var s = new GameStat(profile.statname, profile.persist, profile.monotonic);
            s.SetPersistor(Persistor);
            stats[profile.statname] = s;
        }
    }

    void Start() {
        UnityEngine.Object[] allstats = Resources.LoadAll("Stats", typeof(StatProfile));
        StatProfile[] profiles = (from r in allstats select (StatProfile)r).ToArray();
        SetStats(profiles);
    }

    //this is kinda hacky, probably need to rewrite the interface for persistor
    public void OnDisable() {
        persistor.Destroy();
    }
}
