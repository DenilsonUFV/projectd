using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public TrackProgressGenerator trackProgress;

    public List<Transform> pilots;

    public void Update()
    {
        if (trackProgress != null)
        {
            Debug.Log(trackProgress.GetLeader(pilots));
        }
    }
}
