using UnityEngine;


namespace Stroy {
    namespace Platform {
        public class PatrolAgent : DynamicPlatformAgent {
            public override void HandlePlatform(float deltaTime) {
                if (isJoint == false) return;

                if (m_isCoolDown) { 
                    m_elapsedCoolDown += deltaTime;
                    if(m_elapsedCoolDown > m_coolDownLimit) {
                        m_isCoolDown = false;
                    } else {
                        return;
                    }
                }

                Vector2 current = Platform.Rigidbody.position;
                if (current == m_destPos) {
                    UpdateNode();
                    m_isCoolDown = true;
                    m_elapsedCoolDown = 0f;
                    Platform.Rigidbody.velocity = Vector2.zero;
                    return;
                }

                Vector2 destination = Vector2.MoveTowards(current, m_destPos, Speed * deltaTime);                
                Platform.Rigidbody.velocity = (destination - current) / deltaTime;
            }

            public void Joint(Track track) {
                m_track = track;
                isJoint = true;

                GCD_Result result = m_track.GetClosest(transform.position);
                transform.position = result.Point + m_track.LocalZero;

                int size = m_track.Size;
                // Get current node
                if (result.OnPoint) {
                    m_destIndex = result.ClosestNode;
                } else {
                    if (m_track.IsCycle
                        && (result.ClosestNode == 0 && result.OtherClosestNode == size - 1 || result.ClosestNode == size - 1 && result.OtherClosestNode == 0)
                        ) {
                        if (m_asscending) {
                            m_destIndex = size - 1;
                        } else {
                            m_destIndex = 0;
                        }
                    } else {
                        if (m_asscending) {
                            m_destIndex = result.ClosestNode > result.OtherClosestNode ? result.OtherClosestNode : result.ClosestNode;
                        } else {
                            m_destIndex = result.ClosestNode < result.OtherClosestNode ? result.OtherClosestNode : result.ClosestNode;
                        }
                    }
                }
                UpdateNode();
            }


            [SerializeField] private Track m_track;                 // Jointed track
            [SerializeField] private float Speed;                   // Patrol speed
            [SerializeField] private bool m_asscending = true;      // Patrol direction; if true, follow order by nodes index
            [SerializeField] private float m_coolDownLimit;         // Break time when reach node
            private bool isJoint;                                   // Whether has track
            private int m_destIndex;                                // Destination node's index
            private Vector2 m_destPos;                              // World position of destination node
            private bool m_isCoolDown;                              // State flag of cool down
            private float m_elapsedCoolDown;                        // Elapsed time for cool down


            private void Start() {
                if (m_track != null) {
                    Joint(m_track);
                }
            }

            private void UpdateNode() {
                if (m_track.IsCycle) {
                    m_destIndex = (m_destIndex + m_track.Size + (m_asscending ? 1 : -1)) % m_track.Size;
                } else {
                    if (m_asscending && m_destIndex == m_track.Size - 1) {
                        m_asscending = false;
                    } else if (!m_asscending && m_destIndex == 0) {
                        m_asscending = true;
                    }

                    if (m_asscending) {
                        ++m_destIndex;
                    } else {
                        --m_destIndex;
                    }
                }
                m_destPos = m_track.GetNode(m_destIndex) + m_track.LocalZero;
            }
        }
    }
}
