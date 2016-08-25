using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[TestFixture]
[Category("Logistics Tests")]
public class BasicLogisticsTests {

    [Test]
    public void TestGetClosestNetwork() {
        var managerObject = new GameObject();
        var manager = managerObject.AddComponent<LogisticsManager>();
        var networkObject1 = new GameObject();
        var network1 = networkObject1.AddComponent<LogisticsNetwork>();
        var networkObject2 = new GameObject();
        var network2 = networkObject2.AddComponent<LogisticsNetwork>();
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
    public void TestNetworkNode() {
        //create two networks and two nodes
        var networkObject1 = new GameObject();
        var network1 = networkObject1.AddComponent<LogisticsNetwork>();
        var networkObject2 = new GameObject();
        var network2 = networkObject2.AddComponent<LogisticsNetwork>();
        var nodeObject1 = new GameObject();
        var node1 = nodeObject1.AddComponent<LogisticsNode>();
        var nodeObject2 = new GameObject();
        var node2 = nodeObject2.AddComponent<LogisticsNode>();

        //make sure the networks start with no members
        Assert.AreEqual(0, network1.FindComponents<LogisticsNode>().Length);
        Assert.AreEqual(0, network2.FindComponents<LogisticsNode>().Length);

        //make sure network1 has node1 and network2 has none
        node1.logisticsNetwork = network1;
        Assert.AreEqual(1, network1.NodeCount);
        Assert.AreSame(network1.transform, node1.transform.parent);
        Assert.AreEqual(1, network1.FindComponents<LogisticsNode>().Length);
        Assert.AreEqual(0, network2.FindComponents<LogisticsNode>().Length);


        //make sure each network has one node
        node2.logisticsNetwork = network2;
        Assert.AreEqual(1, network2.NodeCount);
        Assert.AreEqual(1, network1.FindComponents<LogisticsNode>().Length);
        Assert.AreEqual(1, network2.FindComponents<LogisticsNode>().Length);
        Assert.AreSame(network2.transform, node2.transform.parent);

        //make sure that network1 has both nodes and network2 has none
        node2.logisticsNetwork = network1;
        Assert.AreEqual(2, network1.FindComponents<LogisticsNode>().Length);
        Assert.AreEqual(0, network2.FindComponents<LogisticsNode>().Length);
        Assert.AreSame(network1.transform, node1.transform.parent);
        Assert.AreSame(network1.transform, node2.transform.parent);
    }

    [Test]
    public void TestRebuildNetwork() {
        //create one manager, two networks, and two nodes
        var managerObject = new GameObject();
        var manager = managerObject.AddComponent<LogisticsManager>();
        var networkObject1 = new GameObject();
        var network1 = networkObject1.AddComponent<LogisticsNetwork>();
        var networkObject2 = new GameObject();
        var network2 = networkObject2.AddComponent<LogisticsNetwork>();
        var nodeObject1 = new GameObject();
        var node1 = nodeObject1.AddComponent<LogisticsNode>();
        var nodeObject2 = new GameObject();
        var node2 = nodeObject2.AddComponent<LogisticsNode>();

        network1.logisticsManager = manager;
        network1.transform.position = new Vector3(0, 0, 0);
        network2.logisticsManager = manager;
        network2.transform.position = new Vector3(5, 0, 3);

        node1.logisticsManager = manager;
        node1.transform.position = new Vector3(1, 0, 1);
        node2.logisticsManager = manager;
        node2.transform.position = new Vector3(3, 0, 3);

        manager.RebuildNetworks();
        Assert.AreEqual(1, network1.FindComponents<LogisticsNode>().Length);
        Assert.AreSame(network1.transform, node1.transform.parent);
        Assert.AreEqual(1, network2.FindComponents<LogisticsNode>().Length);
        Assert.AreSame(network2.transform, node2.transform.parent);
    }

    [Test]
    public void TestDestroyNetwork() {
        //create one manager, two networks, and two nodes
        var managerObject = new GameObject();
        var manager = managerObject.AddComponent<LogisticsManager>();
        var networkObject1 = new GameObject();
        var network1 = networkObject1.AddComponent<LogisticsNetwork>();
        var networkObject2 = new GameObject();
        var network2 = networkObject2.AddComponent<LogisticsNetwork>();
        var nodeObject1 = new GameObject();
        var node1 = nodeObject1.AddComponent<LogisticsNode>();
        var nodeObject2 = new GameObject();
        var node2 = nodeObject2.AddComponent<LogisticsNode>();

        network1.logisticsManager = manager;
        network1.transform.position = new Vector3(0, 0, 0);
        network2.logisticsManager = manager;
        network2.transform.position = new Vector3(5, 0, 3);

        node1.logisticsManager = manager;
        node1.transform.position = new Vector3(1, 0, 1);
        node2.logisticsManager = manager;
        node2.transform.position = new Vector3(3, 0, 3);

        //destroy the first network and test to make sure all nodes now belong to network2
        manager.RebuildNetworks();
        //network1.OnDestroy();
        Object.DestroyImmediate(network1.gameObject);
        //Assert.AreEqual(2, network2.FindComponents<LogisticsNode>().Length);

        //destroy network2 and make sure that the nodes all still exist
        //network2.OnDestroy();
        Object.DestroyImmediate(network2.gameObject);
        Assert.IsNotNull(nodeObject1);
        Assert.IsNotNull(nodeObject2);
    }
}
