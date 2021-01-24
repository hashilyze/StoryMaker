using UnityEngine;
using Stroy.Platform;

namespace Stroy.Entity {
    [RequireComponent(typeof(EntityController))]
    public class PlatformerController : MonoBehaviour {
        #region Public
        // Component
        public EntityController Entity => m_entity;
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
        public System.Action OnLanding;
        public System.Action OnFalling;


        // State Command
        public void SetWallSensor(bool active) {
            m_activeWallSensor = active;
            if (active == false) ResetWallInfo();
        }
        // Action Command
        public void Run(float velocity) { m_runSpeed = velocity; }
        public void Jump(float velocity) {
            m_velocity = m_entity.Velocity;
            m_velocity.y = velocity;
            m_entity.SetVelocity(m_velocity);

            if (velocity >= 0f) {
                m_entity.SetGravityScale(EntityConstants.DefaultGravityScale);
                m_isJump = true;
                m_elapsedJump = 0f;
            } else {
                m_entity.SetGravityScale(EntityConstants.FallGravityScale);
                m_isJump = false;
            }
        }
        public bool BreakJump(float scale) {
            if (m_isJump) {
                m_entity.SetGravityScale(scale);
                m_isJump = false;
                return true;
            }
            return false;
        }
        

        public void Grab(Collider2D wall) {
            if (wall.gameObject.layer == PlatformConstants.L_DynamicBlock) {
                m_entity.SetFollower(wall.attachedRigidbody);
                m_entity.SetFollowSolver(FollowOnGrab);
            }

            m_isClimb = true;
            m_climbWall = wall;
            m_entity.SetUseGravity(false);
        }
        public void Release() {
            if (!m_isClimb) return;

            if(m_climbWall.gameObject.layer == PlatformConstants.L_DynamicBlock) {
                m_entity.SetFollower(null);
                m_entity.SetFollowSolver(FollowOnStep);
            }

            m_isClimb = false;
            m_climbWall = null;
            m_entity.SetUseGravity(true);
        }
        public void Climb(float velocityY) {
            if (!m_isClimb) return;

            m_velocity = m_entity.Velocity;
            m_velocity.y = velocityY;
            m_entity.SetVelocity(m_velocity);
        }
        #endregion

        #region Private
        private const float MIN_JUMP_TIME = 0.1f;
        private static readonly Vector2 R_SLOPE_DIR = new Vector2(1f, -1f).normalized;
        private static readonly Vector2 L_SLOPE_DIR = new Vector2(-1f, -1f).normalized;
        private static readonly Vector2 BR_OFFSET = new Vector2(0.5f, -0.5f);
        private static readonly Vector2 BL_OFFSET = new Vector2(-0.5f, -0.5f);
        // Component
        [HideInInspector] private EntityController m_entity;
        // State
        [SerializeField] private Vector2 m_velocity;
        [HideInInspector] private Vector2 m_size;
        [HideInInspector] private Vector2 m_lastVelocity;
        [HideInInspector] private Vector2 m_lastPos;
        // Run State
        private float m_runSpeed;
        // Jump State
        private bool m_isJump;
        private float m_elapsedJump;
        // Climb State
        private bool m_isClimb;
        private Collider2D m_climbWall;
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
        // Contact cell info
        [SerializeField] private bool m_isCell;


        private void OnDisable() {
            m_velocity = Vector2.zero;
            
            Run(0f);
            BreakJump(1f);
            Release();

            ResetGroundInfo();
            ResetWallInfo();
            ResetCellInfo();

            m_entity.SetGravityScale(EntityConstants.DefaultGravityScale);
        }

        private void Awake() {
            m_entity = GetComponent<EntityController>();

            if (m_entity.FollowSolver == null) {
                m_entity.SetFollowSolver(FollowOnStep);
            }
            // Cache size of entity
            m_entity.OnResized += CacheSize;
            if (m_entity.Size != Vector2.zero) {
                CacheSize(m_entity);
            }
        }
        private void Update() { HandleMovement(); }

