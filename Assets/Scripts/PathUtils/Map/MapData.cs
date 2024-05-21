using UnityEngine;

namespace PathUtils
{
    public class MapData : IMapData
    {
        private Vector2Int _size;
        public Vector2Int size => _size;

        public MapData(Vector2Int size)
        {
            _size = size;
            _traversabilityArray = new bool[size.x, size.y];
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    _traversabilityArray[i, j] = true;
                }
            }
        }

        private bool[,] _traversabilityArray;

        public bool this[Vector2Int index]
        {
            get => _size.Contains(index) && _traversabilityArray[index.x, index.y];
            set => _traversabilityArray[index.x, index.y] = value;
        }
    }
}