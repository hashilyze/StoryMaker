using System.Collections.Generic;
using UnityEngine;

namespace StroyMaker.Physics {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public class KinematicPlatformController2D : MonoBehaviour {
        #region Public
        public Rigidbody2D Rigidbody => m_rigidbody;
        public Collider2D GetCollider (int offset) => m_colliders[offset];
        public int ColliderCount => m_colliders.Count;
            
        public Vector2 Velocity { get => m_velocity; set => m_velocity = value; }
        #endregion

        #region Private
        // Component
        [SerializeField] private Rigidbody2D m_rigidbody;
        private readonly List<Collider2D> m_colliders = new List<Collider2D>();
        // Motor
        [SerializeField] private Vector2 m_velocity;


        private void Reset () {
            m_rigidbody = GetComponent<Rigidbody2D>();
            m_rigidbody.isKinematic = true;
        }
        private void Awake () {
            if(m_rigidbody == null) {
                m_rigidbody = GetComponent<Rigidbody2D>();
                m_rigidbody.isKinematic = true;
            }

            m_colliders.Capacity = m_rigidbody.attachedColliderCount;
            m_rigidbody.GetAttachedColliders(m_colliders);

            PlatformHolder.Add(this);
        }
        private void OnDestroy () {
            PlatformHolder.Remove(this);
        }

        private void FixedUpdate () {
            float deltaTime = Time.fixedDeltaTime;
            if (m_rigidbody.velocity != m_velocity) {
                m_rigidbody.velocity = m_velocity;
            }
            if(m_velocity != Vector2.zero) {
                m_rigidbody.MovePosition(m_rigidbody.position + m_velocity * deltaTime);
            }
        }
        #endregion
    }
}