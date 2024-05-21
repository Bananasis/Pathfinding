using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace PathUtils
{
    public interface ISolver
    {
        public void Clear();
        public void Init(Vector2Int size);

        public List<Vector2Int> SolveWithBenchMark(IMapData map, Vector2Int source, Vector2Int target,
            out long timeElapsedMs)
        {
            var watch = new Stopwatch();
            watch.Start();
            var path = Solve(map, source, target);
            watch.Stop();
            timeElapsedMs = watch.ElapsedMilliseconds;
            return path;
        }

        public List<Vector2Int> Solve(IMapData map, Vector2Int source, Vector2Int target);

        public IEnumerable<(SolutionMark, Vector2Int, Vector2Int)> SolveWithSteps(IMapData map, Vector2Int source,
            Vector2Int target);
        
        
    }
}