using UnityEngine;

namespace StroyMaker.Physics {
    [RequireComponent(typeof(CapsuleCollider2D))]
    public class Capsule2D : Shape2D {
        public override Collider2D Collider => m_capsule;
        [SerializeField] private CapsuleCollider2D m_capsule;

        protected override void Reset () {
            base.Reset();
            m_capsule = GetComponent<CapsuleCollider2D>();
        }
        protected override void Awake () {
            if (m_capsule == null) {
                m_capsule = GetComponent<CapsuleCollider2D>();
            }
        }

        public override int Sweep (Vector2 position, float rotation, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask) {
            return Physics2D.CapsuleCastNonAlloc(position, m_capsule.size, m_capsule.direction, rotation, direction, results, distance, layerMask);
        }
        public override RaycastHit2D Sweep (Vector2 position, float rotation, Vector2 direction, float distance, int layerMask) {
            return Physics2D.CapsuleCast(position, m_capsule.size, m_capsule.direction, rotation, direction, distance, layerMask);
        }
        public override int Overlap (Vector2 position, float rotation, Collider2D[] results, int layerMask) {
            return Physics2D.OverlapCapsuleNonAlloc(position, m_capsule.size, m_capsule.direction, rotation, results, layerMask);
        }
    }
}
