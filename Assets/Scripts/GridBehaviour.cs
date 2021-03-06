using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Unity.Collections;
using Random = UnityEngine.Random;

namespace Lodis.GridScripts
{

    public class GridBehaviour : MonoBehaviour
    {
        [Tooltip("The grid will use this game object to create the panels. MUST HAVE A PANEL BEHAVIOUR ATTACHED")]
        [SerializeField]
        private GameObject _panelRef;
        [Tooltip("The dimensions to use when building the grid.")]
        [SerializeField]
        private Vector2 _dimensions;
        [SerializeField]
        private Vector3 _roomScale;
        [SerializeField]
        private Vector2 _roomOffset;
        private RoomBehaviour[,] _panels;
        private float _width;
        private float _height;
        [SerializeField]
        private Vector2 _spawnRoomPosition;
        [SerializeField]
        private RoomSpawnData _startRoomData;
        [SerializeField]
        private RoomSpawnData _exitRoomData;
        [SerializeField]
        private RoomSpawnData _inkRoomData;
        [SerializeField]
        private RoomSpawnData[] _roomSpawnData;
        [SerializeField]
        private List<RoomConnectionData> _roomConnectionData;
        private Dictionary<(Vector2, Vector2), RoomConnectionData> _roomDataDictionary;
        private RoomBehaviour _spawnRoom;
        [SerializeField]
        private RoomConnectionBehaviour _connectionRef;

        /// <summary>
        /// The spawn point for the character on the left side of the grid
        /// </summary>
        public RoomBehaviour SpawnRoom
        {
            get
            {
                return _spawnRoom;
            }
        }

        /// <summary>
        /// A reference to the panel object to use for building the grid
        /// </summary>
        public GameObject PanelRef
        {
            get
            {
                return _panelRef;
            }
        }

        /// <summary>
        /// The space in between each panel
        /// </summary>
        public float PanelSpacingX
        {
            get
            {
                return _roomScale.x * 10f;
            }
        }
        
        /// <summary>
        /// The space in between each panel
        /// </summary>
        public float PanelSpacingZ
        {
            get
            {
                return _roomScale.z * 10f;
            }
        }

        /// <summary>
        /// The dimensions of the panels on the grid
        /// </summary>
        public Vector2 Dimensions
        {
            get 
            {
                return _dimensions;
            }
        }

        /// <summary>
        /// The dimensions of each panel
        /// </summary>
        public Vector3 PanelScale
        {
            get { return _roomScale; }
        }


        /// <summary>
        /// The width of the grid in relation to the world space
        /// </summary>
        public float Width { get => _width; }

        /// <summary>
        /// The height of the grid in relation to the world space
        /// </summary>
        public float Height { get => _height; }
        public List<RoomConnectionData> RoomConnectionData { get => _roomConnectionData; set => _roomConnectionData = value; }

        public void GenerateConnectionDictionary()
        {
            _roomDataDictionary = new Dictionary<(Vector2, Vector2), RoomConnectionData>();
            foreach (RoomConnectionData data in _roomConnectionData)
            {
                (Vector2, Vector2) connectionPositions1 = (data.Room1Position, data.Room2Position);
                (Vector2, Vector2) connectionPositions2 = (data.Room2Position, data.Room1Position);
                _roomDataDictionary.Add(connectionPositions1, data);
                _roomDataDictionary.Add(connectionPositions2, data);
            }
        }

