using System;
using System.Collections.Generic;
using Neolithica.Serialization.Utilities;
using NUnit.Framework;

namespace Neolithica.Test.Editor {
    [TestFixture]
    [Category("Serialization Tests")]
    public class TypeTesting : NeolithicTest {

        //Testing properties of collections to make sure assumptions made in Save/Load serialization codes works as intended
        [Test]
        public void CollectionTypeTesting() {
            Type t = typeof(Dictionary<string, string>);
            Type kvpss = typeof(KeyValuePair<string, string>);
            Assert.That(TypeSystem.IsCollectionType(t));
            Assert.AreEqual(kvpss, TypeSystem.GetElementType(t));
            Assert.That(!TypeSystem.IsCollectionType(kvpss));
            Assert.That(!TypeSystem.IsEnumerableType(kvpss));
            Assert.AreEqual(kvpss, TypeSystem.GetElementType(kvpss));
        }
    }
}
