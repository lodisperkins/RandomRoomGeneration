using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lodis.GridScripts
{
    [System.Serializable]
    public struct RoomData
    {
        [Range(0,100)]
        public float Likelihood;
        public Color Color;
        [HideInInspector]
        public RoomType Type;
    }

    public class RoomBehaviour : MonoBehaviour
    {
        [SerializeField]
        private Vector2 _position;
        [SerializeField]
        private bool _isLocked;
        private RoomData _data;
        [SerializeField]
        private MeshRenderer _renderer;
        private RoomType _type;

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
        public RoomType Type { get => _type; set => _type = value; }

        private void Start()
        {
            SetTestColor();
        }

        private void SetTestColor()
        {
            switch (_type)
            {
                case RoomType.EMPTY:
                    _renderer.material.color = Color.white;
                    break;
                case RoomType.ENEMY:
                    _renderer.material.color = Color.red;
                    break;
                case RoomType.TREASURE:
                    _renderer.material.color = Color.yellow;
                    break;
                case RoomType.START:
                    _renderer.material.color = Color.green;
                    break;
            }
        }
    }
}