using UnityEngine;

namespace StroyMaker.Physics {
    /// <summary>Generic primative shape of body to query or physics collision</summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public abstract class Shape2D : MonoBehaviour {
        public Rigidbody2D Rigidbody => m_rigidbody;
        [SerializeField] private Rigidbody2D m_rigidbody;


        protected virtual void Reset () {
            m_rigidbody = GetComponent<Rigidbody2D>();
        }
        protected virtual void Awake () {
            if(m_rigidbody == null) {
                m_rigidbody = GetComponent<Rigidbody2D>();
            }
        }


        public abstract Collider2D Collider { get; }


        // Query
        public abstract int Sweep (Vector2 position, float rotation, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask);
        public abstract RaycastHit2D Sweep (Vector2 position, float rotation, Vector2 direction, float distance, int layerMask);
        public abstract int Overlap (Vector2 position, float rotation, Collider2D[] results, int layerMask);


        // Physics
        public bool ComputePenetration (Vector2 position, float rotation, Collider2D other, out Vector2 direction, out float distance, out Vector2 pointA, out Vector2 pointB) {
            Vector2 savedPosition = m_rigidbody.position;
            float savedRotation = m_rigidbody.rotation;

            bool isDiffPos = savedPosition != position;
            bool isDiffRot = savedRotation != rotation;

            if (isDiffPos) m_rigidbody.position = position;
            if (isDiffRot) m_rigidbody.rotation = rotation;

            ColliderDistance2D cd = Physics2D.Distance(Collider, other);

            if (isDiffPos) m_rigidbody.position = savedPosition;
            if (isDiffRot) m_rigidbody.rotation = savedRotation;

            if (cd.isOverlapped) {
                direction = cd.distance < 0f ? -cd.normal : cd.normal;
                distance = -(cd.distance + 2f * Physics2D.defaultContactOffset);
                pointA = cd.pointA + direction * Physics2D.defaultContactOffset;
                pointB = cd.pointB - direction * Physics2D.defaultContactOffset;
                
                return true;
            } else {
                direction = Vector2.zero;
                distance = 0f;
                pointA = pointB = Vector2.zero;

                return false;
            }
        }
        public int GetContacts(Vector2 position, float rotation, ContactPoint2D[] results, int layerMask) {
            Vector2 backupPos = m_rigidbody.position;
            float backupRot = m_rigidbody.rotation;

            bool isDiffPos = backupPos != position;
            bool isDiffRot = backupRot != rotation;

            if (isDiffPos) m_rigidbody.position = position;
            if (isDiffRot) m_rigidbody.rotation = rotation;

            int count = Collider.GetContacts(new ContactFilter2D() { useLayerMask = true, layerMask = layerMask }, results);

            if (isDiffPos) m_rigidbody.position = backupPos;
            if (isDiffRot) m_rigidbody.rotation = backupRot;

            return count;
        }
    }
}
