using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(StatManager))]
public class StatmanagerEditor : Editor {
    public override void OnInspectorGUI() {
        StatManager my = (StatManager)target;

        foreach (var stat in my.stats.Values) {
            EditorGUILayout.LabelField(stat.name);
            EditorGUILayout.FloatField((float)stat.Value);
        }
    }
}
#endif

/// <summary>
/// Manages values and persistence of stats
/// </summary>
public class StatManager : MonoBehaviour {

    [SerializeField]
    public Dictionary<string, GameStat> stats = new Dictionary<string, GameStat>();
    [DontSaveField]
    [NonSerialized]
    private IStatPersistor persistor;

    //dummy stat persistor for testing purposes
    public static IStatPersistor DummyPersistor {
        get {
            return new StreamStatPersistor(Stream.Null);
        }
    }

    /// <summary>
    /// Stat persistor property, will create a default StreamStatPersistor if no other persistor is supplied
    /// </summary>
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

    /// <summary>
    /// Sets the stat persistor
    /// </summary>
    /// <param name="p"></param>
    public void SetPersistor(IStatPersistor p) {
        persistor = p;
        foreach (var kvp in stats) {
            kvp.Value.SetPersistor(p);
        }
    }

    /// <summary>
    /// Gets the GameStat object with the given name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public GameStat Stat(string name) {
        if (stats.ContainsKey(name)) {
            return stats[name];
        }
        return null;
    }

    /// <summary>
    /// Creates GameStat objects for all of the given StatProfile objects
    /// </summary>
    /// <param name="statProfiles"></param>
    public void SetStats(IEnumerable<StatProfile> statProfiles) {
        stats.Clear();
        foreach (var profile in statProfiles) {
            var s = new GameStat(profile.statname, profile.persist, profile.monotonic);
            s.SetPersistor(Persistor);
            stats[profile.statname] = s;
        }
    }

    /// <summary>
    /// Loads the default stat profiles from the resources folder
    /// </summary>
    public void LoadDefaultStats() {
        UnityEngine.Object[] allstats = Resources.LoadAll("Stats", typeof(StatProfile));
        StatProfile[] profiles = (from r in allstats select (StatProfile)r).ToArray();
        SetStats(profiles);
    }

    // Handles Awake event
    public void Awake() {
        LoadDefaultStats();
    }

    // Handles OnDestroy event
    public void OnDestroy() {
        //this is kinda hacky, probably need to rewrite the interface for persistor
        if (persistor != null) {
            persistor.Destroy();
        }
    }
}
