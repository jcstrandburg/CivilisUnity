using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[TestFixture]
[Category("Logistics Tests")]
public class BasicLogisticsTests : NeolithicTest {

    protected LogisticsManager manager;

    public override void SetUp() {
        base.SetUp();
        manager = MakeTestComponent<LogisticsManager>();
    }

    [Test]
    public void TestGetClosestNetwork() {
        var network1 = MakePlainComponent<LogisticsNetwork>();
        var network2 = MakePlainComponent<LogisticsNetwork>();
        network1.transform.position = new Vector3(10, 0, 10);
        network2.transform.position = new Vector3(-10, 0, -10);

        //no networks added to manager yet, should get null
        Assert.IsNull(manager.FindNearestNetwork(new Vector3(1, 1, 1)));

        //add the first network, FindNearestNetwork should return network1
        network1.logisticsManager = manager;
        Assert.AreSame(network1, manager.FindNearestNetwork(new Vector3(1, 0, 1)));
        Assert.AreSame(network1, manager.FindNearestNetwork(new Vector3(-1, 0, -1)));

        //add the second network, one last test
        network2.logisticsManager = manager;
        Assert.AreSame(network1, manager.FindNearestNetwork(new Vector3(1, 0, 1)));
        Assert.AreSame(network2, manager.FindNearestNetwork(new Vector3(-1, 0, -1)));
    }

    [Test]
    public void TestNodeDistribution() {
        //create two networks and two nodes
        var network1 = MakePlainComponent<LogisticsNetwork>();
        var network2 = MakePlainComponent<LogisticsNetwork>();
        var node1 = MakePlainComponent<LogisticsNode>();
        var node2 = MakePlainComponent<LogisticsNode>();

        //make sure the networks start with no members
        Assert.AreEqual(0, network1.FindComponents<LogisticsNode>().Length);
        Assert.AreEqual(0, network2.FindComponents<LogisticsNode>().Length);

        //make sure network1 has node1 and network2 has none
        node1.LogisticsNetwork = network1;
        Assert.AreEqual(1, network1.NodeCount);
        Assert.AreSame(network1.transform, node1.transform.parent);
        Assert.AreEqual(1, network1.FindComponents<LogisticsNode>().Length);
        Assert.AreEqual(0, network2.FindComponents<LogisticsNode>().Length);

        //make sure each network has one node
        node2.LogisticsNetwork = network2;
        Assert.AreEqual(1, network2.NodeCount);
        Assert.AreEqual(1, network1.FindComponents<LogisticsNode>().Length);
        Assert.AreEqual(1, network2.FindComponents<LogisticsNode>().Length);
        Assert.AreSame(network2.transform, node2.transform.parent);

        //make sure that network1 has both nodes and network2 has none
        node2.LogisticsNetwork = network1;
        Assert.AreEqual(2, network1.FindComponents<LogisticsNode>().Length);
        Assert.AreEqual(0, network2.FindComponents<LogisticsNode>().Length);
        Assert.AreSame(network1.transform, node1.transform.parent);
        Assert.AreSame(network1.transform, node2.transform.parent);
    }

    [Test]
    public void TestRebuildNetwork() {
        //create one manager, two networks, and two nodes
        var manager = MakePlainComponent<LogisticsManager>();
        var network1 = MakePlainComponent<LogisticsNetwork>();
        var network2 = MakePlainComponent<LogisticsNetwork>();
        var node1 = MakePlainComponent<LogisticsNode>();
        var node2 = MakePlainComponent<LogisticsNode>();

        network1.logisticsManager = manager;
        network1.transform.position = new Vector3(0, 0, 0);
        network2.logisticsManager = manager;
        network2.transform.position = new Vector3(5, 0, 3);

        node1.LogisticsLogisticsManager = manager;
        node1.transform.position = new Vector3(1, 0, 1);
        node2.LogisticsLogisticsManager = manager;
        node2.transform.position = new Vector3(3, 0, 3);

        manager.RebuildNetworks();
        Assert.AreEqual(1, network1.FindComponents<LogisticsNode>().Length);
        Assert.AreSame(network1.transform, node1.transform.parent);
        Assert.AreEqual(1, network2.FindComponents<LogisticsNode>().Length);
        Assert.AreSame(network2.transform, node2.transform.parent);
    }

    [Test]
    public void TestNetworkInjectionWorks() {
        var network = MakeTestComponent<LogisticsNetwork>();
        Assert.AreSame(manager, network.logisticsManager);
        var node = MakeTestComponent<LogisticsNode>();
        Assert.AreSame(manager, node.LogisticsLogisticsManager);
        Assert.AreSame(network, node.LogisticsNetwork);
    }

    [Test]
    public void TestDestroyNetworkPreservesNodes() {
        var manager = MakeTestComponent<LogisticsManager>();
        var network = MakeTestComponent<LogisticsNetwork>();
        var node = MakeTestComponent<LogisticsNode>();

        manager.RebuildNetworks();
        Assert.IsNotNull(node.LogisticsNetwork);
        Assert.AreSame(network.transform, node.transform.parent);

        DestroyGameObject(network.gameObject);
        Assert.IsNotNull(node.gameObject);
        Assert.IsNotNull(node);
        Assert.IsNull(node.LogisticsNetwork);
    }

    [Test]
    public void TestDestroyNetworkRedistributesNodes() {
        var network1 = MakeTestComponent<LogisticsNetwork>();
        var network2 = MakeTestComponent<LogisticsNetwork>();
        var node1 = MakePlainComponent<LogisticsNode>();//don't inject yet

        network1.logisticsManager = manager;
        network1.transform.position = new Vector3(0, 0, 0);
        network2.logisticsManager = manager;
        network2.transform.position = new Vector3(5, 0, 3);

        node1.transform.position = new Vector3(0, 0, 0);
        node1.LogisticsLogisticsManager = manager;
        Assert.AreSame(network1, node1.LogisticsNetwork);

        //destroy the first network
        DestroyGameObject(network1.gameObject);
        Assert.AreSame(network2, node1.LogisticsNetwork);
        Assert.IsNotNull(node1.gameObject);
        Assert.IsNotNull(node1);

        //destroy network2 and make sure that the nodes all still exist
        DestroyGameObject(network2.gameObject);
        Assert.IsNotNull(node1.gameObject);
    }
}