        /// <summary>
        /// Creates a grid using the given dimensions and spacing.
        /// </summary>
        public void CreateGrid()
        {
            
            GenerateConnectionDictionary();
            ResetRoomSpawnCount();
            _panels = new RoomBehaviour[(int)_dimensions.x, (int)_dimensions.y];

            //The world spawn position for each gameobject in the grid
            Vector3 spawnPosition = transform.position;
            int spawnRoom = (int)Random.Range(0, _dimensions.x);
            int exitRoom = (int)Random.Range(0, _dimensions.x);

            //The x and y position for each game object in the grid
            for (int i = 0; i < (int)_dimensions.y; i++)
            {
                for (int j = 0; j < (int)_dimensions.x; j++)
                {
                    _panels[j, i] = Instantiate(_panelRef, spawnPosition, new Quaternion(), transform).GetComponent<RoomBehaviour>();
                    RoomBehaviour room = _panels[j, i];
                    room.Position = new Vector2(j, i);
                    room.transform.localScale = _roomScale;

                    //Try spawn start room
                    if (spawnRoom == j && i == 0)
                        room.SpawnData = _startRoomData;
                    else if (exitRoom == j && i == _dimensions.y - 1)
                        room.SpawnData = _exitRoomData;
                    else room.SpawnData = GetRandomRoomType(i);

                    room.SpawnData.IncreaseSpawnCount();

                    if (j > 0)
                    {
                        RoomConnectionData data;
                        RoomBehaviour previousRoom = _panels[j - 1, i];

                        if (_roomDataDictionary.TryGetValue((previousRoom.Position, room.Position), out data))
                        {
                            RoomConnectionBehaviour roomConnection = Instantiate(_connectionRef, null);
                            roomConnection.ConnectionData = data;

                            previousRoom.SetRoomConnection(roomConnection, RoomConnectionDirection.EAST);
                            room.SetRoomConnection(roomConnection, RoomConnectionDirection.WEST);
                        }
                    }
                    
                    if (i > 0)
                    {
                        RoomConnectionData data;
                        RoomBehaviour roomBelow = _panels[j, i - 1];

                        if (_roomDataDictionary.TryGetValue((roomBelow.Position, room.Position), out data))
                        {
                            RoomConnectionBehaviour roomConnection = Instantiate(_connectionRef, null);
                            roomConnection.ConnectionData = data;

                            roomBelow.SetRoomConnection(roomConnection, RoomConnectionDirection.NORTH);
                            room.SetRoomConnection(roomConnection, RoomConnectionDirection.SOUTH);
                        }
                    }

                    //Increase x position
                    spawnPosition.x += PanelSpacingX;
                }

                //If the x position in the grid is equal to the given x dimension,
                //reset x position to be 0, and increase the y position.
                spawnPosition.x = transform.position.x;
                spawnPosition.z += PanelSpacingZ;
            }

            foreach (RoomBehaviour room in _panels)
            {
                room.FillEmptyConnections(_connectionRef);
            }

            if (!GetPanel(_spawnRoomPosition, out _spawnRoom)) Debug.LogError("Spawn point is invalid");

            _width = (_dimensions.x * _panelRef.transform.localScale.x) + (PanelSpacingX * _dimensions.x);
            _height = (_dimensions.y * _panelRef.transform.localScale.z) + (PanelSpacingZ * _dimensions.y);
        }

        private RoomSpawnData GetRandomRoomType(int currentRoomIndex)
        {
            float max = _dimensions.x * _dimensions.y;
            float min = 0;

            for (int i = 0; i < _roomSpawnData.Length; i++)
            {
                float rand = Random.Range(min, max);
                if (rand <= currentRoomIndex + _roomSpawnData[i].MinSpawnAmount && _roomSpawnData[i].GetCanSpawn() )
                    return _roomSpawnData[i];
            }

            return _inkRoomData;
        }

