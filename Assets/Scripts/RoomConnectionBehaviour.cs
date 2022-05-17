using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ConnectionType
{
    NONE,
    LOCKED,
    OPEN
}

[System.Serializable]
public struct RoomConnectionData
{
    public ConnectionType ConnectionType;
    public Vector2 Room1Position;
    public Vector2 Room2Position;
    public void SetConnectionType(ConnectionType connectionType) { ConnectionType = connectionType; }
}


public class RoomConnectionBehaviour : MonoBehaviour
{
    [SerializeField]
    private GameObject _wallFill;
    [SerializeField]
    private GameObject _unlockedDoor;
    [SerializeField]
    private GameObject _lockedDoor;
    [SerializeField]
    private RoomConnectionData _connectionData;

    public RoomConnectionData ConnectionData { get => _connectionData; set => _connectionData = value; }

    private void Start()
    {
        SpawnDoor();
    }

    private void OnTriggerEnter(Collider other)
    {
    }

    private void SpawnDoor()
    {
        switch (_connectionData.ConnectionType)
        {
            case ConnectionType.NONE:
                Instantiate(_wallFill, transform);
                break;
            case ConnectionType.LOCKED:
                Instantiate(_lockedDoor, transform);
                break;
            case ConnectionType.OPEN:
                Instantiate(_unlockedDoor, transform);
                break;
        }
    }
}