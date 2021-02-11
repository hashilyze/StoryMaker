using UnityEngine;

namespace Stroy.Entities {
    public class Fov_Arc : Fov {
        public float Radius { get => m_radius; set => m_radius = value; }
        public float Angle { get => m_angle; set => m_angle = value; }

        [SerializeField] private float m_radius;
        [SerializeField] private float m_angle;
        

        public override int Monitor(int targetMask, int obstacleMask, Collider2D[] results) {
            int visibleCount = 0;

            Vector2 localZero = transform.position;
            int count = Physics2D.OverlapCircleNonAlloc(localZero, Radius, results);
            for (int c = 0; c != count; ++c) {
                Vector2 distance = (Vector2)results[visibleCount].transform.position - localZero;
                if (Vector2.Angle(transform.right, distance.normalized) > Angle * 0.5f ||
                    Physics2D.Raycast(localZero, distance.normalized, distance.magnitude, obstacleMask)) {
                    results[visibleCount] = results[count - (c - visibleCount + 1)];
                    results[count - (c - visibleCount + 1)] = null;
                } else {
                    ++visibleCount;
                }
            }
            return visibleCount;
        }
        public override bool IsVisible(Collider2D target, int obstacleMask) {
            Vector2 localZero = transform.position;
            Vector2 distance = (Vector2)target.transform.position - localZero;
            return  distance.sqrMagnitude <= m_radius * m_radius
                && Vector2.Angle(transform.right, distance.normalized) <= Angle * 0.5f
                && !Physics2D.Raycast(localZero, distance.normalized, distance.magnitude, obstacleMask);
        }
#if UNITY_EDITOR
        [Header("Debug")]
        public int Resolution;

        private void OnDrawGizmos() {
            Vector3 localZero = transform.position;
            float maxNegativeAngle = -Angle * 0.5f;
            float gapAngle = Angle / Resolution;

            Vector3[] vertices = new Vector3[Resolution + 2];
            vertices[0] = Vector3.zero;

            for (int r = 0; r <= Resolution; ++r) {
                float angle = maxNegativeAngle + r * gapAngle;
                Vector3 vertex = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0f) * Radius;
                vertices[r + 1] = vertex;
            }
            float convertAngle = Vector2.SignedAngle(Vector2.right, transform.right);
            for (int v = 0; v != vertices.Length; ++v) {
                vertices[v] = (Vector3)MathfExtra.RotateMatrix(vertices[v], convertAngle) + localZero;
            }
            
            for(int i = 0; i != vertices.Length - 1; ++i) {
                Gizmos.DrawLine(vertices[i], vertices[i + 1]);
            }
            Gizmos.DrawLine(vertices[vertices.Length - 1], vertices[0]);
        }
#endif
    }
}
