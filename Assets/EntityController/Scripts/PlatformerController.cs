using UnityEngine;

namespace Stroy {
    
    namespace EC {
        [RequireComponent(typeof(EntityController))]
        public class PlatformerController : MonoBehaviour {
            #region Public
            public EntityController Controller => m_controller;

            public bool IsGround => m_isGround;
            public Collider2D ContactGround => m_contactGround;
            public float ContactRadian => m_contactRadian;


            public System.Action OnGround;


            // Action Command
            public void Move(float velocity) {
                m_moveValue = velocity;
            }
            public void Jump(Vector2 velocity) {
                m_velocity = velocity;
                m_isJump = true;
                m_gravityScale = DEFAULT_GRAVITY;
            }
            public void PauseJump(float scale) {
                if (m_isJump) {
                    m_gravityScale = scale;
                }
            }
            #endregion
            
            #region Private
            private const float DEFAULT_GRAVITY = 1f;
            private const float FALL_GRAVITY = 1.5f;
            // Cache
            private static readonly Vector2 SIDE_RIGHT = new Vector2(1f, -1f).normalized;
            private static readonly Vector2 SIDE_LEFT = new Vector2(-1f, -1f).normalized;
            private static readonly Vector2 BOTTOM_RIGHT = new Vector2(0.5f, -0.5f);
            private static readonly Vector2 BOTTOM_LEFT = new Vector2(-0.5f, -0.5f);

            // Component
            [HideInInspector] private EntityController m_controller;
            // State
            private Vector2 m_velocity;
            private bool m_isGround;
            private float m_contactRadian;
            private Collider2D m_contactGround;
            private bool m_isJump;
            private float m_gravityScale = DEFAULT_GRAVITY;
            [HideInInspector] private Vector2 m_prevPos;
            // Input
            private float m_moveValue;


            private void OnDisable() {
                m_velocity = Vector2.zero;
                m_isGround = false;
                m_contactRadian = 0f;
                m_isJump = false;
                m_gravityScale = DEFAULT_GRAVITY;
                m_controller.SetFollowBlock(null);
            }

            private void Awake() {
                m_controller = GetComponent<EntityController>();
                // Set Default Follow Distance Generator
                if(m_controller.FollowDistanceGenerator == null) {
                    m_controller.SetFollowDistanceGenerator(DefaultFollowDistanceGenerator);
                }
            }
            private void Update() {
                HandleMovement();
            }

            private void HandleMovement() {
                Vector2 origin = m_controller.Position;

                bool wasGround = m_isGround;
                CheckGround(in origin);

                // Revise conner bouncing
                if (!m_isGround && wasGround && m_prevPos != origin && !m_isJump) {
                    RaycastHit2D hitBlock = Physics2D.BoxCast(origin, m_controller.Size, 0f, Vector2.down, Mathf.Infinity, 0x01 << ECConstants.StaticBlock);
                    if (TryGetGroundAngle(in hitBlock, out float angle)) {
                        if (m_velocity.y > 0f && hitBlock.distance <= origin.y - m_prevPos.y + ECConstants.MaxContactOffset
                            || m_velocity.y <= 0f && hitBlock.distance <= Mathf.Tan(angle * Mathf.Deg2Rad) * (m_prevPos.x - origin.x) + ECConstants.MaxContactOffset
                            ) {
                            // On dynamic block, don't support RCB
                            m_controller.SetPosition(origin + Vector2.down * (hitBlock.distance - ECConstants.MinContactOffset));
                            m_contactRadian = angle * Mathf.Deg2Rad;
                            m_contactGround = hitBlock.collider;
                            m_isGround = true;
                            m_prevPos = origin;
                            return;
                        }
                    }
                }

                if (m_isGround && !wasGround) { // Landing                    
                    // Gravity
                    m_gravityScale = DEFAULT_GRAVITY;
                    // Move
                    m_velocity = Vector2.zero;
                    // Event
                    OnGround?.Invoke();
                } else if (!m_isGround || m_isJump) { // Airbone
                    if (m_isJump) { // Jumping
                        // If touch cell, stop to jump and fast fall
                        if (Physics2D.BoxCast(origin, m_controller.Size, 0f, Vector2.up, ECConstants.MaxContactOffset, ECConstants.BlockMask)) {
                            m_velocity.y = 0f;
                        }
                        if(m_velocity.y <= 0f) {
                            m_isJump = false;
                        }
                    }
                    if(m_velocity.y < 0f) { // Falling
                        m_gravityScale = FALL_GRAVITY;
                    }
                    // Apply gravity
                    // Clamp falling speed
                    if(m_velocity.y > -ECConstants.FallLimit) {
                        m_velocity.y -= ECConstants.Gravity * Time.deltaTime * m_gravityScale;
                    } else {
                        m_velocity.y = -ECConstants.FallLimit;
                    }
                    // Move
                    m_velocity.x = m_moveValue;
                } else { // Walking or Standing
                    // Move
                    if (m_contactRadian != 0f) {
                        m_velocity = m_moveValue * new Vector2(Mathf.Cos(m_contactRadian), Mathf.Sin(m_contactRadian));
                    } else {
                        m_velocity = m_moveValue * Vector2.right;
                    }
                }

                m_controller.SetVelocity(in m_velocity);
                m_prevPos = origin;
            }

