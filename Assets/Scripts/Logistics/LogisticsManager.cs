using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class LogisticsManager : MonoBehaviour {

    [SerializeField]
    private List<LogisticsNetwork> networks = new List<LogisticsNetwork>();
    [SerializeField]
    private List<LogisticsNode> nodes = new List<LogisticsNode>();

    public void RegisterNetwork(LogisticsNetwork network) {
        if (networks.Contains(network)) {
            throw new InvalidOperationException("Network already registered");
        }
        networks.Add(network);
    }

    public void UnregisterNetwork(LogisticsNetwork network) {
        networks.Remove(network);
    }

    public void RegisterNode(LogisticsNode node) {
        if (nodes.Contains(node)) {
            throw new InvalidOperationException("Node already registered");
        }
        nodes.Add(node);
        var nearestNetwork = FindNearestNetwork(node.transform.position);
        node.logisticsNetwork = nearestNetwork;
    }

    public void UnregisterNode(LogisticsNode node) {
        nodes.Remove(node);
    }

    public void RebuildNetworks() {
        foreach (var n in nodes) {
            n.logisticsNetwork = FindNearestNetwork(n.transform.position);
        }
    }

    public LogisticsNetwork FindNearestNetwork(Vector3 position) {
        LogisticsNetwork closest = null;
        float closestDist = float.PositiveInfinity;

        foreach (var network in networks) {
            if (network == null) {
                continue;
            }
            var diff = position - network.transform.position;
            var dist = diff.sqrMagnitude;
            if (dist < closestDist) {
                closest = network;
                closestDist = dist;
            }
        }
        return closest;
    }
}
