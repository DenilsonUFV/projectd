using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.LightTransport;
using static UnityEditor.Experimental.GraphView.GraphView;

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public class TrackGrid : MonoBehaviour
{

    public List<Transform> pilots;
    public Transform player;
    public Tilemap roadTilemap;

    public GameObject highlightPrefab;

    public Direction currentDirection;

    [System.Serializable]
    public struct ItemDoDicionario
    {
        public Vector2Int chave;
        public bool valor;
    }
    public List<ItemDoDicionario> listaDeItens = new List<ItemDoDicionario>();

    public Dictionary<Vector2Int, bool> walkableGrid
        = new Dictionary<Vector2Int, bool>();

    void Awake()
    {
        BuildGrid();
        foreach (KeyValuePair<Vector2Int,bool> kv in walkableGrid)
        {
            ItemDoDicionario item = new ItemDoDicionario();
            item.chave = kv.Key;
            item.valor = kv.Value;
            listaDeItens.Add(item);
        }
    }

    private void Start()
    {
        
        // Alternatively, in a single line:
        Vector2Int playerPos = Vector2Int.RoundToInt((Vector2)player.position);
        int movement = 4;

        //  List<Vector2Int> cells = GetReachableCellsge(playerPos, movement);

        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();

        foreach (var p in pilots)
        {
            Vector3Int cell = roadTilemap.WorldToCell(p.position);
            occupied.Add((Vector2Int)cell);
        }

        List<Vector2Int> cells = GetFinalPositions(playerPos, movement, currentDirection, occupied);
        foreach (var cell in cells)
        {
            Vector3 world = GridToWorld(cell);
            Instantiate(highlightPrefab, world, Quaternion.identity);
        }

        

        //Instantiate(highlightPrefab, GridToWorld(WorldToGrid(player.position)), Quaternion.identity);
    }

    void BuildGrid()
    {
        walkableGrid.Clear();

        BoundsInt bounds = roadTilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (roadTilemap.HasTile(pos))
            {
                Vector2Int gridPos = new Vector2Int(pos.x, pos.y);
                walkableGrid[gridPos] = true;
            }
        }
    }
    public bool IsWalkable(Vector2Int gridPosition)
    {
        return walkableGrid.ContainsKey(gridPosition);
    }

    public Vector3 GridToWorld(Vector2Int gridPosition)
    {
        return roadTilemap.GetCellCenterWorld(
            new Vector3Int(gridPosition.x, gridPosition.y, 0)
        );
    }

    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        Vector3Int cell = roadTilemap.WorldToCell(worldPosition);
        return new Vector2Int(cell.x, cell.y);
    }

    public List<Vector2Int> GetPath(
    Vector2Int start,
    Vector2Int target,
    int movement,
    Direction dir)
    {
        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, int> costSoFar = new Dictionary<Vector2Int, int>();

        frontier.Enqueue(start);
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();

            if (current == target)
                break;

            foreach (Vector2Int neighbor in GetForwardNeighbors(current, dir))
            {
                if (!IsWalkable(neighbor))
                    continue;

                int newCost = costSoFar[current] + 1;

                if (newCost > movement)
                    continue;

                if (!costSoFar.ContainsKey(neighbor))
                {
                    costSoFar[neighbor] = newCost;
                    frontier.Enqueue(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }

        // Reconstruir caminho
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int step = target;

        while (step != start)
        {
            path.Add(step);
            step = cameFrom[step];
        }

        path.Reverse();
        return path;
    }

    public List<Vector2Int> GetFinalPositions(
    Vector2Int start,
    int movement,
    Direction dir,
    HashSet<Vector2Int> occupiedPositions)
    {
        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> costSoFar = new Dictionary<Vector2Int, int>();
        List<Vector2Int> finalPositions = new List<Vector2Int>();

        frontier.Enqueue(start);
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();
            int currentCost = costSoFar[current];

            if (currentCost == movement)
            {
                if (!occupiedPositions.Contains(current))
                {
                    finalPositions.Add(current);
                }
                continue; // não expande mais a partir daqui
            }

            foreach (Vector2Int neighbor in GetForwardNeighbors(current, dir))
            {
                if (!IsWalkable(neighbor))
                    continue;

                int newCost = currentCost + 1;

                if (newCost > movement)
                    continue;

                if (!costSoFar.ContainsKey(neighbor))
                {
                    costSoFar[neighbor] = newCost;
                    frontier.Enqueue(neighbor);
                }
            }
        }

        return finalPositions;
    }

    public List<Vector2Int> GetForwardReachable(
    Vector2Int start,
    int movement,
    Direction dir)
    {
        List<Vector2Int> reachable = new List<Vector2Int>();
        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> costSoFar = new Dictionary<Vector2Int, int>();

        frontier.Enqueue(start);
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();

            foreach (Vector2Int neighbor in GetForwardNeighbors(current, dir))
            {
                if (!IsWalkable(neighbor))
                    continue;

                int newCost = costSoFar[current] + 1;

                if (newCost > movement)
                    continue;

                if (!costSoFar.ContainsKey(neighbor))
                {
                    costSoFar[neighbor] = newCost;
                    frontier.Enqueue(neighbor);
                    reachable.Add(neighbor);
                }
            }
        }

        return reachable;
    }

    public List<Vector2Int> GetReachableCells(Vector2Int start, int movement)
    {
        List<Vector2Int> reachable = new List<Vector2Int>();
        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> costSoFar = new Dictionary<Vector2Int, int>();

        frontier.Enqueue(start);
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (!IsWalkable(neighbor))
                    continue;

                int newCost = costSoFar[current] + 1;

                if (newCost > movement)
                    continue;

                if (!costSoFar.ContainsKey(neighbor))
                {
                    costSoFar[neighbor] = newCost;
                    frontier.Enqueue(neighbor);
                    reachable.Add(neighbor);
                }
            }
        }

        return reachable;
    }

    List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        return new List<Vector2Int>
        {
            pos + Vector2Int.up,
            pos + Vector2Int.down,
            pos + Vector2Int.left,
            pos + Vector2Int.right
        };
    }

    List<Vector2Int> GetForwardNeighbors(Vector2Int pos, Direction dir)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        switch (dir)
        {
            case Direction.Up:
                neighbors.Add(pos + Vector2Int.up);
                neighbors.Add(pos + Vector2Int.left);
                neighbors.Add(pos + Vector2Int.right);
                break;

            case Direction.Down:
                neighbors.Add(pos + Vector2Int.down);
                neighbors.Add(pos + Vector2Int.left);
                neighbors.Add(pos + Vector2Int.right);
                break;

            case Direction.Left:
                neighbors.Add(pos + Vector2Int.left);
                neighbors.Add(pos + Vector2Int.up);
                neighbors.Add(pos + Vector2Int.down);
                break;

            case Direction.Right:
                neighbors.Add(pos + Vector2Int.right);
                neighbors.Add(pos + Vector2Int.up);
                neighbors.Add(pos + Vector2Int.down);
                break;
        }

        return neighbors;
    }

}