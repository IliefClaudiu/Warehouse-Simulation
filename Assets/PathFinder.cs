using System.Collections.Generic;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    public static PathFinder Instance;
    private void Awake()
    {
        Instance = this;
        Debug.Log("Pathfinder Awake called.");
    }

    public List<RodeNode> FindPath(RodeNode startNode, RodeNode endNode)
    {

        List<RodeNode> openSet = new List<RodeNode> { startNode };
        HashSet<RodeNode> closedSet = new HashSet<RodeNode>();

        Dictionary<RodeNode, RodeNode> cameFrom = new Dictionary<RodeNode, RodeNode>();
        Dictionary<RodeNode, float> gScore = new Dictionary<RodeNode, float>();
        Dictionary<RodeNode, float> fScore = new Dictionary<RodeNode, float>();

        foreach (var node in FindObjectsOfType<RodeNode>())
        {
            gScore[node] = float.MaxValue;
            fScore[node] = float.MaxValue;
        }

        gScore[startNode] = 0;
        fScore[startNode] = Vector3.Distance(startNode.transform.position, endNode.transform.position);

        while (openSet.Count > 0)
        {
            RodeNode current = openSet[0];
            foreach (var node in openSet)
            {
                if (fScore[node] < fScore[current])
                    current = node;
            }

            if (current == endNode)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (var neighbor in current.neighbors)
            {
                if (closedSet.Contains(neighbor)) continue;

                float tentativeGScore = gScore[current] + Vector3.Distance(current.transform.position, neighbor.transform.position);
                if (!openSet.Contains(neighbor)) openSet.Add(neighbor);

                if (tentativeGScore >= gScore[neighbor]) continue;

                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + Vector3.Distance(neighbor.transform.position, endNode.transform.position);
            }
        }

        return null; // no path found
    }

    public RodeNode FindClosestNode(Vector3 position)
    {
        RodeNode[] allNodes = FindObjectsOfType<RodeNode>();
        Debug.Log($"FindClosestNode: Found {allNodes.Length} nodes in scene.");


        RodeNode closest = null;
        float shortestDistance = Mathf.Infinity;

        foreach (RodeNode node in allNodes)
        {
            float distance = Vector3.Distance(position, node.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closest = node;
            }
        }
        if (closest != null)
            Debug.Log($"Closest node at position: {closest.transform.position} with distance {shortestDistance}");
        else
            Debug.LogError("FindClosestNode: No nodes found to return!");


        return closest;
    }
    private List<RodeNode> ReconstructPath(Dictionary<RodeNode, RodeNode> cameFrom, RodeNode current)
    {
        List<RodeNode> totalPath = new List<RodeNode> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }
        return totalPath;
    }
}