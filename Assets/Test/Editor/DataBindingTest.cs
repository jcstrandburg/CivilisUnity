using NUnit.Framework;
using UnityEngine;

[TestFixture]
[Category("Data Binding Tests")]
public class DataBindingTest : NeolithicTest {

    [Test]
	public void TestExplicitBindings() {
        var dbs = MakePlainComponent<DataBindingSource>();

        float jimmy = 0.0f;
        dbs.AddBinding("jimmy", () => jimmy, (object val) => jimmy = (float)val);
        int fleur = -1;
        dbs.AddBinding("fleur", () => fleur, (object val) => fleur = 2*(int)val);
        string kat = "woof";
        dbs.AddBinding("kat", () => kat, (object val) => kat = (string)val);

        dbs.SetValue("jimmy", 3.0f);
        Assert.AreEqual(3.0f, dbs.GetValue("jimmy"));
    }
}
