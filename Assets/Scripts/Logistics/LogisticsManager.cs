using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class LogisticsManager : MonoBehaviour {

    [SerializeField]
    private List<LogisticsNetwork> networks = new List<LogisticsNetwork>();
    [SerializeField]
    private List<LogisticsNode> nodes = new List<LogisticsNode>();

    /// <summary>
    /// Adds a LogisticsNetwork to the logistics system
    /// </summary>
    /// <param name="network"></param>
    public void RegisterNetwork(LogisticsNetwork network) {
        if (networks.Contains(network)) {
            throw new InvalidOperationException("Network already registered");
        }
        networks.Add(network);
    }

    /// <summary>
    /// Removes a LogisticsNetwork from the logistics system
    /// </summary>
    /// <param name="network"></param>
    public void UnregisterNetwork(LogisticsNetwork network) {
        networks.Remove(network);
    }

    /// <summary>
    /// Adds a logistics node to the logistics system
    /// </summary>
    /// <param name="node"></param>
    public void RegisterNode(LogisticsNode node) {
        if (nodes.Contains(node)) {
            throw new InvalidOperationException("Node already registered");
        }
        nodes.Add(node);
        var nearestNetwork = FindNearestNetwork(node.transform.position);
        node.LogisticsNetwork = nearestNetwork;
    }

    /// <summary>
    /// Removes a logistics node from the logistics system
    /// </summary>
    /// <param name="node"></param>
    public void UnregisterNode(LogisticsNode node) {
        nodes.Remove(node);
    }

    /// <summary>
    /// Reassigns all logistics nodes to the nearest network
    /// </summary>
    public void RebuildNetworks() {
        foreach (var node in nodes.Where(n => n != null)) {
            node.LogisticsNetwork = FindNearestNetwork(node.transform.position);
        }
    }

    /// <summary>
    /// Finds the active logistics node nearest to the given position
    /// </summary>
    /// <param name="position"></param>
    /// <returns>The nearest active network</returns>
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
