using UnityEngine;

namespace PathUtils
{
    public interface IMapData
    {
        public bool this[Vector2Int index] { get; set; }
        Vector2Int size { get; }
    }
}