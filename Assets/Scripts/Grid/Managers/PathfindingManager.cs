using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PathUtils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Grid
{
    public class PathfindingManager : MonoBehaviour
    {
        [SerializeField] private Button _generatePathButton;
        [SerializeField] private Button _generatePathWithSolutionButton;
        [SerializeField] private GridDisplay _display;
        [SerializeField, Range(0.001f, 1)] private float _stepDelay = 0.001f;
        [SerializeField] private bool _instantSolve;
        [SerializeField] private CharController _char;
        [SerializeField] private TMP_Text timeElapsed;
        [SerializeField] private TMP_Dropdown _solverOption;
        [SerializeField] private TMP_Text _testResult;
        [SerializeField] private Toggle _autoSolve;
        [SerializeField] private Button _doRandomTestsButton;
        [SerializeField] private MapManager _mapManager;
        private SolverHolder _solverHolder;
        private Vector2Int _target;
        private Vector2Int _source;
        private bool _hasTarget;
        private bool _hasSource;
        private SolverType _solverType;
        private ISolver _solver;
        private WaitForFixedUpdate wait = new();

        #region eventFunctions

        private void Awake()
        {
            _solverHolder = new SolverHolder();
        }

        private void OnEnable()
        {
            _solverOption.ClearOptions();
            _solverOption.AddOptions(Enum.GetNames(typeof(SolverType)).ToList());
            _generatePathButton.onClick.AddListener(TryGeneratePath);
            _generatePathWithSolutionButton.onClick.AddListener(StartOverTimeSolution);
            _solverOption.onValueChanged.AddListener(SetSolver);
            _doRandomTestsButton.onClick.AddListener(DoRandomTests);
        }

        private void OnDisable()
        {
            _generatePathButton.onClick.RemoveListener(TryGeneratePath);
            _generatePathWithSolutionButton.onClick.RemoveListener(StartOverTimeSolution);
            _solverOption.onValueChanged.RemoveListener(SetSolver);
            _solverOption.onValueChanged.RemoveListener(SetSolver);
        }

        #endregion

        #region pathfinding

        private void DoRandomTests()
        {
            if (!_mapManager.hasLoadedMap) return;
            var solvers = Enum.GetValues(typeof(SolverType));
            long[] timeTaken = new long[solvers.Length];
            long[] stepsTaken = new long[solvers.Length];
            _solverHolder.InitAll();
            var map = _mapManager.map;
            for (int i = 0; i < 100; i++)
            {
                int tries = 0;
                Vector2Int source;
                do
                {
                    tries++;
                    source = new Vector2Int(Random.Range(0, map.size.x), Random.Range(0, map.size.y));
                    if (tries > 100) break;
                } while (!map[source]);

                tries = 0;
                Vector2Int target;
                do
                {
                    tries++;
                    target = new Vector2Int(Random.Range(0, map.size.x), Random.Range(0, map.size.y));
                    if (tries > 100) break;
                } while (!map[target]);

                if (tries > 100)
                    continue;

                for (int j = 0; j < solvers.Length; j++)
                {
                    var solver = _solverHolder.Get((SolverType)j);
                    var path = solver.SolveWithBenchMark(map, source, target, out var time);
                    timeTaken[j] += time;
                    stepsTaken[j] += path.Count;
                }
            }

            var sb = new StringBuilder();
            for (var j = 0; j < solvers.Length; j++)

            {
                sb.Append(
                    $"{Enum.GetName(typeof(SolverType), (SolverType)j)} time: {timeTaken[j] / 100.0f} steps: {stepsTaken[j]}\n");
            }

            _testResult.text = sb.ToString();
            _testResult.gameObject.SetActive(true);
        }

        private void TryGeneratePath()
        {
            if (!_mapManager.hasLoadedMap) return;
            StopAndClear();
            if (!(_hasSource && _hasTarget))
            {
                timeElapsed.text = "No Target or Source set. Pathfinding is impossible";
                timeElapsed.gameObject.SetActive(true);
                return;
            }

            var path = _solver.SolveWithBenchMark(_mapManager.map, _source, _target, out var time);
            timeElapsed.text = $"{time}ms {path.Count} steps";
            timeElapsed.gameObject.SetActive(true);
            if (path.Count == 0)
            {
                timeElapsed.text = $"No available path from {_source} to {_target} ";
                timeElapsed.gameObject.SetActive(true);
                return;
            }

            _char.RunPath(path.Select(p => new Vector2(p.x, p.y) + new Vector2(0.5f, 0.5f)).ToList());
            _display.DisplayPath(path);
        }


        private void StartOverTimeSolution()
        {
            if (!_mapManager.hasLoadedMap) return;
            StopAndClear();
            StartCoroutine(OverTimeSolution());
        }

        private IEnumerator OverTimeSolution()
        {
            var timeLeft = Time.fixedDeltaTime;
            foreach (var _ in TryGeneratePathWithSolution())
            {
                if (_instantSolve) continue;
                while (timeLeft <= 0)
                {
                    timeLeft += Time.fixedDeltaTime;
                    yield return wait;
                }

                timeLeft -= _stepDelay;
            }
        }

        private IEnumerable TryGeneratePathWithSolution()
        {
            if (!(_hasSource && _hasTarget))
            {
                Debug.LogWarning("No Target or Source is set. Pathfinding is impossible");
                yield break;
            }

            List<Vector2Int> path = new List<Vector2Int>();
            foreach (var (mark, node, parent) in _solver.SolveWithSteps(_mapManager.map, _source, _target))
            {
                switch (mark)
                {
                    case SolutionMark.AddedToOpen:
                        _display.Hide(node);
                        _display.Display(node, DisplayObject.OpenList);
                        break;
                    case SolutionMark.AddedToClose:
                        _display.Hide(node);
                        _display.Display(node, DisplayObject.ClosedList);
                        break;

                    case SolutionMark.Path:
                        _display.Hide(node);
                        path.Add(node);
                        break;
                    case SolutionMark.Start:
                        path.Add(node);
                        break;
                    case SolutionMark.Target:
                        path.Add(node);
                        break;
                    case SolutionMark.Fail:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                yield return null;
            }

            if (path.Count == 0)
            {
                Debug.LogWarning($"No available path from {_source} to {_target} ");
                yield break;
            }

            _char.RunPath(path.Select(p => new Vector2(p.x, p.y) + new Vector2(0.5f, 0.5f)).ToList());
            _display.DisplayPath(path);
        }

        #endregion

        #region listeners

        public bool TrySetSource(Vector2Int point)
        {
            if (!_mapManager.map[point]) return false;
            if (_hasTarget && _target == point)
            {
                _hasTarget = false;
                _display.Clear(point);
            }

            if (_hasSource)
            {
                _display.Clear(_source);
            }

            _source = point;
            _display.Display(point, DisplayObject.Source);
            _hasSource = true;
            if (_hasTarget && _autoSolve.isOn) TryGeneratePath();
            return true;
        }

        public bool TrySetTarget(Vector2Int point)
        {
            if (!_mapManager.map[point]) return false;
            if (_hasSource && _source == point)
            {
                _hasSource = false;
                _display.Clear(point);
            }

            if (_hasTarget)
            {
                _display.Clear(_target);
            }

            _display.Display(point, DisplayObject.Target);
            _target = point;
            _hasTarget = true;
            if (_hasSource && _autoSolve.isOn) TryGeneratePath();
            return true;
        }

        private void SetSolver(int option)
        {
            _solverType = (SolverType)option;
            if (!_mapManager.hasLoadedMap) return;
            _solver.Clear();
            _solver = _solverHolder.Get(_solverType);
        }

        #endregion


        public void ClearAll()
        {
            StopAndClear();
            if (_hasTarget)
            {
                _display.Clear(_target);
            }

            if (_hasSource)
            {
                _display.Clear(_source);
            }

            _hasSource = false;
            _hasTarget = false;
        }


        public void StopAndClear()
        {
            _testResult.gameObject.SetActive(false);
            timeElapsed.gameObject.SetActive(false);
            _char.Stop();
            StopAllCoroutines();
            _solver.Clear();
            _display.TryClearPath();
            _display.TryClearSolutionMarks();
        }

     


        public void Init(Vector2Int size)
        {
            _solverHolder.SetMapSize(size);
            _solver = _solverHolder.Get(_solverType);
           
        }

        public void ClearPoint(Vector2Int point)
        {
            if (_hasTarget && point == _target)
            {
                _hasTarget = false;
                _display.Clear(point);
            }

            if (_hasSource && point == _source)
            {
                _hasSource = false;
                _display.Clear(point);
            }
        }
    }
}