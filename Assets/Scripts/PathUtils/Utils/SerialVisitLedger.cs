using UnityEngine;

namespace PathUtils
{
    public class SerialVisitLedger : IVisitLedger
    {
        private uint _series;
        private uint[,] _seriesArray;
        private Node[,] _nodesArray;

        public SerialVisitLedger(Vector2Int size)
        {
            _series = 1;
            _seriesArray = new uint[size.x, size.y];
            _nodesArray = new Node[size.x, size.y];
        }

        public (bool, int) Visit(Vector2Int nodePos, Vector2Int parentPos)
        {
            var series = _seriesArray[nodePos.x, nodePos.y];
            var newCost = _nodesArray[parentPos.x, parentPos.y].gCost + 1;
            if (series != _series)
            {
                _seriesArray[nodePos.x, nodePos.y] = _series;
                _nodesArray[nodePos.x, nodePos.y] = new Node() { gCost = newCost, parent = parentPos ,open = true};
                
                return (true, newCost);
            }

            if (newCost >= _nodesArray[nodePos.x, nodePos.y].gCost) return (false, 0);
            _nodesArray[nodePos.x, nodePos.y] = new Node() { gCost = newCost, parent = parentPos,open = true };
            return (true, newCost);
        }
        
        public void VisitSource(Vector2Int source)
        {
            _seriesArray[source.x, source.y] = _series;
            _nodesArray[source.x, source.y] = new Node() {parent = source,gCost = 0,open = true};
        }

        public bool IsVisited(Vector2Int nodePos) => _seriesArray[nodePos.x, nodePos.y] == _series;
        public void Close(Vector2Int nodePos) => _nodesArray[nodePos.x, nodePos.y].open = false;
        public bool IsOpen(Vector2Int nodePos) => IsVisited(nodePos) && _nodesArray[nodePos.x, nodePos.y].open;

        public bool TryGetParent(Vector2Int nodepos, out Vector2Int parent)
        {
            parent = _nodesArray[nodepos.x, nodepos.y].parent;
            return IsVisited(nodepos);
        }

        public void Clear()
        {
            if (_series == uint.MaxValue)
            {
                HardClear();
                return;
            }

            _series++;
        }

     

        public void HardClear()
        {
            var xLength = _seriesArray.GetLength(0);
            var yLength = _seriesArray.GetLength(0);
            for (var i = 0; i < xLength; i++)
            {
                for (var j = 0; j < yLength; j++)
                {
                    _seriesArray[i, j] = 0;
                }
            }

            _series = 1;
        }
    }
}