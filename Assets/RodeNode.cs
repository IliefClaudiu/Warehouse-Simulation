using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RodeNode : MonoBehaviour
{
    public RodeNode[] neighbors; // Manually link or auto-detect later
    public bool isOccupied = false; // Optional: for dynamic obstacles
    void AutoConnectNodes()
    {
        RodeNode[] nodes = FindObjectsOfType<RodeNode>();
        foreach (RodeNode node in nodes)
        {
            List<RodeNode> nearby = new List<RodeNode>();
            foreach (RodeNode other in nodes)
            {
                if (node != other && Vector3.Distance(node.transform.position, other.transform.position) < 2f)
                    nearby.Add(other);
            }
            node.neighbors = nearby.ToArray();
        }
    }
}
