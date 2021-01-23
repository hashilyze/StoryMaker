using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stroy.Platform;

namespace Stroy {
    namespace Entity {
        [RequireComponent(typeof(PlatformerController))]
        public class Character : MonoBehaviour {
            private PlatformerController m_platformer;
            private Collider2D m_lastContactGround;

            private void Awake() {
                m_platformer = GetComponent<PlatformerController>();
            }
            private void Update() {
                if (m_platformer.IsGround) {
                    if(m_lastContactGround != m_platformer.ContactGround) {
                        if (PlatformConstants.IsStepPlatform(m_platformer.ContactGround.tag)) {
                            PlatformEffector effector = m_platformer.ContactGround.GetComponent<PlatformEffector>();
                            effector.OnEffect(this);
                        }
                        m_lastContactGround = m_platformer.ContactGround;
                    }
                } else {
                    m_lastContactGround = null;
                }
            }
        }
    }
}
