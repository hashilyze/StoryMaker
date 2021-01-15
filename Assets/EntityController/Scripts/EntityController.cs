using UnityEngine;

namespace Stroy {
    namespace EC {
        [RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
        public sealed class EntityController : MonoBehaviour {
            #region public
            // Component
            public Rigidbody2D Rigidbody => m_rigidbody;
            public BoxCollider2D Body => m_body;
            // Entity State
            public Vector2 Size { get => m_size; set => SetSize(value); }
            public Vector2 Velocity { get => m_velocity; set => SetVelocity(value); }
            public Vector2 Position => m_rigidbody.position;
            // Contact Dynamics
            public bool ContactDynamics => m_dynamicNum > 0;
            public Rigidbody2D FollowBlock { get => m_follower; set => SetFollower(value); }
            public System.Func<Rigidbody2D, Vector2> FollowDistanceGenerator { get => m_getFollowDistance; set => SetFollowDistanceGenerator(value); }
            // Event
            /// <summary>Entity can't react because of stucking</summary>
            public System.Action<EntityController> OnFreeze;
            /// <summary>Changed size of body</summary>
            public System.Action<EntityController> OnResize;


            // Command
            public void SetVelocity(Vector2 velocity) {
                m_velocity = velocity;
            }
            public void SetPosition(Vector2 position, bool safe = false) {
                m_rigidbody.MovePosition(position);
                m_executedSetPos = true;
                if (!safe) m_executedUnsafe = true;
            }
            public void SetSize(Vector2 size, bool safe = false) {
                if (m_size == size) return; // Unchanged

                m_body.size = size;
                m_size = size;
                if (!safe) m_executedUnsafe = true;
                OnResize?.Invoke(this);
            }

            [ContextMenu("Recovery")]
            public void Recovery() { m_executedUnsafe = true; }

            public void SetFollower(Rigidbody2D followBlock) {
                m_follower = followBlock;
                m_existFollow = followBlock != null;
            }
            public void SetFollowDistanceGenerator(System.Func<Rigidbody2D, Vector2> followDistanceGenerator) {
                m_getFollowDistance = followDistanceGenerator;
            }
            #endregion

            #region Private
            // Buffer
            private const int HIT_BUFFER_SIZE = 8;
            private const int COLLIDER_BUFFER_SIZE = 8;
            private readonly RaycastHit2D[] m_bufferHit = new RaycastHit2D[HIT_BUFFER_SIZE];
            private readonly Collider2D[] m_bufferCollider = new Collider2D[COLLIDER_BUFFER_SIZE];
            // Component
            [HideInInspector] private Rigidbody2D m_rigidbody;
            [HideInInspector] private BoxCollider2D m_body;
            // State
            [SerializeField] private Vector2 m_velocity;
            [SerializeField] private Vector2 m_size;
            [HideInInspector] private bool m_executedUnsafe;                // State flag; if flag up, should strictly check
            [HideInInspector] private bool m_executedSetPos;                // State flag; if flag up, current step execute SetPosition, otherwise apply velocity
            // Contact dynamics
            private int m_dynamicNum;                                       // The number of dynamic blocks which entity touch or overlap
            [SerializeField] private Rigidbody2D m_follower;                // Current connected follower; it may not be block;
            private System.Func<Rigidbody2D, Vector2> m_getFollowDistance;
            [HideInInspector] private bool m_existFollow;                   // Flag of exist follow block to optimize check


            private void OnEnable() {
                // When active, always check state
                m_executedUnsafe = true;
            }
            private void OnDisable() {
                // State and flag reset
                m_velocity = Vector2.zero;
                m_executedUnsafe = false;
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
                // Move by position
                if (m_executedSetPos) {
                    m_executedSetPos = false;
                    return;
                }
                // Move by velocity
                {
                    // Initialize properties
                    Vector2 destination = m_rigidbody.position;
                    // Compute destination
                    if (ReactBlock(destination, out Vector2 preDist, out Vector2 postDist)) {
                        destination += preDist;
                        ApplyVelocity(ref destination);
                        destination += postDist;
                    } else {
                        ApplyVelocity(ref destination);
                    }
                    // Apply destination
                    m_rigidbody.MovePosition(destination);
                }
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

            private bool ReactBlock(Vector2 origin, out Vector2 preDistance, out Vector2 postDistance) {
                preDistance = postDistance = Vector2.zero;

                // Fast check
                if (m_dynamicNum == 0 && !m_executedUnsafe && !m_existFollow) return false;
                m_executedUnsafe = false; // Off button

                Vector2 penetration, pushDistance, followDistance;
                bool pushedByFollowX = false, pushedByFollowY = false;
                {
                    // Query pushing blocks
                    int numBlock = Physics2D.OverlapBoxNonAlloc(origin, m_size, 0f, m_bufferCollider, ECConstants.BlockMask);
                    // Calcuate Push
                    if (numBlock > 0) {
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
                                        pushedByFollowX = m_existFollow && detectedCollider.attachedRigidbody == m_follower;
                                        maxPushVelocity.x = newPushVelocity.x;
                                    }
                                    if (colliderDist.normal.y != 0f && Mathf.Abs(maxPushVelocity.y) < Mathf.Abs(newPushVelocity.y)) {
                                        pushedByFollowY = m_existFollow && detectedCollider.attachedRigidbody == m_follower;
                                        maxPushVelocity.y = newPushVelocity.y;
                                    }
                                }
                            }

                            // If gap between EC and query-target is greater than min-contact-offset, don't react
                            if (colliderDist.distance > -ECConstants.MinContactOffset) continue;
                            // If penetrations are contrasted then freeze EC
                            if (maxPenetration.x * newPenetration.x < 0 || maxPenetration.y * newPenetration.y < 0f) {
                                OnFreeze?.Invoke(this);
                                return false;
                            }
                            // Update penetration
                            if (Mathf.Abs(maxPenetration.x) < Mathf.Abs(newPenetration.x)) maxPenetration.x = newPenetration.x;
                            if (Mathf.Abs(maxPenetration.y) < Mathf.Abs(newPenetration.y)) maxPenetration.y = newPenetration.y;
                        }
                        // Make gap between EC and collision-target
                        if (maxPenetration.x != 0f) maxPenetration.x -= (maxPenetration.x < 0f ? -1f : 1f) * (ECConstants.MinContactOffset - 0.005f);
                        if (maxPenetration.y != 0f) maxPenetration.y -= (maxPenetration.y < 0f ? -1f : 1f) * (ECConstants.MinContactOffset - 0.005f);

                        penetration = maxPenetration;
                        pushDistance = maxPushVelocity * Time.fixedDeltaTime;
                    } else {
                        penetration = Vector2.zero;
                        pushDistance = Vector2.zero;
                    }
                }
                // Calcuate Follow
                {
                    if (m_existFollow) {
                        followDistance = m_getFollowDistance(m_follower);
                    } else {
                        followDistance = Vector2.zero;
                    }
                }
                // Set react distance
                {
                    // Set pre-distance
                    preDistance = penetration;
                    // Set post-distance
                    if (pushDistance.x * m_velocity.x > 0f) {
                        pushDistance.x = Mathf.Max(0f, Mathf.Abs(pushDistance.x) - Mathf.Abs(m_velocity.x * Time.fixedDeltaTime)) * Mathf.Sign(pushDistance.x);
                    }
                    if (pushedByFollowX || pushDistance.x * followDistance.x < 0f) {
                        postDistance.x = pushDistance.x;
                    } else {
                        postDistance.x = Mathf.Abs(pushDistance.x) < Mathf.Abs(followDistance.x) ? followDistance.x : pushDistance.x;
                    }

                    if (pushDistance.y * m_velocity.y > 0f) {
                        pushDistance.y = Mathf.Max(0f, Mathf.Abs(pushDistance.y) - Mathf.Abs(m_velocity.y * Time.fixedDeltaTime)) * Mathf.Sign(pushDistance.y);
                    }
                    if (pushedByFollowY || pushDistance.y * followDistance.y < 0f) {
                        postDistance.y = pushDistance.y;
                    } else {
                        postDistance.y = Mathf.Abs(pushDistance.y) < Mathf.Abs(followDistance.y) ? followDistance.y : pushDistance.y;
                    }
                }
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
            private void AddDistance(ref Vector2 destination, float distance, Vector2 axis) {
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
                        // Extend hit.distance because synchronize with position of dynamic block at next step
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
            #endregion

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
