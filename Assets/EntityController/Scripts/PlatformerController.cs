using UnityEngine;

namespace Stroy {
    namespace EC {
        [RequireComponent(typeof(EntityController))]
        public class PlatformerController : MonoBehaviour {
            #region Public
            public EntityController Controller => m_controller;
            // State
            public Vector2 Velocity => m_velocity;
            public bool IsFall => !m_isGround && m_velocity.y < 0f;
            // Contact ground info
            public bool IsGround => m_isGround;
            public Collider2D ContactGround => m_contactGround;
            public float ContactRadian => m_contactRadian;
            // Contact wall info
            public bool ActiveWallSensor { get => m_activeWallSensor; set => SetWallSensor(value); }
            public bool IsWall => m_isWall;
            public Collider2D ContactWall => m_contactWall;
            public bool WallOnLeft => m_wallOnLeft;
            public bool WallOnRight => m_wallOnRight;
            // Event
            public System.Action OnGround;
            public System.Action OnFall;


            // State Command
            public void SetWallSensor(bool active) {
                m_activeWallSensor = active;
                if(active == false) {
                    ResetWallInfo();
                }
            }
            // Action Command
            public void MoveX(float velocityX) {
                m_xVal = velocityX;
            }
            public void MoveY(float velocityY) {
                m_velocity.y = velocityY;

                if(velocityY >= 0f) {
                    m_gravityScale = ECConstants.DefaultGravityScale;
                    m_isNormalJump = true;
                    m_elapsedJump = 0f;
                } else {
                    m_gravityScale = ECConstants.FallGravityScale;
                    m_isNormalJump = false;
                }
                
            }
            public void BreakJump(float scale) {
                if (m_isNormalJump) {
                    m_gravityScale = scale;
                    m_isNormalJump = false;
                }
            }
            #endregion
            
            #region Private
            private const float MIN_JUMP_TIME = 0.1f;
            private static readonly Vector2 SIDE_RIGHT = new Vector2(1f, -1f).normalized;
            private static readonly Vector2 SIDE_LEFT = new Vector2(-1f, -1f).normalized;
            private static readonly Vector2 BOTTOM_RIGHT = new Vector2(0.5f, -0.5f);
            private static readonly Vector2 BOTTOM_LEFT = new Vector2(-0.5f, -0.5f);
            // Component
            [HideInInspector] private EntityController m_controller;
            // State
            [SerializeField] private Vector2 m_velocity;
            [HideInInspector] private Vector2 m_size;
            // Jump state
            private bool m_isNormalJump;
            private float m_elapsedJump;
            [SerializeField] private float m_gravityScale = ECConstants.DefaultGravityScale;
            // Contact ground info
            [SerializeField] private bool m_isGround;
            [SerializeField] private float m_contactRadian;
            [SerializeField] private Collider2D m_contactGround;
            // Contact wall info
            [SerializeField] private bool m_activeWallSensor;
            [SerializeField] private bool m_isWall;
            [SerializeField] private Collider2D m_contactWall;
            [SerializeField] private bool m_wallOnLeft;
            [SerializeField] private bool m_wallOnRight;

            private float m_xVal;
            [HideInInspector] private Vector2 m_prevPos;


            private void OnDisable() {
                m_velocity = Vector2.zero;
                ResetGroundInfo();
                ResetWallInfo();
                m_isNormalJump = false;
                m_elapsedJump = 0f;
                m_gravityScale = ECConstants.DefaultGravityScale;
            }

            private void Awake() {
                m_controller = GetComponent<EntityController>();
                // Set Default Follow Distance Generator
                if(m_controller.FollowDistanceGenerator == null) {
                    m_controller.SetFollowDistanceGenerator(DefaultFollowDistanceGenerator);
                }
                // Cache size of entity
                m_controller.OnResize += CacheSize;
                if (m_controller.Size != Vector2.zero) {
                    CacheSize(m_controller);
                }
            }
            private void Update() {
                HandleMovement();
            }

            private void HandleMovement() {
                Vector2 origin = m_controller.Position;

                if (m_isNormalJump) {
                    m_elapsedJump += Time.deltaTime;
                }

                bool wasGround = m_isGround;
                CheckCollision(origin);

                // Revise conner bouncing
                if (!m_isGround && wasGround && m_prevPos != origin && !m_isNormalJump) {
                    RaycastHit2D hitBlock = Physics2D.BoxCast(origin, m_size, 0f, Vector2.down, Mathf.Infinity, 0x01 << ECConstants.StaticBlock);
                    if (UpdateGroundInfo(hitBlock)) {
                        if (m_velocity.y > 0f && hitBlock.distance <= origin.y - m_prevPos.y + ECConstants.MaxContactOffset
                            || m_velocity.y <= 0f && hitBlock.distance <= Mathf.Tan(m_contactRadian) * (m_prevPos.x - origin.x) + ECConstants.MaxContactOffset
                            ) {
                            m_controller.SetPosition(origin + Vector2.down * (hitBlock.distance - ECConstants.MinContactOffset));
                            
                            m_prevPos = origin; // End Handle movement
                            return;
                        } else {
                            ResetGroundInfo();
                        }
                    }
                }

                // Update Velocity
                {
                    if (!m_isGround) { // On Airbone
                        float preY = m_velocity.y;
                        // Apply gravity
                        if (m_velocity.y > -ECConstants.FallLimit) { 
                            m_velocity.y -= ECConstants.Gravity * Time.deltaTime * m_gravityScale;
                        } else {
                            m_velocity.y = -ECConstants.FallLimit;
                        }
                        // Fall
                        if (m_velocity.y < 0f) {
                            if(preY >= 0f) {
                                OnFall?.Invoke();
                            }
                            m_gravityScale = ECConstants.FallGravityScale;
                            m_isNormalJump = false;
                        }
                        // Move x-pos
                        m_velocity.x = m_xVal;
                    } else { // On Ground
                        if (!wasGround) { // Landing
                            m_gravityScale = ECConstants.DefaultGravityScale;
                            m_velocity = Vector2.zero;
                            OnGround?.Invoke();
                        } else { // Walk or Stand
                            if (m_xVal != 0f) {
                                if (m_contactRadian != 0f) { // Walk on Slope
                                    m_velocity = m_xVal * new Vector2(Mathf.Cos(m_contactRadian), Mathf.Sin(m_contactRadian));
                                } else { // Walk on plane
                                    m_velocity = m_xVal * Vector2.right;
                                }
                            } else { // Stand
                                m_velocity = Vector2.zero;
                            }
                        }
                    }
                    m_controller.SetVelocity(m_velocity);
                }
                m_prevPos = origin;
            }

