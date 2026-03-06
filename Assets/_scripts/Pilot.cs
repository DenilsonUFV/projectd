using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Tilemaps;


public class Pilot : MonoBehaviour
{

    public TrackGrid trackSystem;

    public int moveSpeed = 5;
    public Tilemap tilemap;

    public Vector2Int chosenCell;

    void OnMouseDown()
    {
        Debug.Log("Teste");
        Vector2Int start = trackSystem.WorldToGrid(transform.position);
        Vector2Int target = chosenCell;

        List<Vector2Int> path = trackSystem.GetPath(start, target, moveSpeed, trackSystem.currentDirection);

        MoveAlongPath(path);
    }

    IEnumerator MoveRoutine(List<Vector2Int> path)
    {
        foreach (Vector2Int gridPos in path)
        {
            Vector3 worldPos = tilemap.GetCellCenterWorld((Vector3Int)gridPos);

            while (Vector3.Distance(transform.position, worldPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    worldPos,
                    moveSpeed * Time.deltaTime);

                yield return null;
            }

            transform.position = worldPos;
        }
    }

    public void MoveAlongPath(List<Vector2Int> path)
    {
        StartCoroutine(MoveRoutine(path));
    }

}
