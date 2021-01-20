using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stroy {
    namespace Platforms {
        [RequireComponent(typeof(Rigidbody2D))]
        public class DynamicPlatform : MonoBehaviour {
            public Rigidbody2D Rigidbody => m_rigidbody;
            public DynamicPlatformAgent Agent { get => m_agent; set => SetAgent(value); }
            

            public void SetAgent(DynamicPlatformAgent agent) { m_agent = agent; }

            // -------------------------------------------------------------------------------
            // Component
            private Rigidbody2D m_rigidbody;
            [SerializeField] private DynamicPlatformAgent m_agent;

            private void Awake() {
                m_rigidbody = GetComponent<Rigidbody2D>();
            }
            private void FixedUpdate() {
                m_agent.HandlePlatform();
            }
        }
    }
}
