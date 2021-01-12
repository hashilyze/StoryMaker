using UnityEngine;

namespace Stroy {
    namespace EC {
        [System.Flags] public enum ECollision { None = 0x00, Below = 0x01, Above = 0x02, Left = 0x04, Right = 0x08 }


        [RequireComponent(typeof(EntityController))]
        public class PlatformerController : MonoBehaviour {
            #region Public
            public EntityController Controller => m_controller;

            public Vector2 Velocity => m_velocity;
            public ECollision Collision => m_collision;
            public bool ActiveWallCheck { get => m_activeWallCheck; set => m_activeWallCheck = value; }

            public bool IsGround => m_isGround;
            public Collider2D ContactGround => m_contactGround;
            public float ContactRadian => m_contactRadian;

            public bool IsWall => m_isWall;
            public Collider2D ContactWall => m_contactWall;

            public System.Action OnGround;


            // Action Command
            /// <summary>Trasform x-pos with given velocity</summary>
            public void Move(float velocityX) {
                m_moveValue = velocityX;
            }
            /// <summary>Transform y-pos with given velocity</summary>
            public void Jump(float velocityY) {
                m_velocity.y = velocityY;
                m_isJumpUp = velocityY > 0f;
                m_gravityScale = DEFAULT_GRAVITY;
                m_elapsedJump = 0f;
            }
            /// <summary>Break jump by gravity</summary>
            public void BreakJump(float scale) {
                if (m_isJumpUp) {
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
            private ECollision m_collision;
            [SerializeField] private bool m_activeWallCheck;
            private bool m_isGround;
            private float m_contactRadian;
            private Collider2D m_contactGround;
            private bool m_isJumpUp;
            private float m_gravityScale = DEFAULT_GRAVITY;
            private bool m_isWall;
            private Collider2D m_contactWall;
            [HideInInspector] private Vector2 m_prevPos;
            private float m_elapsedJump;
            // Input
            private float m_moveValue;


            private void OnDisable() {
                m_velocity = Vector2.zero;
                m_isGround = false;
                m_contactRadian = 0f;
                m_isJumpUp = false;
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
                Vector2 size = m_controller.Size;

                bool wasGround = m_isGround;
                CheckCollision(origin, size);

                // Revise conner bouncing
                if (!m_isGround && wasGround && m_prevPos != origin && !m_isJumpUp) {
                    RaycastHit2D hitBlock = Physics2D.BoxCast(origin, size, 0f, Vector2.down, Mathf.Infinity, 0x01 << ECConstants.StaticBlock);
                    if (hitBlock) {
                        float angle = Vector2.SignedAngle(Vector2.up, hitBlock.normal);
                        if(-ECConstants.SlopeLimit < angle && angle < ECConstants.SlopeLimit) {
                            if (m_velocity.y > 0f && hitBlock.distance <= origin.y - m_prevPos.y + ECConstants.MaxContactOffset
                            || m_velocity.y <= 0f && hitBlock.distance <= Mathf.Tan(angle * Mathf.Deg2Rad) * (m_prevPos.x - origin.x) + ECConstants.MaxContactOffset
                            ) {
                                // On dynamic block, don't support RCB
                                m_controller.SetPosition(origin + Vector2.down * (hitBlock.distance - ECConstants.MinContactOffset));
                                m_contactRadian = angle * Mathf.Deg2Rad;
                                m_contactGround = hitBlock.collider;
                                m_isGround = true;
                                m_collision |= ECollision.Below;
                                m_prevPos = origin;
                                return;
                            }
                        }
                    }
                }

                if (!m_isGround) { // On Airbone
                    // Apply gravity
                    if (m_velocity.y > -ECConstants.FallLimit) { // Clamp falling speed
                        m_velocity.y -= ECConstants.Gravity * Time.deltaTime * m_gravityScale;
                    } else {
                        m_velocity.y = -ECConstants.FallLimit;
                    }
                    // Fall
                    if (m_velocity.y < 0f) {
                        m_gravityScale = FALL_GRAVITY;
                        m_isJumpUp = false;
                    }
                    // Move
                    m_velocity.x = m_moveValue;
                } else { // On Ground
                    if (!wasGround) { // Landing
                        m_gravityScale = DEFAULT_GRAVITY;
                        m_velocity = Vector2.zero;
                        OnGround?.Invoke();
                    } else { // Walk or Stand
                        if(m_moveValue != 0f) {
                            if (m_contactRadian != 0f) { // Walk on Slope
                                m_velocity = m_moveValue * new Vector2(Mathf.Cos(m_contactRadian), Mathf.Sin(m_contactRadian));
                            } else { // Walk on plane
                                m_velocity = m_moveValue * Vector2.right;
                            }
                        } else { // Stand
                            m_velocity = Vector2.zero;
                        }
                    }
                }

                m_controller.SetVelocity(m_velocity);
                m_prevPos = origin;
            }

            private void CheckCollision(Vector2 origin, Vector2 size) {
                m_collision = ECollision.None;
                
                CheckGround(origin, size);
                CheckCell(origin, size);
                if (m_activeWallCheck) {
                    CheckWall(origin, size);
                }
            }

            private void CheckGround(Vector2 origin, Vector2 size) {
                if (m_isJumpUp) {
                    m_elapsedJump += Time.deltaTime;
                }
                if ((!m_isJumpUp || m_elapsedJump > 0.1f) && (m_isGround || m_velocity.y <= (m_velocity.x < 0f ? -m_velocity.x : m_velocity.x))) {
                    RaycastHit2D hitBlock;
                    // Priority check in move direction
                    if(m_moveValue < 0f) { // Move left
                        hitBlock = Physics2D.Raycast(origin + size * BOTTOM_LEFT, SIDE_LEFT, ECConstants.MaxContactOffset, ECConstants.BlockMask);
                        if (TryUpdateGroundInfo(hitBlock)) return;
                    } else if(m_moveValue > 0f){ // Move right
                        hitBlock = Physics2D.Raycast(origin + size * BOTTOM_RIGHT, SIDE_RIGHT, ECConstants.MaxContactOffset, ECConstants.BlockMask);
                        if (TryUpdateGroundInfo(hitBlock)) return;
                    }
                    hitBlock = Physics2D.BoxCast(origin, size, 0f, Vector2.down, ECConstants.MaxContactOffset, ECConstants.BlockMask);
                    if (TryUpdateGroundInfo(hitBlock)) return;
                }

                // No ground
                m_isGround = false;
                m_contactRadian = 0f;
                m_contactGround = null;
                m_controller.SetFollowBlock(null);
            }
            private bool TryUpdateGroundInfo(RaycastHit2D hitBlock) {
                if (hitBlock) {
                    float angle = Vector2.SignedAngle(Vector2.up, hitBlock.normal);
                    if((angle < 0f ? -angle : angle) < ECConstants.SlopeLimit) {
                        m_isGround = true;
                        m_contactRadian = angle * Mathf.Deg2Rad;
                        m_contactGround = hitBlock.collider;
                        m_collision |= ECollision.Below;
                        m_isJumpUp = false;

                        if (hitBlock.collider.gameObject.layer == ECConstants.DynamicBlock) {
                            m_controller.SetFollowBlock(hitBlock.rigidbody);
                        }
                        return true;
                    }
                }
                return false;
            }

            private void CheckCell(Vector2 origin, Vector2 size) {
                if (m_isJumpUp) {
                    // If touch cell, stop to jump and fast fall
                    if (Physics2D.BoxCast(origin, size, 0f, Vector2.up, ECConstants.MaxContactOffset, ECConstants.BlockMask)) {
                        m_velocity.y = 0f;
                        m_collision |= ECollision.Above;
                    }
                }
            }

            private void CheckWall(Vector2 origin, Vector2 size) {
                RaycastHit2D hitWall;
                if (m_moveValue < 0f) {
                    hitWall = Physics2D.BoxCast(origin, size, 0f, Vector2.right, ECConstants.MaxContactOffset, ECConstants.BlockMask);
                    if (TryUpdateWallInfo(hitWall)) return;
                    hitWall = Physics2D.BoxCast(origin, size, 0f, Vector2.left, ECConstants.MaxContactOffset, ECConstants.BlockMask);
                    if (TryUpdateWallInfo(hitWall)) return;
                } else {
                    hitWall = Physics2D.BoxCast(origin, size, 0f, Vector2.left, ECConstants.MaxContactOffset, ECConstants.BlockMask);
                    if (TryUpdateWallInfo(hitWall)) return;
                    hitWall = Physics2D.BoxCast(origin, size, 0f, Vector2.right, ECConstants.MaxContactOffset, ECConstants.BlockMask);
                    if (TryUpdateWallInfo(hitWall)) return;
                }
                m_isWall = false;
                m_contactWall = null;
            }
            private bool TryUpdateWallInfo(RaycastHit2D hitWall) {
                if (hitWall) {
                    float angle = Vector2.Angle(Vector2.up, hitWall.normal);
                    if (89f < angle && angle < 91f) {
                        m_collision |= hitWall.normal.x < 0f ? ECollision.Right : ECollision.Left;
                        m_isWall = true;
                        m_contactWall = hitWall.collider;
                        return true;
                    }
                }
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
