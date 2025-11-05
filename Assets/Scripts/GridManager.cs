using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Node : IComparable<Node>
{
    public int x;
    public int y;
    public bool isWalkable;

    public Vector3Int AsVector3Int ()
    {
        return new(x, y);
    }

    public int gCost;
    public int hCost;
    public int Cost
    {
        get
        {
            return gCost + hCost;
        }
    }
    public Node parent;
    public Node(int _x, int _y, bool _walkable)
    {
        x = _x;
        y = _y;
        isWalkable = _walkable;
    }

    public int CompareTo(Node other)
    {
        int compare = Cost.CompareTo(other.Cost);

        if (compare == 0)
            compare = hCost.CompareTo(other.hCost);
        
        if (compare == 0)
             compare = x.CompareTo(other.x);
        
        if (compare == 0)
             compare = y.CompareTo(other.y);
        
        return compare;
    }
}

public class GridManager : MonoBehaviour
{
    [SerializeField] private Tilemap walkTile;
    [SerializeField] private List<Tilemap> obstacleTiles;

    public static GridManager instance;
    public Node[,] grid;
    private Vector3Int minLimit;
    private Vector3Int maxLimit;

    void Awake()
    {
        walkTile.CompressBounds();
        minLimit = walkTile.cellBounds.min;
        maxLimit = walkTile.cellBounds.max;

        CreateGrid();
    }

    void Start()
    {
        instance = this;
    }

    private void CreateGrid()
    {
        Vector3Int size = maxLimit - minLimit;
        grid = new Node[size.x, size.y];

        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
            {
                var cellPos = new Vector3Int(minLimit.x + x, minLimit.y + y, 0);
                bool obstacle = false;
                foreach (var tm in obstacleTiles)
                {
                    if (tm.HasTile(cellPos))
                    {
                        obstacle = true;
                        break;
                    }
                }
                bool walkable = !obstacle && walkTile.HasTile(cellPos);
                grid[x, y] = new(cellPos.x, cellPos.y, walkable);
            }
        Debug.Log("Grid A* criado com sucesso. Tamanho: " + size.x + "x" + size.y);
    }

    public bool TryGetNode(Vector3 wordlPoint, out Node node)
    {
        var cellPos = walkTile.WorldToCell(wordlPoint);
        var pos = cellPos - minLimit;

        if (pos.x >= 0 && pos.x < grid.GetLength(0) && pos.y >= 0 && pos.y < grid.GetLength(1))
        {
            node = grid[pos.x, pos.y];
            return true;
        }
        node = null;
        return false;
    }

    public List<Node> GetNeighbors(Node node)
    {
        Vector3Int[] steps = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        List<Node> nodes = new();
        foreach (var s in steps)
        {
            if (TryGetNode(s + node.AsVector3Int(), out Node p))
            {
                nodes.Add(p);
            }
        }
        return nodes;
    }

    public Vector3 CellToWorldPosition(int x, int y)
    {
        Vector3Int cellPos = new (x, y, 0);
        
        Vector3 worldPoint = walkTile.CellToWorld(cellPos);
        
        return worldPoint + walkTile.cellSize * 0.5f; 
    }
}
