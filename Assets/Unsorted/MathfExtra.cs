using UnityEngine;

public static class MathfExtra {
    public static Vector2 RotateMatrix(Vector2 point, float angle) {
        float sin = Mathf.Sin(angle * Mathf.Deg2Rad);
        float cos = Mathf.Cos(angle * Mathf.Deg2Rad);
        return new Vector2(cos * point.x - sin * point.y, sin * point.x + cos * point.y);
    }
    public static void RotateMatrix(Vector2[] points, float angle) {
        float sin = Mathf.Sin(angle * Mathf.Deg2Rad);
        float cos = Mathf.Cos(angle * Mathf.Deg2Rad);
        for(int beg = 0, end = points.Length - 1; beg != end; ++beg) {
            float x = points[beg].x;
            float y = points[beg].y;
            points[beg] = new Vector2(cos * x - sin * y, sin * x + cos * y);
        }
    }
}
