using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stroy {
    namespace Platform {
        public class DropPlatformAgent : DynamicPlatformAgent {
            [SerializeField] private float m_dropTerm;
            [SerializeField] private float m_dropSpeed;
            private bool m_playerTouch;
            private float m_elapsedTouch;

            public override void HandlePlatform(float deltaTime) {
                if (m_playerTouch) {
                    m_elapsedTouch += deltaTime;

                    if(m_elapsedTouch > m_dropTerm) {
                        Platform.Rigidbody.velocity = Vector2.down * m_dropSpeed;
                    }
                } else {
                    m_elapsedTouch = 0f;
                }
            }

            private void OnCollisionEnter2D(Collision2D collision) {
                if(collision.collider.CompareTag("Player")) {
                    m_playerTouch = true;
                }
            }
        }
    }
}
