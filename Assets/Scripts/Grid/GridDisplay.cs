using System;
using System.Collections.Generic;
using PathUtils;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

namespace Grid
{
    public enum DisplayObject
    {
        Obstacle,
        Path,
        Source,
        Target,
        OpenList,
        ClosedList
    }

    public class GridDisplay : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _displayPref;
        [SerializeField] private Transform _objectContainer;
        private List<IObjectPool<GameObject>> _pools = new();

        private HashSet<Vector2Int> _openMarks = new();
        private HashSet<Vector2Int> _closedMarks = new();
        private bool _hasSolutionMarks;
        private List<Vector2Int> _path;
        private bool _hasPath;
        private (GameObject, DisplayObject)[,] _displayObjects;

        public void Init(Vector2Int size)
        {
            _displayObjects = new (GameObject, DisplayObject)[size.x, size.y];
        }

        private void Awake()
        {
            for (var i = 0; i < _displayPref.Count; i++)
            {
                var capture = i;
                _pools.Add(new ObjectPool<GameObject>(() => Instantiate(_displayPref[capture], _objectContainer),
                    (g) => g.SetActive(true), (g) => g.SetActive(false)));
            }
        }

        private GameObject Get(DisplayObject type) => _pools[(int)type].Get();
        private void Release(GameObject obj, DisplayObject type) => _pools[(int)type].Release(obj);

        public void Clear(Vector2Int point)
        {
            var (obj, type) = _displayObjects[point.x, point.y];
            Release(obj, type);
        }

        public void Hide(Vector2Int point)
        {
            var (obj, type) = _displayObjects[point.x, point.y];
            if (_closedMarks.Contains(point))
            {
                _closedMarks.Remove(point);
                Release(obj,type);
                return;
            }

            if (_openMarks.Contains(point))
            {
                _openMarks.Remove(point);
                Release(obj,type);
                return;
            }
        }


        public void Display(Vector2Int point, DisplayObject type)
        {
            var obj = Get(type);
            if (type == DisplayObject.OpenList)
            {
                _hasSolutionMarks = true;
                _openMarks.Add(point);
            }

            if (type == DisplayObject.ClosedList)
            {
                _hasSolutionMarks = true;
                _closedMarks.Add(point);
            }

            obj.transform.position = new Vector3(point.x, 0, point.y) + new Vector3(0.5f, 0, 0.5f);
            _displayObjects[point.x, point.y] = (obj, type);
        }

        public void TryClearPath()
        {
            if (!_hasPath) return;
            _hasPath = false;
            for (var i = 1; i < _path.Count - 1; i++)
            {
                Clear(_path[i]);
            }
        
        }

        public void TryClearSolutionMarks()
        {
            if (!_hasSolutionMarks) return;
            _hasSolutionMarks = false;
            foreach (var openMark in _openMarks)
            {
                Clear(openMark);
            }

            foreach (var openMark in _closedMarks)
            {
                Clear(openMark);
            }
            _openMarks.Clear();
            _closedMarks.Clear();
        }

        public void DisplayPath(List<Vector2Int> path)
        {
         
            TryClearPath();
            _hasPath = true;
            _path = path;
            for (var i = 1; i < _path.Count - 1; i++)
            {
                Display(_path[i], DisplayObject.Path);
            }
        }

        public void Init(Vector2Int size, IMapData mapData)
        {
            Init(size);
            for (int i = 0; i < mapData.size.x; i++)
            {
                for (int j = 0; j < mapData.size.y; j++)
                {
                    if (!mapData[new Vector2Int(i, j)])
                        Display(new Vector2Int(i, j), DisplayObject.Obstacle);
                }
            }
        }

    }
}