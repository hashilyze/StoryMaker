using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stroy.Entities {
    [RequireComponent(typeof(Rigidbody2D))]
    public class Bullet : MonoBehaviour {
        public float Speed;
        public float Range;
        public float Damage;
        public string TargetTag;

        private Rigidbody2D m_rigidbody;
        private float m_elapsedDistance;


        private void Awake() {
            m_rigidbody = GetComponent<Rigidbody2D>();
        }
        private void FixedUpdate() {
            float deltaTime = Time.fixedDeltaTime;
            float deltaDistance = Speed * deltaTime;
            
            m_rigidbody.MovePosition(m_rigidbody.position + deltaDistance * (Vector2)transform.right);

            m_elapsedDistance += deltaDistance;
            if (m_elapsedDistance > Range) {
                Destroy(gameObject);
            }
        }
        private void OnTriggerEnter2D(Collider2D collision) {
            int target = 0x01 << collision.gameObject.layer;
            // Hit to block
            if ((target & Platforms.PlatformConstants.L_BlockMask) == target) {
                Destroy(gameObject);
            }
            // Hit to target
            if (collision.CompareTag(TargetTag)) {
                collision.GetComponent<Health>().TakeDamage(Damage);
                Destroy(gameObject);
            }
        }
    }
}
