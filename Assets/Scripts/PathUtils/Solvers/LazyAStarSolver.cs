using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PathUtils
{
    public class LazyAStarSolver : ISolver
    {
        private IVisitLedger _visits;
        private readonly TwoBucketHeap<Vector2Int> _openList = new();

        public void Clear()
        {
            _visits.Clear();
            _openList.Clear();
        }

        public void Init(Vector2Int size)
        {
            _visits = new SerialVisitLedger(size);
        }


        public List<Vector2Int> Solve(IMapData map, Vector2Int source, Vector2Int target)
        {
            _visits.Visit(source, source);
            _openList.InsertInBeginning(source);
            (Vector2Int, bool)[] neighbors = new (Vector2Int, bool)[4];
            while (_openList.TryPop(out var currentNode))
            {
                if (!_visits.IsOpen(currentNode)) continue;
                _visits.Close(currentNode);
                if (currentNode == target)
                {
                    List<Vector2Int> path = ConstructPath(target);
                    Clear();
                    return path;
                }

                currentNode.GetNeighbors(target, neighbors);

                for (int i = 0; i < 4; i++)
                {
                    Vector2Int visitedNode;
                    bool isSameDist;
                    (visitedNode, isSameDist) = neighbors[i];
                    if (!map[visitedNode]) continue;
                    var (better, cost) = _visits.Visit(visitedNode, currentNode);
                    if(!better) continue;
                    if (isSameDist)
                    {
                        _openList.InsertInBeginning(visitedNode);
                        continue;
                    }

                    _openList.InsertAtEnd(visitedNode);
                }
            }

            Clear();
            return new List<Vector2Int>();
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

        public IEnumerable<(SolutionMark, Vector2Int, Vector2Int)> SolveWithSteps(IMapData map, Vector2Int source,
            Vector2Int target)
        {
            _visits.Visit(source, source);
            _openList.InsertInBeginning(source);
            yield return (SolutionMark.Start, source, default);
            (Vector2Int, bool)[] neighbors = new (Vector2Int, bool)[4];
            while (_openList.TryPop(out var currentNode))
            {
                
                if (!_visits.IsOpen(currentNode)) continue;
                _visits.Close(currentNode);
                if (currentNode == target)
                {
                    List<Vector2Int> path = ConstructPath(target);
                    Clear();
                    for (var i = 1; i < path.Count - 1; i++)
                    {
                        yield return (SolutionMark.Path, path[i], default);
                    }

                    yield return (SolutionMark.Target, target, default);
                    yield break;
                }

                if (currentNode != source)
                    yield return (SolutionMark.AddedToClose, currentNode, default);

                currentNode.GetNeighbors(target, neighbors);

                for (int i = 0; i < 4; i++)
                {
                    Vector2Int visitedNode;
                    bool isSameDist;
                    (visitedNode, isSameDist) = neighbors[i];
                    if (!map[visitedNode]) continue;
                    if (_visits.IsOpen(visitedNode)) continue;
                    var (better, cost) = _visits.Visit(visitedNode, currentNode);
                    if(!better) continue;
                    if (visitedNode != target)
                        yield return (SolutionMark.AddedToOpen, visitedNode, currentNode);
                    if (isSameDist)
                    {
                        _openList.InsertInBeginning(visitedNode);
                        continue;
                    }

                    _openList.InsertAtEnd(visitedNode);
                }
            }

            yield return (SolutionMark.Fail, default, default);
            Clear();
        }
    }
}