        private void HandleMovement() {
            float deltaTime = Time.deltaTime;
            Vector2 origin = m_entity.Position;
            bool wasGround = m_isGround;
            m_velocity = m_entity.Velocity;

            if (m_isJump) m_elapsedJump += deltaTime;

            CheckCollision(origin);

            // Revise conner bouncing
            {
                if (!m_isGround && wasGround && m_lastPos != origin && !m_isJump && !m_isClimb) {
                    RaycastHit2D hitBlock = Physics2D.BoxCast(origin, m_size, 0f, Vector2.down, Mathf.Infinity, 0x01 << PlatformConstants.L_StaticBlock);
                    if (UpdateGroundInfo(hitBlock)) {
                        if (m_velocity.y > 0f && hitBlock.distance <= origin.y - m_lastPos.y + EntityConstants.MaxContactOffset
                            || m_velocity.y <= 0f && hitBlock.distance <= Mathf.Tan(m_contactRadian) * (m_lastPos.x - origin.x) + EntityConstants.MaxContactOffset
                            ) {
                            m_entity.SetPosition(origin + Vector2.down * (hitBlock.distance - EntityConstants.MinContactOffset));
                            // End HandleMovement
                            m_lastVelocity = m_velocity;
                            m_lastPos = origin;
                            return;
                        } else {
                            ResetGroundInfo();
                        }
                    }
                }
            }

            // Update Velocity
            {
                
                if (m_isGround) { // On Ground
                    if (wasGround) {
                        if (m_runSpeed != 0f) { // Run
                            if (m_contactRadian != 0f
                                && (!m_isWall || (m_runSpeed < 0f && m_wallOnRight) || (m_runSpeed > 0f && m_wallOnLeft))
                                ) { // On slope
                                m_velocity = m_runSpeed * new Vector2(Mathf.Cos(m_contactRadian), Mathf.Sin(m_contactRadian));
                            } else { // On plane
                                m_velocity = m_runSpeed * Vector2.right;
                            }
                        } else { // Stand
                            m_velocity = Vector2.zero;
                        }
                    } else { // Landing
                        m_entity.SetGravityScale(EntityConstants.DefaultGravityScale);
                        m_velocity = Vector2.zero;
                        OnLanding?.Invoke();
                    }
                } else { // On Airbone
                    if (m_isJump && m_isCell) m_velocity.y = 0f;

                    // Fall
                    if (m_velocity.y < 0f) {
                        if (m_lastVelocity.y >= 0f) {
                            OnFalling?.Invoke();
                        }
                        m_entity.SetGravityScale(EntityConstants.FallGravityScale);
                        m_isJump = false;
                    }
                    m_velocity.x = m_runSpeed;
                }
            }

            m_entity.SetVelocity(m_velocity);

            m_lastVelocity = m_velocity;
            m_lastPos = origin;
        }

        #region Platform Sensor
        private void CheckCollision(Vector2 origin) {
            CheckGround(origin);
            CheckCell(origin);
            if (m_activeWallSensor) CheckWall(origin);
        }

