﻿using UnityEngine;

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
                    m_executedUnsafe = true;
                }
            }
            public void SetSize(in Vector2 size, bool unsafeMode = true) {
                m_body.size = size;
                m_size = size;

                if (unsafeMode) {
                    m_executedUnsafe = true;
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
            [HideInInspector] private bool m_executedUnsafe;                // Whether has been executed unsafe command between current and previous steps
            [HideInInspector] private bool m_executedSetPos;                // Whether has been executed SetPosition between current and previous steps
            
            private Rigidbody2D m_followBlock;
            private System.Func<Rigidbody2D, Vector2> m_getFollowDistance;
            private bool m_existFollow;
            
            private void OnEnable() {
                m_executedUnsafe = true;
            }
            private void OnDisable() {
                m_velocity = Vector2.zero;
                m_executedUnsafe = false;
                m_executedSetPos = false;
            }
            
            private void Awake() {
                // Setup body
                m_body = GetComponent<BoxCollider2D>();
                SetSize(m_body.size, true);
                // Setup rigidbody
                m_rigidbody = GetComponent<Rigidbody2D>();
                m_rigidbody.isKinematic = true;
                m_rigidbody.useFullKinematicContacts = true;
                m_rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            }

            private void FixedUpdate() {
                // Don't apply velocity to movement when teleport
                if (m_executedSetPos) {
                    m_executedSetPos = false;
                    return;
                }

                // Initialize properties
                Vector2 origin = m_rigidbody.position;
                Vector2 destination = origin;
                // Compute destination
                if (ReactBlock(in origin, out Vector2 pushDistance, out Vector2 penetration)) {
                    destination += penetration;
                    ApplyVelocity(ref destination);

                    if(m_existFollow) {
                        Vector2 followDistance = m_getFollowDistance != null ? m_getFollowDistance(m_followBlock) : m_followBlock.velocity * Time.fixedDeltaTime;
                        destination.x += pushDistance.x * followDistance.x < 0f ? pushDistance.x : pushDistance.x + followDistance.x;
                        destination.y += pushDistance.y * followDistance.y < 0f ? pushDistance.y : pushDistance.y + followDistance.y;
                    } else {
                        destination += pushDistance;
                    }
                } else {
                    ApplyVelocity(ref destination);
                    if (m_existFollow) {
                        destination += m_getFollowDistance != null ? m_getFollowDistance(m_followBlock) : m_followBlock.velocity * Time.fixedDeltaTime;
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

                // Fast check
                if (m_dynamicNum == 0 && !m_executedUnsafe) return false;
                m_executedUnsafe = false;

                // Query blocks required reaction
                int numBlock = Physics2D.OverlapBoxNonAlloc(origin, m_size, 0f, m_bufferCollider, ECConstants.BlockMask);

                // No blocks, No react
                if (numBlock == 0) return false;

                // Compute reaction values
                Vector2 maxPenetration = Vector2.zero;  // Query result.1 : Recovery value to overlapped blocks
                Vector2 maxPushVelocity = Vector2.zero; // Query result.2 : Estimated penetration by dynamic blocks at next step

                for (int n = 0; n != numBlock; ++n) {
                    Collider2D detectedCollider = m_bufferCollider[n];
                    ColliderDistance2D coliiderDist = m_body.Distance(detectedCollider);

                    Vector2 newPenetration = coliiderDist.distance * coliiderDist.normal;

                    // [Dynamic]
                    // Query push-distance which is penetration at next step only with dynamic-block
                    if (detectedCollider.gameObject.layer == ECConstants.DynamicBlock) {
                        Vector2 newPushVelocity = detectedCollider.attachedRigidbody.velocity;
                        if (Vector2.Dot(newPushVelocity, newPenetration) > 0f) { // Query only forward objects
                            // Update push-velocity
                            if (coliiderDist.normal.x != 0f && Mathf.Abs(maxPushVelocity.x) < Mathf.Abs(newPushVelocity.x)) {
                                maxPushVelocity.x = newPushVelocity.x;
                            }
                            if (coliiderDist.normal.y != 0f && Mathf.Abs(maxPushVelocity.y) < Mathf.Abs(newPushVelocity.y)) {
                                maxPushVelocity.y = newPushVelocity.y;
                            }
                        }
                    }

                    // No require to update peneration unless overlapped
                    if (coliiderDist.distance > -ECConstants.MinContactOffset) continue;
                    // If penerations are contrasted, freezing character (Can't recovery)
                    if (maxPenetration.x * newPenetration.x < 0 || maxPenetration.y * newPenetration.y < 0f) return false;

                    // Update penetration
                    if (Mathf.Abs(maxPenetration.x) < Mathf.Abs(newPenetration.x)) maxPenetration.x = newPenetration.x;
                    if (Mathf.Abs(maxPenetration.y) < Mathf.Abs(newPenetration.y)) maxPenetration.y = newPenetration.y;
                }
                // Interpolate contact-offset
                if (maxPenetration.x != 0f) maxPenetration.x -= Mathf.Sign(maxPenetration.x) * (ECConstants.MinContactOffset - 0.005f);
                if (maxPenetration.y != 0f) maxPenetration.y -= Mathf.Sign(maxPenetration.y) * (ECConstants.MinContactOffset - 0.005f);

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