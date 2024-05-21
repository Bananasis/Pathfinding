using System;
using System.Collections.Generic;
using PathUtils;
using UnityEngine;

namespace Grid
{
    public class SolverHolder
    {
        private Dictionary<SolverType, ISolver> _initialized = new Dictionary<SolverType, ISolver>();

        private Vector2Int _size;

        public ISolver Get(SolverType type)
        {
            if (_initialized.TryGetValue(type, out var solver)) return _initialized[type];
            solver = MakeSolver(type);
            solver.Init(_size);
            _initialized[type] = solver;
            return solver;
        }


        private ISolver MakeSolver(SolverType type) => type switch
        {
            SolverType.LazyAStar => new LazyAStarSolver(),
            SolverType.AStar => new StandartAStarSolver(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        public void SetMapSize(Vector2Int size)
        {
            _size = size;
            _initialized.Clear();
        }

        public void InitAll()
        {
            var solvers = Enum.GetValues(typeof(SolverType));

            for (int i = 0; i < solvers.Length; i++)
            {
                var type = (SolverType)i;
                if (_initialized.TryGetValue(type, out var solver)) continue;
                solver = MakeSolver(type);
                solver.Init(_size);
                _initialized[type] = solver;
            }
        }
    }

    public enum SolverType
    {
        LazyAStar,
        AStar,
    }
}