            private void CheckGround(in Vector2 origin) {
                if (m_isGround                                                              /* Was grounded at previous step */
                    || m_velocity.y <= (m_velocity.x < 0f ? -m_velocity.x : m_velocity.x)   /* Character can intersect with slope */
                    ) {
                    Vector2 size = m_controller.Size;

                    RaycastHit2D hitBlock;
                    // Priority check in move direction
                    if(m_moveValue < 0f) { // Move left
                        hitBlock = Physics2D.Raycast(origin + size * BOTTOM_LEFT, SIDE_LEFT, ECConstants.MaxContactOffset, ECConstants.BlockMask);
                        if (TryUpdateGroundInfo(in hitBlock)) return;
                    } else if(m_moveValue > 0f){ // Move right
                        hitBlock = Physics2D.Raycast(origin + size * BOTTOM_RIGHT, SIDE_RIGHT, ECConstants.MaxContactOffset, ECConstants.BlockMask);
                        if (TryUpdateGroundInfo(in hitBlock)) return;
                    }
                    hitBlock = Physics2D.BoxCast(origin, size, 0f, Vector2.down, ECConstants.MaxContactOffset, ECConstants.BlockMask);
                    if (TryUpdateGroundInfo(in hitBlock)) return;
                }

                // No ground
                m_isGround = false;
                m_contactRadian = 0f;
                m_contactGround = null;
                m_controller.SetFollowBlock(null);
            }
            private bool TryUpdateGroundInfo(in RaycastHit2D hitBlock) {
                if(TryGetGroundAngle(in hitBlock, out float angle)) {
                    m_isGround = true;
                    m_contactRadian = angle * Mathf.Deg2Rad;
                    m_contactGround = hitBlock.collider;

                    if (hitBlock.collider.gameObject.layer == ECConstants.DynamicBlock) {
                        m_controller.SetFollowBlock(hitBlock.rigidbody);
                    }
                    return true;
                }
                return false;
            }
            private bool TryGetGroundAngle(in RaycastHit2D hitBlock, out float angle) {
                if (hitBlock) {
                    angle = Vector2.SignedAngle(Vector2.up, hitBlock.normal);
                    return -ECConstants.SlopeLimit < angle && angle < ECConstants.SlopeLimit;
                }
                angle = 0f;
                return false;
            }

            ///<summary>Extract follow distance from current observing block</summary>
            private static Vector2 DefaultFollowDistanceGenerator(Rigidbody2D followBlock) {
                Vector2 followDistance = Vector2.zero;
                followDistance.x = followBlock.velocity.x * Time.fixedDeltaTime;
                if (followBlock.velocity.y < 0f) {
                    followDistance.y = followBlock.velocity.y * Time.fixedDeltaTime;
                }
                return followDistance;
            }
            #endregion
        }
    }
}
