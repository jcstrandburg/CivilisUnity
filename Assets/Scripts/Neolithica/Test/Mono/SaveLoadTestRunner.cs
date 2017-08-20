using System;
using Neolithica.MonoBehaviours;
using Neolithica.ScriptableObjects;
using UnityEngine;

namespace Neolithica.Test.Mono {
    /// <summary>
    /// Tests that certain objects are getting serialized correctly given the standard object heirarchy.
    /// </summary>
    public class SaveLoadTestRunner : MonoBehaviour {
        public void Start() {

            stats.SetStats(new StatProfile[] {
                StatProfile.Make("teststat", false, false), 
            });
            stats.SetPersistor(StatManager.DummyPersistor);

            gameController.ForbiddenActions.Clear();
            gameController.ForbiddenActions.Add(CommandType.Butcher);

            var vers = GetType().Assembly.GetName().Version;
            expectedStatValue = vers.Revision;
            stats.Stat("teststat").Add(expectedStatValue);
            Debug.Log("Wat");
        }

        public void FixedUpdate() {
            switch (updateCount) {
                case 0:
                    Debug.Log("Saving state");
                    saverLoader.SaveGame("IntegrationTest");
                    break;
                case 1:
                    Debug.Log("Mutation Game State");
                    gameController.ForbiddenActions.Clear();
                    stats.Stat("teststat").Add(10);
                    break;
                case 2:
                    Debug.Log("Reloading Game State");
                    saverLoader.LoadGame("IntegrationTest");
                    break;
                case 3:
                    Debug.Log("Examining Game State");
                    if (stats.Stat("teststat").Value != expectedStatValue) {
                        Fail($"Expected teststat to have value {expectedStatValue}, get value {stats.Stat("teststat").Value}");
                        break;
                    }
                    if (gameController.ForbiddenActions.Count != 1 || !gameController.ForbiddenActions.Contains(CommandType.Butcher)) {
                        Fail(gameController.ForbiddenActions);
                        break;
                    }
                    Pass("Passed the test");
                    break;
            }
            updateCount++;
        }

        private void Pass(object msg) {
            Debug.Log(msg);
            Debug.Break();
        }

        private void Fail(object msg) {
            Debug.Log(msg);
            Debug.Break();
        }

        private StatManager stats => FindObjectOfType<StatManager>();
        private GameController gameController => FindObjectOfType<GameController>();
        private SaverLoader saverLoader => FindObjectOfType<SaverLoader>();

        private decimal expectedStatValue;
        private int updateCount = 0;
    }
}
