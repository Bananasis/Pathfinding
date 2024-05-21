using UnityEngine;

namespace Utils
{
    public static class Utils
    {
        public static Vector2 Clamp(this Rect rect, Vector2 vector)
        {
            return new Vector2(Mathf.Clamp(vector.x, rect.xMin, rect.xMax), 
                Mathf.Clamp(vector.y, rect.yMin, rect.yMax));
        }
    
        public static Vector3 Clamp(this Rect rect, Vector3 vector)
        {
            return new Vector3(Mathf.Clamp(vector.x, rect.xMin, rect.xMax),vector.y, 
                Mathf.Clamp(vector.z, rect.yMin, rect.yMax));
        }
    }
}