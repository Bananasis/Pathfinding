using UnityEngine;

namespace PathUtils
{
    public interface IVisitLedger
    {
        public (bool, int) Visit(Vector2Int nod, Vector2Int pos);
        public void Close(Vector2Int pos);

        public bool IsOpen(Vector2Int nodePos);
        public bool TryGetParent(Vector2Int nodepos, out Vector2Int parent);

        public void Clear();
        void VisitSource(Vector2Int source);
    }
}

public struct Node
{
    public int gCost;
    public Vector2Int parent;
    public bool open;
}