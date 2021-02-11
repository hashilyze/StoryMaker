using UnityEngine;


namespace Stroy.Entities {
    public class Fov_Circle : Fov {
        public float Radius { get => m_radius; set => m_radius = value; }

        [SerializeField] private float m_radius;


        public override int Monitor(int targetMask, int obstacleMask, Collider2D[] results) {
            int visibleCount = 0;

            Vector2 localZero = transform.position;
            int count = Physics2D.OverlapCircleNonAlloc(localZero, Radius, results);
            for (int c = 0; c != count; ++c) {
                Vector2 distance = (Vector2)results[visibleCount].transform.position - localZero;
                if (Physics2D.Raycast(localZero, distance.normalized, distance.magnitude, obstacleMask)) {
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
            return distance.sqrMagnitude <= m_radius * m_radius
                && !Physics2D.Raycast(localZero, distance.normalized, distance.magnitude, obstacleMask);
        }
#if UNITY_EDITOR
        private void OnDrawGizmos() {
            Gizmos.DrawWireSphere(transform.position, Radius);
        }
#endif
    }
}
