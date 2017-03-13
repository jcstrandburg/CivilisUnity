using System;
using System.Collections.ObjectModel;
using AqlaSerializer;
using AqlaSerializer.Meta;
using Neolithica.ScriptableObjects;
using Neolithica.Serialization.Surrogates;
using NUnit.Framework;
using Tofu.Serialization;
using UnityEngine;

namespace Neolithica.Test.Editor {
    public class TypeModelTests : NeolithicTest {

        [Test]
        public void TestActionProfile() {
            ITypeModelBuilder builder = new MockModelBuilder();
            var container = new ActionProfileContainer {
                ID = Guid.NewGuid().ToString(),
                ActionProfile = Resources.Load<ActionProfile>(c_testActionProfileName)
            };

            ActionProfileContainer newContainer = builder.BuildRuntimeTypeModel().DeepClone(container);
            Assert.AreEqual(container.ID, newContainer.ID);
            Assert.AreEqual(container.ActionProfile, newContainer.ActionProfile);
        }

        [SerializableType]
        private class ActionProfileContainer {
            public string ID { get; set; }
            public ActionProfile ActionProfile { get; set; }
        }

        private class MockModelBuilder : TypeModelBuilderBase {
            public override ReadOnlyCollection<Type> GetSavableMonobehaviours() {
                return new[] { typeof(Savable), typeof(TestSavableBehaviour) }.ToReadOnlyCollection();
            }

            public override RuntimeTypeModel BuildRuntimeTypeModel() {
                var model = GetBaseModel();
                model[typeof(ActionProfile)].SetSurrogate(typeof(ActionProfileSurrogate));
                return model;
            }
        }

        private string c_testActionProfileName = "Hut";
    }
}