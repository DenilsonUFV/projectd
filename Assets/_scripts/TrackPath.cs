using UnityEngine;
using System.Collections.Generic;

public class TrackPath : MonoBehaviour
{
    public List<Transform> waypoints = new List<Transform>();

    // Desenha uma linha no editor para facilitar a visualização
    private void OnDrawGizmos()
    {
        if (waypoints.Count < 2) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            if (waypoints[i] && waypoints[i + 1])
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
    }
}