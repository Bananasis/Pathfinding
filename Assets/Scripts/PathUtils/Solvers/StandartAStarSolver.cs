using System.Collections.Generic;
using UnityEngine;

namespace PathUtils
{
    public class StandartAStarSolver : ISolver
    {
        private IVisitLedger _visits;

        private readonly BinaryHeap<Vector2Int> _openList = new BinaryHeap<Vector2Int>();

        public void Clear()
        {
            _visits.Clear();
            _openList.Clear();
        }

        public void Init(Vector2Int size)
        {
            _visits = new SerialVisitLedger(size);
        }

        private List<Vector2Int> ConstructPath(Vector2Int target)
        {
            List<Vector2Int> path = new();
            Vector2Int curNode = target;
            Vector2Int prevNode;
            while (_visits.TryGetParent(curNode, out prevNode))
            {
                path.Add(curNode);
                if (prevNode == curNode) break;
                curNode = prevNode;
            }

            path.Reverse();
            return path;
        }

        public List<Vector2Int> Solve(IMapData map, Vector2Int source, Vector2Int target)
        {
            _openList.Add(source, source.Dist(target));
            _visits.VisitSource(source);

            (Vector2Int, bool)[] neighbors = new (Vector2Int, bool)[4];
            while (_openList.Count > 0)
            {
                var curNodePos = _openList.ExtractMin();
                if (!_visits.IsOpen(curNodePos)) continue;
                _visits.Close(curNodePos);
                if (curNodePos == target)
                {
                    var path = ConstructPath(target);
                    Clear();
                    return path;
                }

                curNodePos.GetNeighbors(target, neighbors);

                for (int i = 0; i < 4; i++)
                {
                    var (visitedNode, _) = neighbors[i];
                    if (!map[visitedNode]) continue;
                    var (isBetter, gCost) = _visits.Visit(visitedNode, curNodePos);
                    if (!isBetter) continue;
                    _openList.Add(visitedNode, gCost + visitedNode.Dist(target));
                }
            }

            Clear();
            return new List<Vector2Int>();
        }

        public IEnumerable<(SolutionMark, Vector2Int, Vector2Int)> SolveWithSteps(IMapData map, Vector2Int source,
            Vector2Int target)
        {
            _openList.Add(source, source.Dist(target));
            _visits.VisitSource(source);
            yield return (SolutionMark.Start, source, default);
            var tiebreakerMultiplier = 1 - 1.0f / (map.size.x * map.size.y);
            (Vector2Int, bool)[] neighbors = new (Vector2Int, bool)[4];
            while (_openList.Count > 0)
            {
                var curNodePos = _openList.ExtractMin();
                if (!_visits.IsOpen(curNodePos)) continue;
                _visits.Close(curNodePos);

                if (curNodePos == target)
                {
                    var path = ConstructPath(target);
                    Clear();
                    for (var i = 1; i < path.Count - 1; i++)
                    {
                        yield return (SolutionMark.Path, path[i], default);
                    }

                    yield return (SolutionMark.Target, target, default);
                    yield break;
                }

                if (curNodePos != source)
                    yield return (SolutionMark.AddedToClose, curNodePos, default);
                curNodePos.GetNeighbors(target, neighbors);

                for (int i = 0; i < 4; i++)
                {
                    var (visitedNode, _) = neighbors[i];
                    if (!map[visitedNode]) continue;
                    var (isBetter, gCost) = _visits.Visit(visitedNode, curNodePos);
                    if (!isBetter) continue;


                    _openList.Add(visitedNode, gCost * tiebreakerMultiplier + visitedNode.Dist(target));
                    if (curNodePos != target)
                    {
                        yield return (SolutionMark.AddedToOpen, visitedNode, default);
                    }
                }
            }

            yield return (SolutionMark.Fail, default, default);
        }
    }
}