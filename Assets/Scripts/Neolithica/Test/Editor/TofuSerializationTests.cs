using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using AqlaSerializer.Meta;
using NUnit.Framework;
using Tofu.Serialization;
using Tofu.Serialization.Surrogates;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Neolithica.Test.Editor {
    [TestFixture]
    [Category("Serialization Tests")]
    public class TofuSerializationTests : NeolithicTest {

        [SetUp]
        public override void SetUp() {
            base.SetUp();
            Assert.NotNull(s_testPrefab);
        }

        [Test]
        public void BasicSaveTestMocked() {
            var builder = new MockModelBuilder();
            BasicTestWithModelBuilder(builder, new[] { s_testPrefab });
        }

        [Test]
        public void BasicSaveTestAttributeBased() {
            var builder = new AttributeBasedTypeModelBuilder();
            BasicTestWithModelBuilder(builder, new [] { s_testPrefab });
        }

        [Test]
        [TestCaseSource(typeof(PrefabProvider), "GetEnumerator")]
        public void SaveAllPrefabs(GameObject prefab) {
            TestSerializePrefab(prefab, s_allSavablePrefabs);
        }

        [Test]
        public void MonoBehaviourReferencesAreSerialized() {
            string id1 = Guid.NewGuid().ToString();
            string id2 = Guid.NewGuid().ToString();
            GameObject object1 = Object.Instantiate(s_testPrefab);
            object1.name = id1;
            GameObject object2 = Object.Instantiate(s_testPrefab);
            object2.name = id2;
            TestSavableBehaviour behaviour1 = object1.GetComponent<TestSavableBehaviour>();
            TestSavableBehaviour behaviour2 = object2.GetComponent<TestSavableBehaviour>();

            behaviour1.monobehaviourReference = behaviour2;

            var builder = new MockModelBuilder();
            SaveGame saveGame = SaveGamePacker.PackSaveGame(new[] { object1, object2 }, builder.GetSavableMonobehaviours());
            SerializeAndUnserialize(saveGame, builder, s_allSavablePrefabs);

            GameObject newObject1 = GameObject.Find(id1);
            GameObject newObject2 = GameObject.Find(id2);

            Assert.AreEqual(newObject2.GetComponent<TestSavableBehaviour>(),
                newObject1.GetComponent<TestSavableBehaviour>().monobehaviourReference);
        }

        [Test]
        public void GameObjectReferencesAreSerialized() {
            string id1 = "object1";
            string id2 = "object2";
            GameObject object1 = Object.Instantiate(s_testPrefab);
            object1.name = id1;
            GameObject object2 = Object.Instantiate(s_testPrefab);
            object2.name = id2;
            TestSavableBehaviour behaviour1 = object1.GetComponent<TestSavableBehaviour>();
            TestSavableBehaviour behaviour2 = object2.GetComponent<TestSavableBehaviour>();

            behaviour1.gameObjectReferece = object2;
            behaviour2.referenceContainer = new TestReferenceContainer { gameObjectReferece = object1 };

            var builder = new MockModelBuilder();
            SaveGame saveGame = SaveGamePacker.PackSaveGame(new[] { object1, object2 }, builder.GetSavableMonobehaviours());
            SerializeAndUnserialize(saveGame, builder, s_allSavablePrefabs);

            GameObject newObject1 = GameObject.Find(id1);
            GameObject newObject2 = GameObject.Find(id2);

            Assert.AreEqual(newObject2, newObject1.GetComponent<TestSavableBehaviour>().gameObjectReferece);
            Assert.AreEqual(newObject1, newObject2.GetComponent<TestSavableBehaviour>().referenceContainer.gameObjectReferece);
        }

        private SaveGame SerializeAndUnserialize(SaveGame saveGame, ITypeModelBuilder builder, IEnumerable<GameObject> prefabs) {
            var serializer = new GameSerializer(prefabs, builder);

            byte[] output;
            using (var s = new MemoryStream()) {
                serializer.Serialize(s, saveGame);
                output = s.ToArray();
            }

            foreach (var gameObject in saveGame.GameObjects)
                Object.DestroyImmediate(gameObject);

            SaveGame deserialized;
            using (var s = new MemoryStream(output)) {
                deserialized = serializer.Deserialize<SaveGame>(s);
            }

            Assert.NotNull(deserialized);
            return deserialized;
        }

        private void TestSerializePrefab(GameObject prefab, IEnumerable<GameObject> allPrefabs) {
            var builder = new AttributeBasedTypeModelBuilder();
            string testName = Guid.NewGuid().ToString();
            GameObject instantiated = Object.Instantiate(prefab);
            instantiated.name = testName;

            SaveGame saveGame = SaveGamePacker.PackSaveGame(new[] { instantiated }, builder.GetSavableMonobehaviours());
            SerializeAndUnserialize(saveGame, builder, allPrefabs);

            GameObject testObject = GameObject.Find(testName);
            Assert.NotNull(testObject);
            Assert.AreEqual(testName, testObject.name);
        }

        private void BasicTestWithModelBuilder(ITypeModelBuilder builder, IEnumerable<GameObject> prefabs) {
            GameObject instantiated = Object.Instantiate(s_testPrefab);
            string testName = Guid.NewGuid().ToString();
            string testValue = Guid.NewGuid().ToString();
            Vector3 testPosition = new Vector3(10, 20, 30);

            instantiated.name = testName;
            instantiated.GetComponent<TestSavableBehaviour>().savableValue = testValue;
            instantiated.transform.position = testPosition;

            SaveGame saveGame = SaveGamePacker.PackSaveGame(new[] { instantiated }, builder.GetSavableMonobehaviours());
            SerializeAndUnserialize(saveGame, builder, prefabs);

            GameObject testObject = GameObject.Find(testName);
            Assert.NotNull(testObject);
            Assert.AreEqual(testName, testObject.name);
            Assert.NotNull(testObject.GetComponent<TestSavableBehaviour>());
            Assert.AreEqual(testValue, testObject.GetComponent<TestSavableBehaviour>().savableValue);
            Assert.AreEqual(testPosition, testObject.transform.position);
        }

        private static GameObject s_testPrefab = Resources.Load<GameObject>("TestSavable");
        private static List<GameObject> s_allSavablePrefabs = Resources.LoadAll<Savable>("")
            .Where(savable => savable.PrefabId != null).Select(savable => savable.gameObject).ToList();

        private class MockModelBuilder : TypeModelBuilderBase {
            public override ReadOnlyCollection<Type> GetSavableMonobehaviours() {
                return new[] { typeof(Savable), typeof(TestSavableBehaviour) }.ToReadOnlyCollection();
            }

            public override RuntimeTypeModel BuildRuntimeTypeModel() {
                var model = GetBaseModel();
                model[typeof(MonoBehaviour)].AddSubType(1, typeof(Savable));
                model[typeof(Savable)].SetSurrogate(typeof(SavableSurrogate));
                model[typeof(MonoBehaviour)].AddSubType(2, typeof(TestSavableBehaviour));
                model[typeof(TestSavableBehaviour)].SetSurrogate(typeof(TestSavableBehaviourSurrogate));
                return model;
            }
        }

        private class PrefabProvider {
            public IEnumerator<TestCaseData> GetEnumerator() {
                return s_allSavablePrefabs.Select(prefab => new TestCaseData(prefab).SetName(prefab.name)).GetEnumerator();
            }
        }
    }
}
