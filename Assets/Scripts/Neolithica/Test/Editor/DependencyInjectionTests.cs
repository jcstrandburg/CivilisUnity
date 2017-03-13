using System.Collections.Generic;
using Neolithica.DependencyInjection;
using Neolithica.MonoBehaviours.Logistics;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Neolithica.Test.Editor {
    public class DependencyInjectionTests : NeolithicTest {

        [Test]
        public void TestMainGameFactoryInstatiable() {
            new MainGameFactory(null);
        }

        [Test]
        public void TestSingletonBehaviours() {
            var resolver = new SingletonMonoBehaviourResolver<InjectableBehaviour>(null);
            Assert.IsNull(resolver.Resolve());

            var testBehaviour = MakeTestComponent<InjectableBehaviour>();
            Assert.AreEqual(testBehaviour, resolver.Resolve());

            var testBehaviour2 = MakeTestComponent<InjectableBehaviour>();
            Object.DestroyImmediate(testBehaviour);
            Assert.AreEqual(testBehaviour2, resolver.Resolve());
        }

        [Test]
        public void TestGameFactorySingletonReplacesOnDestruction() {
            GameFactoryBase factory = new TestGameFactory();
            var injectable1 = MakePlainComponent<InjectableBehaviour>();
            var testBehaviour = MakePlainComponent<InjectBehaviour>();


            factory.InjectObject(testBehaviour);
            Assert.AreEqual(injectable1, testBehaviour.InjectMe);

            var injectable2 = MakePlainComponent<InjectableBehaviour>();
            Object.DestroyImmediate(injectable1);

            factory.InjectObject(testBehaviour);
            Assert.AreEqual(injectable2, testBehaviour.InjectMe);
        }

        [Test]
        public void TestLogisticsInjection() {
            var logisticsManager = MakeTestComponent<LogisticsManager>();
            var logisticsnode = MakeTestComponent<LogisticsNode>();
            Factory.InjectObject(logisticsnode);
            Assert.AreSame(logisticsManager, logisticsnode.LogisticsLogisticsManager);
        }

        private class InjectBehaviour : MonoBehaviour {
            [Inject] public InjectableBehaviour InjectMe { get; set; }
        }

        private class InjectableBehaviour : MonoBehaviour {}

        private class TestGameFactory : GameFactoryBase {
            public TestGameFactory() : base(s_dependencyResolvers) {}

            private static readonly List<IDependencyResolver> s_dependencyResolvers = new List<IDependencyResolver> {
                new SingletonMonoBehaviourResolver<InjectableBehaviour>()
            };
        }
    }
}
