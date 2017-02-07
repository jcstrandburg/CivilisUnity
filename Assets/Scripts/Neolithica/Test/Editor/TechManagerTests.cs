using Neolithica.ScriptableObjects;
using NUnit.Framework;

namespace Neolithica.Test.Editor {
    [TestFixture]
    [Category("Techmanager Tests")]
    public class TechManagerTests : NeolithicTest {

        /// <summary>
        /// Helper function to build techs for testing purposes
        /// </summary>
        private Technology MakeTech(string name, string displayName, string desc, string[] requires, float cost) {
            return Technology.Make(name, displayName, desc, requires, cost);
        }

        [Test]
        public void TestElligibilityAndPrereqs() {
            Technology[] techs = new Technology[] {
                MakeTech("0", "0", "", new string[] {}, 1.0f),
                MakeTech("1", "1", "", new string[] {}, 1.0f),
                MakeTech("2", "2", "", new string[] {}, 3.0f),
                MakeTech("3", "3", "", new string[] {"0"}, 1.0f),
                MakeTech("4", "4", "", new string[] {"0","1"}, 1.0f),
                MakeTech("5", "5", "", new string[] {"1"}, 1.0f),
                MakeTech("6", "6", "", new string[] {"2"}, 1.0f),
                MakeTech("7", "7", "", new string[] {"4","1"}, 1.0f),
            };
            TechManager tm = new TechManager();
            Technology[] eligibles;
            tm.LoadArray(techs);

            eligibles = tm.GetEligibleTechs();
            Assert.True(tm.PrereqsMet(techs[0]));
            Assert.True(tm.PrereqsMet(techs[1]));
            Assert.True(tm.PrereqsMet(techs[2]));
            Assert.False(tm.PrereqsMet(techs[3]));
            Assert.False(tm.PrereqsMet(techs[4]));
            Assert.False(tm.PrereqsMet(techs[5]));
            Assert.False(tm.PrereqsMet(techs[6]));
            Assert.False(tm.PrereqsMet(techs[7]));
            Assert.That(eligibles, Is.EquivalentTo(new Technology[] { techs[0], techs[1], techs[2] }));
        
            //buy some techs, assert that the returned costs are as expected, buying tech 0 should return zero the second time as it has already been purchased
            Assert.AreEqual(1.0f, tm.BuyTech("0"));
            Assert.AreEqual(0.0f, tm.BuyTech("0"));
            Assert.AreEqual(3.0f, tm.BuyTech("2"));

            eligibles = tm.GetEligibleTechs();
            Assert.True(tm.PrereqsMet(techs[0]));
            Assert.True(tm.PrereqsMet(techs[1]));
            Assert.True(tm.PrereqsMet(techs[2]));
            Assert.True(tm.PrereqsMet(techs[3]));
            Assert.False(tm.PrereqsMet(techs[4]));
            Assert.False(tm.PrereqsMet(techs[5]));
            Assert.True(tm.PrereqsMet(techs[6]));
            Assert.False(tm.PrereqsMet(techs[7]));
            Assert.That(eligibles, Is.EquivalentTo(new Technology[] { techs[1], techs[3], techs[6] }));

            tm.BuyTech("1");
            eligibles = tm.GetEligibleTechs();
            Assert.True(tm.PrereqsMet(techs[4]));
            Assert.False(tm.PrereqsMet(techs[7]));
            Assert.That(eligibles, Is.EquivalentTo(new Technology[] { techs[3], techs[4], techs[5], techs[6] }));

            tm.BuyTech("4");
            eligibles = tm.GetEligibleTechs();
            Assert.IsTrue(tm.PrereqsMet(techs[7]));
            Assert.That(eligibles, Is.EquivalentTo(new Technology[] { techs[3], techs[7], techs[5], techs[6] }));
        }
    }
}
