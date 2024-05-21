using System;
using UnityEngine;

namespace PathUtils
{
    public static class VectorIntUtils
    {
        public static bool Contains(this Vector2Int size, Vector2Int point)
            => point.x >= 0 && point.y >= 0 && point.x < size.x && point.y < size.y;

        private static readonly Vector2Int[] dirs = { new(0, 1), new(-1, 0), new(0, -1), new(1, 0) };

        public static void GetNeighbors(this Vector2Int node, Vector2Int target, (Vector2Int, bool)[] array)
        {
            var targetDir = target - node;
            // Unrolling loop because .NET ignores the opportunity(
            // Actual loop
            // for (int i = 0; i < 4; i++)
            // {
            //     var dir = dirs[i];
            //     var neighbour = dir + node;
            //     array[i] = (neighbour,
            //         dir.x != 0 && Math.Sign(targetDir.x) == Math.Sign(dir.x) ||
            //         dir.y != 0 && Math.Sign(targetDir.y) == Math.Sign(dir.y));
            // }
            
            //Unrolled version
            //Indexes inversed to avoid array size checks
            var dir = dirs[3];
            var neighbour = dir + node;
            array[3] = (neighbour,
                dir.x != 0 && Math.Sign(targetDir.x) == Math.Sign(dir.x) ||
                dir.y != 0 && Math.Sign(targetDir.y) == Math.Sign(dir.y));
            dir = dirs[2];
            neighbour = dir + node;
            array[2] = (neighbour,
                dir.x != 0 && Math.Sign(targetDir.x) == Math.Sign(dir.x) ||
                dir.y != 0 && Math.Sign(targetDir.y) == Math.Sign(dir.y));
            dir = dirs[1];
            neighbour = dir + node;
            array[1] = (neighbour,
                dir.x != 0 && Math.Sign(targetDir.x) == Math.Sign(dir.x) ||
                dir.y != 0 && Math.Sign(targetDir.y) == Math.Sign(dir.y));
            dir = dirs[0];
            neighbour = dir + node;
            array[0] = (neighbour,
                dir.x != 0 && Math.Sign(targetDir.x) == Math.Sign(dir.x) ||
                dir.y != 0 && Math.Sign(targetDir.y) == Math.Sign(dir.y));
        }
        public static int Dist(this Vector2Int node,Vector2Int other)
        {
            return Math.Abs(other.x - node.x) + Math.Abs(other.y - node.y);
        }
    }
    // public struct IntVector2
    // {
    //     public bool Equals(IntVector2 other)
    //     {
    //         return x == other.x && y == other.y;
    //     }
    //
    //     public override bool Equals(object obj)
    //     {
    //         return obj is IntVector2 other && Equals(other);
    //     }
    //
    //     public override int GetHashCode()
    //     {
    //         return HashCode.Combine(x, y);
    //     }
    //
    //     public int x;
    //     public int y;
    //
    //    
    //
    //     public static bool operator ==(IntVector2 obj1, IntVector2 obj2)
    //     {
    //         return obj2.x == obj1.x && obj1.y == obj2.y;
    //     }
    //
    //     public static bool operator !=(IntVector2 obj1, IntVector2 obj2)
    //     {
    //         return !(obj2 == obj1);
    //     }
    //
    //     public IntVector2(int x, int y)
    //     {
    //         this.x = x;
    //         this.y = y;
    //     }
    //
    //     public bool SameDist(IntVector2 target, Direction dir)
    //     {
    //         var direction = dirs[(int)dir];
    //         return Math.Sign(target.x - x) == direction.x && direction.x != 0 ||
    //                Math.Sign(target.y - y) == direction.y && direction.y != 0;
    //     }
    //
    //     public bool Contains(IntVector2 point) => point.x >= 0 && point.y >= 0 && point.x < x && point.y < y;
    //
    //
    //     
    //
    //     public IntVector2 Shift(Direction dir)
    //     {
    //         var dirVec = dirs[(int)dir];
    //         return new IntVector2(dirVec.x + x, dirVec.y + y);
    //     }
    // }
}