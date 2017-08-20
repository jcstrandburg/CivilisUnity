using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Neolithica.ScriptableObjects;
using Tofu.Serialization;
using UnityEngine;

namespace Neolithica.MonoBehaviours {
    /// <summary>
    /// Manages values and persistence of stats
    /// </summary>
    [SavableMonobehaviour(27)]
    public class StatManager : MonoBehaviour {

        private Dictionary<string, GameStat> stats = new Dictionary<string, GameStat>();
        private StatPersistor persistor;

        /// <summary>Temporary storage for GameStats from a save game, to be loaded from on invokation of SetStats</summary>
        private ReadOnlyCollection<GameStat> statsFromSaveGame;

        //dummy stat persistor for testing purposes
        public static StatPersistor DummyPersistor => new StreamStatPersistor(Stream.Null);

        /// <summary>
        /// Stat persistor property, will create a default StreamStatPersistor if no other persistor is supplied
        /// </summary>
        private StatPersistor Persistor {
            set {
                SetPersistor(value);
            }
            get {
                if (persistor == null) {
                    var path = Application.persistentDataPath + "/Saved Games/stats";
                    SetPersistor(new FilePathStatPersistor(path));
                }
                return persistor;
            }
        }

        /// <summary>
        /// Sets the stat persistor
        /// </summary>
        /// <param name="p"></param>
        public void SetPersistor(StatPersistor p) {
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
        public IGameStat Stat(string name) {
            return stats.ContainsKey(name) ? stats[name] : null;
        }

        public ReadOnlyCollection<GameStat> Stats() {
            return stats.Values.ToReadOnlyCollection();
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

        public void RestoreStatsFromSaveGameIfPresent() {
            if (statsFromSaveGame != null) {
                Dictionary<string, GameStat> statsFromSaveGameByName = statsFromSaveGame.ToDictionary(stat => stat.Name);

                foreach (var stat in stats.Values.Where(stat => !stat.Persist && statsFromSaveGameByName.ContainsKey(stat.Name))) {
                    stat.SetValue(statsFromSaveGameByName[stat.Name].Value);
                }

                statsFromSaveGame = null;
            }
        }

        public void SetStatsFromSaveGame(IEnumerable<GameStat> stats) {
            statsFromSaveGame = stats.ToReadOnlyCollection();
        }

        /// <summary>
        /// Loads the default stat profiles from the resources folder
        /// </summary>
        public void LoadDefaultStats() {
            StatProfile[] profiles = Resources.LoadAll("Stats", typeof(StatProfile))
                .Select(r => (StatProfile) r)
                .ToArray();
            SetStats(profiles);
        }

        // Handles Awake event
        public void Awake() {
            LoadDefaultStats();
        }

        // Handles Start event
        public void Start() {
            RestoreStatsFromSaveGameIfPresent();
        }

        // Handles OnDestroy event
        public void OnDestroy() {
            //this is kinda hacky, probably need to rewrite the interface for persistor
            if (persistor != null) {
                persistor.Destroy();
            }
        }
    }
}