        private void CheckGround(Vector2 origin) {
            if ((!m_isClimb || m_velocity.y <= 0f)
                && (!m_isJump || m_elapsedJump > MIN_JUMP_TIME)
                && (m_isGround || m_velocity.y < (m_velocity.x < 0f ? -m_velocity.x : m_velocity.x) || m_entity.ContactDynamics)) {
                // Check uphill
                if (m_runSpeed < 0f) { // Move left
                    if (UpdateGroundInfo(Physics2D.Raycast(origin + m_size * BL_OFFSET, L_SLOPE_DIR, EntityConstants.MaxContactOffset, PlatformConstants.L_BlockMask))) return;
                } else if (m_runSpeed > 0f) { // Move right
                    if (UpdateGroundInfo(Physics2D.Raycast(origin + m_size * BR_OFFSET, R_SLOPE_DIR, EntityConstants.MaxContactOffset, PlatformConstants.L_BlockMask))) return;
                }
                if (UpdateGroundInfo(Physics2D.BoxCast(origin, m_size, 0f, Vector2.down, EntityConstants.MaxContactOffset, PlatformConstants.L_BlockMask))) return;
            }
            // No ground
            ResetGroundInfo();
        }
        private bool UpdateGroundInfo(RaycastHit2D hitGround) {
            if (hitGround) {
                float angle = Vector2.SignedAngle(Vector2.up, hitGround.normal);
                if ((angle < 0f ? -angle : angle) < EntityConstants.SlopeLimit) {
                    m_isGround = true;
                    m_contactRadian = angle * Mathf.Deg2Rad;
                    m_contactGround = hitGround.collider;
                    m_isJump = false;

                    if (hitGround.collider.gameObject.layer == PlatformConstants.L_DynamicBlock) {
                        m_entity.SetFollower(hitGround.rigidbody);
                    } else {
                        m_entity.SetFollower(null);
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
            if(!m_isClimb) m_entity.SetFollower(null);
        }

        private void CheckCell(Vector2 origin) {
            if (m_isJump) {
                // If touch cell, stop to jump and fast fall
                if (UpdateCellInfo(Physics2D.BoxCast(origin, m_size, 0f, Vector2.up, EntityConstants.MaxContactOffset, PlatformConstants.L_BlockMask))) return;
            }
            ResetCellInfo();
        }
        private bool UpdateCellInfo(RaycastHit2D hitCell) {
            if (hitCell) {
                m_isCell = true;
                return true;
            }
            return false;
        }
        private void ResetCellInfo() {
            m_isCell = false;
        }


        private void CheckWall(Vector2 origin) {
            if (m_isWall || m_runSpeed != 0f || m_entity.ContactDynamics) {
                if (m_runSpeed < 0f) {
                    if (UpdateWallInfo(Physics2D.BoxCast(origin, m_size, 0f, Vector2.left, EntityConstants.MaxContactOffset, PlatformConstants.L_BlockMask))) return;
                    if (UpdateWallInfo(Physics2D.BoxCast(origin, m_size, 0f, Vector2.right, EntityConstants.MaxContactOffset, PlatformConstants.L_BlockMask))) return;
                } else {
                    if (UpdateWallInfo(Physics2D.BoxCast(origin, m_size, 0f, Vector2.right, EntityConstants.MaxContactOffset, PlatformConstants.L_BlockMask))) return;
                    if (UpdateWallInfo(Physics2D.BoxCast(origin, m_size, 0f, Vector2.left, EntityConstants.MaxContactOffset, PlatformConstants.L_BlockMask))) return;
                }
            }
            ResetWallInfo();
        }
        private bool UpdateWallInfo(RaycastHit2D hitWall) {
            if (hitWall) {
                float angle = Vector2.Angle(Vector2.up, hitWall.normal);
                if ((angle < 90f ? 90f - angle : angle - 90f) < EntityConstants.WallLimit) {
                    m_isWall = true;
                    m_contactWall = hitWall.collider;
                    m_wallOnRight = hitWall.normal.x < 0f;
                    m_wallOnLeft = !m_wallOnRight;
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
        #endregion


        private void CacheSize(EntityController ec) => m_size = ec.Size;

        private static Vector2 FollowOnStep(Rigidbody2D followPlatform, float deltaTime) {
            Vector2 followDistance = Vector2.zero;
            followDistance.x = followPlatform.velocity.x * deltaTime;
            if (followPlatform.velocity.y < 0f) {
                followDistance.y = followPlatform.velocity.y * deltaTime;
            }
            return followDistance;
        }
        private static Vector2 FollowOnGrab(Rigidbody2D followPlatform, float deltaTime) {
            return followPlatform.velocity * deltaTime;
        }
        #endregion
    }
}