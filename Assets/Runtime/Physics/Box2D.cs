using UnityEngine;

namespace StroyMaker.Physics {
    [RequireComponent(typeof(BoxCollider2D))]
    public class Box2D : Shape2D {
        public override Collider2D Collider => m_box;
        [SerializeField] private BoxCollider2D m_box;

        protected override void Reset () {
            base.Reset();
            m_box = GetComponent<BoxCollider2D>();
        }
        protected override void Awake () {
            if(m_box == null) {
                m_box = GetComponent<BoxCollider2D>();
            }
        }


        public override int Sweep (Vector2 position, float rotation, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask) {
            return Physics2D.BoxCastNonAlloc(position, m_box.size, rotation, direction, results, distance, layerMask);
        }
        public override RaycastHit2D Sweep (Vector2 position, float rotation, Vector2 direction, float distance, int layerMask) {
            return Physics2D.BoxCast(position, m_box.size, rotation, direction, distance, layerMask);
        }
        public override int Overlap (Vector2 position, float rotation, Collider2D[] results, int layerMask) {
            return Physics2D.OverlapBoxNonAlloc(position, m_box.size, rotation, results, layerMask);
        }
    }
}
