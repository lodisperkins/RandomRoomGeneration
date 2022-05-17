using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoomConnectionDirection
{
    NORTH,
    SOUTH,
    EAST,
    WEST
}

public class RoomBehaviour : MonoBehaviour
{
    [SerializeField]
    private Vector2 _position;
    [SerializeField]
    private bool _isLocked;
    [SerializeField]
    private MeshRenderer _renderer;
    private RoomSpawnData _spawnData;
    private RoomConnectionBehaviour[] _roomConnections = new RoomConnectionBehaviour[4];
    [SerializeField]
    private Transform[] _connectionSpawns = new Transform[4];

    /// <summary>
    /// The position of this panel on the grid.
    /// </summary>
    public Vector2 Position
    {
        get
        {
            return _position;
        }
        set
        {
            _position = value;
        }
    }

    public bool IsLocked { get => _isLocked; set => _isLocked = value; }
    public RoomSpawnData SpawnData
    { 
        get => _spawnData;
        set
        {
            _spawnData = value;
            UpdateColor();
        }
    }

    private void UpdateColor()
    {
        _renderer.material.color = SpawnData.DebugColor;
    }

    public void SetRoomConnection(RoomConnectionBehaviour roomConnection, RoomConnectionDirection direction)
    {
        _roomConnections[(int)direction] = roomConnection;
        roomConnection.transform.position = _connectionSpawns[(int)direction].position;
        roomConnection.transform.rotation = _connectionSpawns[(int)direction].rotation;
    }

    public void DestroyConnections()
    {
        for (int i = 0; i < _roomConnections.Length; i++)
            Destroy(_roomConnections[i].gameObject);
    }

    public void FillEmptyConnections(RoomConnectionBehaviour connectionRef)
    {
        for (int i = 0; i < _roomConnections.Length; i++)
        {
            if (_roomConnections[i] == null)
            {
                _roomConnections[i] = Instantiate(connectionRef, _connectionSpawns[i]);
            }
        }
    }
}