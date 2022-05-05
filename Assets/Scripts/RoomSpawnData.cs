using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomSpawnData", menuName = "Room Spawn Data")]
public class RoomSpawnData : ScriptableObject
{
    [SerializeField]
    private string _name;
    [SerializeField]
    private float _minSpawnAmount;
    [SerializeField]
    private float _maxSpawnAmount = -1;
    [SerializeField]
    private int _spawnCount;
    [SerializeField]
    private Color _debugColor;

    public string Name { get => _name; set => _name = value; }
    public float MinSpawnAmount { get => _minSpawnAmount; }
    public float MaxSpawnAmount { get => _maxSpawnAmount; }
    public Color DebugColor { get => _debugColor; set => _debugColor = value; }
    public int SpawnCount { get => _spawnCount; } 
    public void IncreaseSpawnCount()
    {
        _spawnCount++;
    }
    public void ResetSpawnCount()
    {
        _spawnCount = 0;
    }

    public bool GetCanSpawn()
    {
        if (MaxSpawnAmount < 0) return true;

        return _spawnCount < MaxSpawnAmount;
    }
}
