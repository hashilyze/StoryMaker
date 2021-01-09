using UnityEngine;

namespace Stroy {

    namespace EC {
        [RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
        public sealed class EntityController : MonoBehaviour {
            public Rigidbody2D Rigidbody => m_rigidbody;
            public BoxCollider2D Body => m_body;

            public Vector2 Size => m_size;
            public Vector2 Velocity => m_velocity;
            public Rigidbody2D FollowBlock => m_followBlock;
            public System.Func<Rigidbody2D, Vector2> FollowDistanceGenerator => m_getFollowDistance;


            // Command
            public void SetVelocity(in Vector2 velocity) {
                m_velocity = velocity;
            }
            public void SetPosition(in Vector2 position, bool unsafeMode = true) {
                m_rigidbody.MovePosition(position);
                m_executedSetPos = true;
                if (unsafeMode) {
                    m_undangerous = false;
                }
            }
            public void SetSize(in Vector2 size, bool unsafeMode = true) {
                // Unchanged
                if (m_size == size) return;

                m_body.size = size;
                m_size = size;
                if (unsafeMode) {
                    m_undangerous = false;
                }
            }
            public void SetFollowBlock(Rigidbody2D followBlock) {
                m_followBlock = followBlock;
                m_existFollow = followBlock != null;
            }
            public void SetFollowDistanceGenerator(System.Func<Rigidbody2D, Vector2> followDistanceGenerator) {
                m_getFollowDistance = followDistanceGenerator;
            }
            //==========================================================================================

            // Buffer
            private const int HIT_BUFFER_SIZE = 8;
            private const int COLLIDER_BUFFER_SIZE = 8;
            private readonly RaycastHit2D[] m_bufferHit = new RaycastHit2D[HIT_BUFFER_SIZE];
            private readonly Collider2D[] m_bufferCollider = new Collider2D[COLLIDER_BUFFER_SIZE];
            // Component
            [HideInInspector] private Rigidbody2D m_rigidbody;
            [HideInInspector] private BoxCollider2D m_body;
            // State
            private Vector2 m_velocity;
            private Vector2 m_size;
            private int m_dynamicNum;                                       // The number of dynamic blocks which entity touch or overlap
            [HideInInspector] private bool m_undangerous;                   // State flag; if flag down, EC always strictly monitor interaction
            [HideInInspector] private bool m_executedSetPos;                // State flag; if flag up, current step execute SetPosition, otherwise apply velocity
            
            private Rigidbody2D m_followBlock;
            private System.Func<Rigidbody2D, Vector2> m_getFollowDistance;
            [HideInInspector] private bool m_existFollow;                   // Flag of exist follow block to optimize check
            private bool m_pushedByFollowX;                                 // Whether pushed by follow block to axis-X
            private bool m_pushedByFollowY;                                 // Whether pushed by follow block to axis-Y



            private void OnEnable() {
                m_undangerous = false;
            }
            private void OnDisable() {
                m_velocity = Vector2.zero;
                m_undangerous = true;
                m_executedSetPos = false;
            }
            
            private void Awake() {
                // Setup body
                m_body = GetComponent<BoxCollider2D>();
                SetSize(m_body.size);
                // Setup rigidbody
                m_rigidbody = GetComponent<Rigidbody2D>();
                m_rigidbody.isKinematic = true;
                m_rigidbody.useFullKinematicContacts = true;
                m_rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            }

            private void FixedUpdate() {
                // Execute SetPosition, instead of velocity
                if (m_executedSetPos) {
                    m_executedSetPos = false;
                    return;
                }

                // Initialize properties
                Vector2 origin = m_rigidbody.position;
                Vector2 destination = origin;
                // Compute destination
                if (ReactBlock(in origin, out Vector2 pushDistance, out Vector2 penetration)) { // Pushed
                    destination += penetration;
                    ApplyVelocity(ref destination);

                    if (m_existFollow) {
                        Vector2 followDistance = m_getFollowDistance(m_followBlock);

                        destination.x += m_pushedByFollowX || pushDistance.x * followDistance.x < 0f ? pushDistance.x : pushDistance.x + followDistance.x;
                        destination.y += m_pushedByFollowY || pushDistance.y * followDistance.y < 0f ? pushDistance.y : pushDistance.y + followDistance.y;
                    } else {
                        destination += pushDistance;
                    }
                } else { // No pushed
                    ApplyVelocity(ref destination);
                    if (m_existFollow) {
                        destination += m_getFollowDistance(m_followBlock);
                    }
                }
                // Apply destination
                m_rigidbody.MovePosition(destination);
            }

            private void OnCollisionEnter2D(Collision2D collision) {
                if (collision.gameObject.layer == ECConstants.DynamicBlock) {
                    ++m_dynamicNum;
                }
            }
            private void OnCollisionExit2D(Collision2D collision) {
                if (collision.gameObject.layer == ECConstants.DynamicBlock) {
                    --m_dynamicNum;
                }
            }

            /// <summary>Compute reaction value to blocks</summary>
            private bool ReactBlock(in Vector2 origin, out Vector2 pushDistance, out Vector2 penetration) {
                pushDistance = penetration = Vector2.zero;
                m_pushedByFollowX = m_pushedByFollowY = false;

                // Fast check
                if (m_dynamicNum == 0 && m_undangerous) return false;
                m_undangerous = true;

                // Query blocks required reaction
                int numBlock = Physics2D.OverlapBoxNonAlloc(origin, m_size, 0f, m_bufferCollider, ECConstants.BlockMask);
                
                // No blocks, No react
                if (numBlock == 0) return false;

                // Compute reaction values
                Vector2 maxPenetration = Vector2.zero;  // Query result.1 : Recovery value to overlapped blocks
                Vector2 maxPushVelocity = Vector2.zero; // Query result.2 : Estimated penetration by dynamic blocks at next step

                for (int n = 0; n != numBlock; ++n) {
                    Collider2D detectedCollider = m_bufferCollider[n];
                    ColliderDistance2D colliderDist = m_body.Distance(detectedCollider);
                    
                    Vector2 newPenetration = colliderDist.distance * colliderDist.normal;
                    
                    // [Dynamic]
                    // Query push-distance which is penetration at next step only with dynamic-block
                    if (detectedCollider.gameObject.layer == ECConstants.DynamicBlock) {
                        Vector2 newPushVelocity = detectedCollider.attachedRigidbody.velocity;
                        if (Vector2.Dot(newPushVelocity, newPenetration) > 0f) { // Query only forward objects
                            // Update push-velocity
                            if (colliderDist.normal.x != 0f && Mathf.Abs(maxPushVelocity.x) < Mathf.Abs(newPushVelocity.x)) {
                                m_pushedByFollowX = m_existFollow && detectedCollider.attachedRigidbody == m_followBlock;
                                maxPushVelocity.x = newPushVelocity.x;
                            }
                            if (colliderDist.normal.y != 0f && Mathf.Abs(maxPushVelocity.y) < Mathf.Abs(newPushVelocity.y)) {
                                m_pushedByFollowY = m_existFollow && detectedCollider.attachedRigidbody == m_followBlock;
                                maxPushVelocity.y = newPushVelocity.y;
                            }
                        }
                    }

                    // If gap between EC and query-target is greater than min-contact-offset, don't react
                    if (colliderDist.distance > -ECConstants.MinContactOffset) continue;
                    // If penetrations are contrasted then freeze EC
                    if (maxPenetration.x * newPenetration.x < 0 || maxPenetration.y * newPenetration.y < 0f) {
                        return false;
                    }

                    // Update penetration
                    if (Mathf.Abs(maxPenetration.x) < Mathf.Abs(newPenetration.x)) maxPenetration.x = newPenetration.x;
                    if (Mathf.Abs(maxPenetration.y) < Mathf.Abs(newPenetration.y)) maxPenetration.y = newPenetration.y;
                }
                // Make gap between EC and collision-target
                if (maxPenetration.x != 0f) maxPenetration.x -= (maxPenetration.x < 0f ? -maxPenetration.x : maxPenetration.x) * (ECConstants.MinContactOffset - 0.005f);
                if (maxPenetration.y != 0f) maxPenetration.y -= (maxPenetration.y < 0f ? -maxPenetration.y : maxPenetration.y) * (ECConstants.MinContactOffset - 0.005f);

                penetration = maxPenetration;
                pushDistance = maxPushVelocity * Time.fixedDeltaTime;
                return true;
            }
            /// <summary>Apply velocity to lastest destination and update velocity</summary>
            private void ApplyVelocity(ref Vector2 destination) {
                if (m_velocity == Vector2.zero) return;

                Vector2 befPos = destination;
                Vector2 distance = m_velocity * Time.fixedDeltaTime;
                if (distance.y > 0f) {
                    if (distance.y != 0f) AddDistance(ref destination, distance.y, Vector2.up);
                    if (distance.x != 0f) AddDistance(ref destination, distance.x, Vector2.right);
                } else {
                    if (distance.x != 0f) AddDistance(ref destination, distance.x, Vector2.right);
                    if (distance.y != 0f) AddDistance(ref destination, distance.y, Vector2.up);
                }
                // Update velocity
                m_velocity = (destination - befPos) / Time.fixedDeltaTime;
            }
            /// <summary>Add distance to lastest destination</summary>
            private void AddDistance(ref Vector2 destination, float distance, in Vector2 axis) {
                float clampedDist = (distance < 0 ? -distance : distance) + ECConstants.MinContactOffset;
                float sign = distance < 0 ? -1f : 1f;

                // Query blocks which are obstacle for moving
                int numBlock = Physics2D.BoxCastNonAlloc(destination, m_size, 0f, axis * sign, m_bufferHit, clampedDist, ECConstants.BlockMask);
                // Clamp distance
                for (int n = 0; n != numBlock; ++n) {
                    RaycastHit2D blockHit = m_bufferHit[n];

                    // [Dynamic]
                    // Adjust blockHit.distance
                    if (blockHit.collider.gameObject.layer == ECConstants.DynamicBlock) {
                        float dynamicVelocity = axis.x != 0f ? blockHit.rigidbody.velocity.x : blockHit.rigidbody.velocity.y;
                        // Extend hit.distance because of synchronizing with position of dynamic block at next step
                        if (sign * dynamicVelocity > 0f) {
                            blockHit.distance += (dynamicVelocity < 0 ? -dynamicVelocity : dynamicVelocity) * Time.fixedDeltaTime;
                        }
                    }

                    // Update add-distance
                    if (blockHit.distance < clampedDist) clampedDist = blockHit.distance;
                }
                // Add clamped distance
                (axis.x != 0f ? ref destination.x : ref destination.y) += (clampedDist - ECConstants.MinContactOffset) * sign;
            }


            #region DEBUG
#if DEBUG_MODE
            private void OnDrawGizmos() {
                if (Application.isEditor) {
                    if (m_body == null) m_body = GetComponent<BoxCollider2D>();
                    if (m_rigidbody == null) m_rigidbody = GetComponent<Rigidbody2D>();
                }

                // Visualize body
                DrawRoundBox(0f, Color.green);
                // Visualize max contact offset
                DrawRoundBox(ECConstants.MaxContactOffset, Color.blue);
                // Visualized min contact offset
                DrawRoundBox(ECConstants.MinContactOffset, Color.red);
                // Visualized good constact offset
                DrawRoundBox((ECConstants.MaxContactOffset + ECConstants.MinContactOffset) * 0.5f, Color.magenta);
            }
            private void DrawRoundBox(float edgeOffset, Color color) {
                Vector2 center = Rigidbody.position;
                Vector2 size = m_body.size;

                Vector2 tr = size * new Vector2(0.5f, 0.5f);
                Vector2 br = size * new Vector2(0.5f, -0.5f);
                Vector2 tl = size * new Vector2(-0.5f, 0.5f);
                Vector2 bl = size * new Vector2(-0.5f, -0.5f);

                Gizmos.color = color;
                Gizmos.DrawWireCube(center, (Vector2)m_body.bounds.size + 2 * edgeOffset * Vector2.one);
                if (edgeOffset > 0f) {
                    Gizmos.DrawWireSphere(center + tr, edgeOffset);
                    Gizmos.DrawWireSphere(center + br, edgeOffset);
                    Gizmos.DrawWireSphere(center + tl, edgeOffset);
                    Gizmos.DrawWireSphere(center + bl, edgeOffset);
                }
            }
#endif
            #endregion
        }
    }
}
