using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Stroy {
    namespace EC.Samples {
        [RequireComponent(typeof(Rigidbody2D))]
        public class DynamicController : MonoBehaviour {
            public Vector2 Velocity;
            public float Span;

            private Rigidbody2D m_rigidbody;
            private float direction = 1f;
            private float leftSpan;

            private void Awake() {
                m_rigidbody = GetComponent<Rigidbody2D>();
                m_rigidbody.velocity = Velocity;
                leftSpan = Span;
            }
            private void FixedUpdate() {
                leftSpan -= m_rigidbody.velocity.magnitude * Time.fixedDeltaTime;
                if(leftSpan < 0) {
                    direction *= -1f;
                    m_rigidbody.velocity = Velocity * direction;
                    leftSpan = Span;
                }
            }
        }
    }
}
