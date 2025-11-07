using System.Collections.Generic;
using UnityEngine;

public static class PathFinder
{
    private const int MOVE_COST = 10;

    public static bool TryFindPath(Node startNode, Node targetNode, out List<Vector3> path)
    {
        if (startNode == null || targetNode == null || !targetNode.isWalkable)
        {
            path = null;
            return false;
        }
        path = new();

        SortedSet<Node> bstree = new();
        HashSet<Node> visited = new();

        startNode.gCost = 0;
        startNode.hCost = GetManhattan(startNode, targetNode);
        startNode.parent = null;
        bstree.Add(startNode);

        while (bstree.Count > 0)
        {
            Node current = bstree.Min;
            bstree.Remove(current);
            visited.Add(current);

            if (current.x == targetNode.x && current.y == targetNode.y)
            {
                path = RebuildPath(startNode, targetNode);
                return true;
            }

            List<Node> neighbours = GridManager.instance.GetNeighbors(current);
            foreach (var n in neighbours)
            {
                if (!n.isWalkable || visited.Contains(n)) continue;
                int newCost = current.gCost + MOVE_COST;

                bool visi = bstree.Contains(n);
                if (newCost < n.gCost || !visi)
                {
                    if (visi) bstree.Remove(n);

                    n.gCost = newCost;
                    n.hCost = GetManhattan(n, targetNode);
                    n.parent = current;

                    bstree.Add(n);
                }
            }
        }

        return false;
    }
    
    private static List<Vector3> RebuildPath(Node startNode, Node endNode)
    {
        List<Vector3> path = new ();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(GridManager.instance.CellToWorldPosition(currentNode.x, currentNode.y));
            currentNode = currentNode.parent;

            if (currentNode == null) break;
        }
        
        path.Reverse();
        return path;
    }
    
    public static int GetManhattan(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.x - nodeB.x);
        int dstY = Mathf.Abs(nodeA.y - nodeB.y);
        
        return (dstX + dstY) * MOVE_COST; 
    }
}
