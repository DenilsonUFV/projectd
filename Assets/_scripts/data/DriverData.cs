using UnityEngine;

[CreateAssetMenu(fileName = "NewDriver", menuName = "Race/Driver")]
public class DriverData : ScriptableObject
{
    public string driverName;
    public GameObject carPrefab;

    [Range(1, 10)] public int agility = 5; // Define a largura da zona verde e a iniciativa base
    [Range(1, 10)] public int acceleration = 5;
}