        /// <summary>
        /// Destroys panels that may have existed before the game starts
        /// </summary>
        public void DestroyTempPanels()
        {
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Destroys all panels in the grid. (Only for use in editor)
        /// </summary>
        public void DestroyTempPanelsInEditor()
        {
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }

        /// <summary>
        /// Destroys all panels in the grid.
        /// </summary>
        public void DestroyGrid()
        {
            if (_panels == null)
                return;

            transform.DetachChildren();

            for (int i = 0; i < _dimensions.x; i++)
            {
                for (int j = 0; j < _dimensions.y; j++)
                {
                    _panels[i, j].DestroyConnections();
                    Destroy(_panels[i, j].gameObject);
                }
            }
            _panels = null;
        }

        /// <summary>
        /// Finds and outputs the panel at the given location.
        /// </summary>
        /// <param name="x">The x position  of the panel.</param>
        /// <param name="y">The y position of the panel.</param>
        /// <param name="room">The panel reference to output to.</param>
        /// <param name="isLocked">If true, the function will return true even if the panel found is occupied.</param>
        /// <param name="roomType">Will return false if the panel found doesn't match this roomType.</param>
        /// <returns>Returns true if the panel is found in the list and the IsLocked condition is met.</returns>
        public bool GetPanel(int x, int y, out RoomBehaviour room, bool isLocked = true, string roomType = "")
        {
            room = null;

            //If the given position is in range or if the panel is occupied when it shouldn't be, return false.
            if (x < 0 || x >= _dimensions.x || y < 0 || y >= _dimensions.y)
                return false;
            else if (!isLocked && _panels[x, y].IsLocked)
                return false;
            else if (_panels[x, y].SpawnData.Name != roomType && roomType != "")
                return false;

            room = _panels[x, y];

            return true;
        }


        /// <summary>
        /// Finds and outputs the panel at the given location.
        /// </summary>
        /// <param name="position">The position of the panel on the grid.</param>
        /// <param name="panel">The panel reference to output to.</param>
        /// <param name="isLocked">If true, the function will return true even if the panel found is occupied.</param>
        /// <param name="roomType">Will return false if the panel found doesn't match this roomType.</param>
        /// <returns>Returns true if the panel is found in the list and the IsLocked condition is met.</returns>
        public bool GetPanel(Vector2 position, out RoomBehaviour panel, bool isLocked = true, string roomType = "")
        { 
            panel = null;

            if (_panels == null)
                return false;

            //If the given position is in range or if the panel is occupied when it shouldn't be, return false.
            if (position.x < 0 || position.x >= _dimensions.x || position.y < 0 || position.y >= _dimensions.y || float.IsNaN(position.x) || float.IsNaN(position.y))
                return false;
            else if (!isLocked && _panels[(int)position.x, (int)position.y].IsLocked)
                return false;
            else if (_panels[(int)position.x, (int)position.y].SpawnData.Name != roomType && roomType != "")
                return false;

            panel = _panels[Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y)];

            return true;
        }

        /// <summary>
        /// Gets the panel that is closest to the given location in the world
        /// </summary>
        /// <param name="location">The location to look for the panel in world space</param>
        /// <param name="panel">The panel reference to assign</param>
        /// <param name="IsLocked">Whether or not panels that are occupied should be ignored</param>
        /// <param name="roomType">The side of the grid to look for this panel</param>
        /// <returns></returns>
        public bool GetPanelAtLocationInWorld(Vector3 location, out RoomBehaviour panel, bool IsLocked = true, string roomType = "")
        {
            panel = null;

            if (_panels == null)
                return false;

            int x = Mathf.RoundToInt((location.x / (PanelRef.transform.localScale.x + PanelSpacingX)));
            int y = Mathf.RoundToInt((location.z / (PanelRef.transform.localScale.z + PanelSpacingZ)));

            //If the given position is in range or if the panel is occupied when it shouldn't be, return false.
            if (x < 0 || x >= _dimensions.x || y < 0 || y >= _dimensions.y || float.IsNaN(x) || float.IsNaN(y))
                return false;
            else if (!IsLocked && _panels[x, y].IsLocked)
                return false;
            else if (_panels[x, y].SpawnData.Name != roomType && roomType != "")
                return false;

            panel = _panels[x, y];

            return true;
        }

        /// <summary>
        /// Gets a list of panels that are withing a range of 1
        /// </summary>
        /// <param name="position">The position of the panel to find neighbors for</param>
        /// <param name="IsLocked">Whether or not to ignore panels that are ooccupied</param>
        /// <param name="roomType">The side of the grid to look for neighbors. Panels found on the other side will be ignored</param>
        /// <returns></returns>
        public List<RoomBehaviour> GetPanelNeighbors(Vector2 position, bool IsLocked = true, string roomType = "")
        {
            List<RoomBehaviour> neighbors = new List<RoomBehaviour>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Vector2 offset = new Vector2(x, y);

                    if (offset + position == position)
                        continue;

                    RoomBehaviour panel = null;
                    if (GetPanel((position + offset), out panel, IsLocked, roomType))
                        neighbors.Add(panel);
                }
            }

            return neighbors;
        }

        private void ResetRoomSpawnCount()
        {
            _startRoomData.ResetSpawnCount();
            _exitRoomData.ResetSpawnCount();
            _inkRoomData.ResetSpawnCount();
            for (int i = 0; i < _roomSpawnData.Length; i++)
                _roomSpawnData[i].ResetSpawnCount();
        }

        private void OnDestroy()
        {
            ResetRoomSpawnCount();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GridBehaviour))]
    class GridEditor : Editor
    {
        private GridBehaviour _grid;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            _grid = (GridBehaviour)target;

            if (GUILayout.Button("View Grid"))
            {
                _grid.DestroyGrid();
                _grid.CreateGrid();
            }
        }
    }

#endif
}

