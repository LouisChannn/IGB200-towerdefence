using UnityEngine;

[System.Serializable]
public class EnemyType
{
    public GameObject prefab;
    public int weight = 1; // how much of the wave's weight budget this enemy costs to spawn
}