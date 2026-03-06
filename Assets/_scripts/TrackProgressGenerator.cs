using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TrackProgressGenerator : MonoBehaviour
{
    public Tilemap tilemap;
    public Vector2Int startCell; // Defina no Inspector

    private Dictionary<Vector2Int, int> trackProgress = new Dictionary<Vector2Int, int>();
    private HashSet<Vector2Int> trackCells = new HashSet<Vector2Int>();

    void Start()
    {
        CacheTrackCells();
        trackProgress = GenerateTrackProgress(startCell);
        Debug.Log("Track gerado com " + trackProgress.Count + " células.");
    }

    void CacheTrackCells()
    {
        BoundsInt bounds = tilemap.cellBounds;

        foreach (var pos in bounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos))
            {
                trackCells.Add((Vector2Int)pos);
            }
        }
    }

    Dictionary<Vector2Int, int> GenerateTrackProgress(Vector2Int start)
    {
        Dictionary<Vector2Int, int> progress = new Dictionary<Vector2Int, int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        queue.Enqueue(start);
        progress[start] = 0;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            int currentIndex = progress[current];

            foreach (var neighbor in GetNeighbors(current))
            {
                if (!trackCells.Contains(neighbor))
                    continue;

                if (progress.ContainsKey(neighbor))
                    continue;

                progress[neighbor] = currentIndex + 1;
                queue.Enqueue(neighbor);
            }
        }

        return progress;
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

    public int GetProgress(Vector2Int position)
    {
        if (trackProgress.ContainsKey(position))
            return trackProgress[position];

        return -1;
    }

    public Transform GetLeader(List<Transform> players)
    {
        Transform leader = null;
        int bestProgress = -1;

        foreach (var player in players)
        {
            Vector3Int cell = tilemap.WorldToCell(player.position);
            Vector2Int gridPos = (Vector2Int)cell;

            int progress = GetProgress(gridPos);

            if (progress > bestProgress)
            {
                bestProgress = progress;
                leader = player;
            }
        }

        return leader;
    }
}