            private void CheckCollision(Vector2 origin) {
                CheckGround(origin);
                CheckCell(origin);
                if (m_activeWallSensor) {
                    CheckWall(origin);
                }
            }

            private void CheckGround(Vector2 origin) {
                if ((!m_isNormalJump || m_elapsedJump > MIN_JUMP_TIME)
                    && (m_isGround || m_velocity.y < (m_velocity.x < 0f ? -m_velocity.x : m_velocity.x) || m_controller.ContactDynamics)) {
                    // Check uphill
                    if(m_xVal < 0f) { // Move left
                        if (UpdateGroundInfo(Physics2D.Raycast(origin + m_size * BOTTOM_LEFT, SIDE_LEFT, ECConstants.MaxContactOffset, ECConstants.BlockMask))) return;
                    } else if(m_xVal > 0f){ // Move right
                        if (UpdateGroundInfo(Physics2D.Raycast(origin + m_size * BOTTOM_RIGHT, SIDE_RIGHT, ECConstants.MaxContactOffset, ECConstants.BlockMask))) return;
                    }
                    if (UpdateGroundInfo(Physics2D.BoxCast(origin, m_size, 0f, Vector2.down, ECConstants.MaxContactOffset, ECConstants.BlockMask))) return;
                }
                // No ground
                ResetGroundInfo();
            }
            private bool UpdateGroundInfo(RaycastHit2D hitGround) {
                if (hitGround) {
                    float angle = Vector2.SignedAngle(Vector2.up, hitGround.normal);
                    if((angle < 0f ? -angle : angle) < ECConstants.SlopeLimit) {
                        m_isGround = true;
                        m_contactRadian = angle * Mathf.Deg2Rad;
                        m_contactGround = hitGround.collider;
                        m_isNormalJump = false;

                        if (hitGround.collider.gameObject.layer == ECConstants.DynamicBlock) {
                            m_controller.SetFollower(hitGround.rigidbody);
                        } else {
                            m_controller.SetFollower(null);
                        }
                        return true;
                    }
                }
                return false;
            }
            private void ResetGroundInfo() {
                m_isGround = false;
                m_contactRadian = 0f;
                m_contactGround = null;
                m_controller.SetFollower(null);
            }

            private void CheckCell(Vector2 origin) {
                if (m_isNormalJump) {
                    // If touch cell, stop to jump and fast fall
                    if (Physics2D.BoxCast(origin, m_size, 0f, Vector2.up, ECConstants.MaxContactOffset, ECConstants.BlockMask)) {
                        m_velocity.y = 0f;
                    }
                }
            }

            private void CheckWall(Vector2 origin) { 
                if (m_isWall || m_xVal != 0f || m_controller.ContactDynamics) {
                    RaycastHit2D hitWall;
                    if (m_xVal < 0f) {
                        hitWall = Physics2D.BoxCast(origin, m_size, 0f, Vector2.right, ECConstants.MaxContactOffset, ECConstants.BlockMask);
                        if (UpdateWallInfo(hitWall)) return;
                        hitWall = Physics2D.BoxCast(origin, m_size, 0f, Vector2.left, ECConstants.MaxContactOffset, ECConstants.BlockMask);
                        if (UpdateWallInfo(hitWall)) return;
                    } else {
                        hitWall = Physics2D.BoxCast(origin, m_size, 0f, Vector2.left, ECConstants.MaxContactOffset, ECConstants.BlockMask);
                        if (UpdateWallInfo(hitWall)) return;
                        hitWall = Physics2D.BoxCast(origin, m_size, 0f, Vector2.right, ECConstants.MaxContactOffset, ECConstants.BlockMask);
                        if (UpdateWallInfo(hitWall)) return;
                    }
                } 
                ResetWallInfo();
            }
            private bool UpdateWallInfo(RaycastHit2D hitWall) {
                if (hitWall) {
                    float angle = Vector2.Angle(Vector2.up, hitWall.normal);
                    if ((angle < 90f ? 90f - angle : angle - 90f) < ECConstants.WallLimit) {
                        m_isWall = true;
                        m_contactWall = hitWall.collider;
                        if(hitWall.normal.x < 0f) {
                            m_wallOnRight = true;
                            m_wallOnLeft = false;
                        } else {
                            m_wallOnRight = false;
                            m_wallOnLeft = true;
                        }
                        return true;
                    }
                }
                return false;
            }
            private void ResetWallInfo() {
                m_isWall = false;
                m_contactWall = null;
                m_wallOnLeft = m_wallOnRight = false;
            }
            

            private void CacheSize(EntityController ec) => m_size = ec.Size;

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
