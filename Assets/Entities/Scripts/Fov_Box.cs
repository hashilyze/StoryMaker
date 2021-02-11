using UnityEngine;

namespace Stroy.Entities {
    public class Fov_Box : Fov {
        public Vector2 Size { get => m_size; set => m_size = value; }
        public Vector2 Pivot { get => m_pivot; set => m_pivot = value; }

        [SerializeField] private Vector2 m_size;
        [SerializeField] private Vector2 m_pivot;

        public override int Monitor(int targetMask, int obstacleMask, Collider2D[] results) {
            int visibleCount = 0;

            Vector2 localZero = transform.position;
            float angle = Vector2.SignedAngle(Vector2.right, transform.right);
            Vector2 localCenter = MathfExtra.RotateMatrix(m_size * (Vector2.one * 0.5f - m_pivot), angle);

            int count = Physics2D.OverlapBoxNonAlloc(localZero + localCenter, m_size, Vector2.SignedAngle(Vector2.right, transform.right), results);
            
            for (int c = 0; c != count; ++c) {
                Vector2 distance = (Vector2)results[visibleCount].transform.position - localZero;
                RaycastHit2D hit = Physics2D.Raycast(localZero, distance.normalized, distance.magnitude, obstacleMask);

                if (hit) {
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
            Vector2 targetPos = target.transform.position;
            Vector2 distance = targetPos - localZero;

            Vector2 min = -m_pivot * m_size;
            Vector2 max = (Vector2.one - m_pivot) * m_size;
            Vector2 orthogonalDist = new Vector2(Vector2.Dot(transform.right, distance), Vector2.Dot(transform.up, distance));

            return min.x <= orthogonalDist.x && orthogonalDist.x <= max.x && min.y <= orthogonalDist.y && orthogonalDist.y <= max.y
                && !Physics2D.Raycast(localZero, distance.normalized, distance.magnitude, obstacleMask);
        }
#if UNITY_EDITOR
        private const int E_COUNT = 4;
        private readonly Vector3[] e_points = new Vector3[E_COUNT];

        private void OnDrawGizmos() {
            e_points[0] = -m_size * m_pivot;
            e_points[1] = e_points[0] + m_size.x * Vector3.right;
            e_points[2] = e_points[1] + m_size.y * Vector3.up;
            e_points[3] = e_points[2] + m_size.x * Vector3.left;

            float angle = Vector2.SignedAngle(Vector2.right, transform.right);
            if (angle != 0f) {
                for (int i = 0; i != e_points.Length; ++i) {
                    e_points[i] = MathfExtra.RotateMatrix(e_points[i], angle);
                }
            }
            Vector3 localZero = transform.position;
            for (int i = 0; i != e_points.Length; ++i) {
                e_points[i] += localZero;
            }
            
            for (int i = 0; i != e_points.Length - 1; ++i) {
                Gizmos.DrawLine(e_points[i], e_points[i + 1]);
            }
            Gizmos.DrawLine(e_points[e_points.Length - 1], e_points[0]);
        }
#endif
    }
}
