using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lodis.GridScripts
{

    public class RoomBehaviour : MonoBehaviour
    {
        [SerializeField]
        private Vector2 _position;
        [SerializeField]
        private bool _isLocked;
        [SerializeField]
        private MeshRenderer _renderer;
        private RoomSpawnData _spawnData;

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
    }
}