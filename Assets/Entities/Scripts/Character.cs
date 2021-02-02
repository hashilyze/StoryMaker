using UnityEngine;
using Stroy.Platforms;

namespace Stroy.Entities {
    public class Character : Entity {
        #region Public
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
        // Contact cell info
        public bool IsCell => m_isCell;

        // Event
        public System.Action OnLanding;
        public System.Action OnFalling;


        // State Command
        public void SetWallSensor(bool active) {
            m_activeWallSensor = active;
            if (!active) ResetWallInfo();
        }
        // Action Command
        public void Run(float velocity) { m_runSpeed = velocity; }
        public void Jump(float velocity) {
            m_velocity = Controller.Velocity;
            m_velocity.y = velocity;
            Controller.SetVelocity(m_velocity);

            if (velocity >= 0f) { // Jump
                Controller.SetGravityScale(EntityConstants.DefaultGravityScale);
                m_isJump = true;
                m_elapsedJump = 0f;
            } else { // Instants fall
                Controller.SetGravityScale(EntityConstants.FallGravityScale);
                m_isJump = false;
            }
        }
        public bool BreakJump(float scale) {
            if (m_isJump) {
                Controller.SetGravityScale(scale);
                m_isJump = false;
                return true;
            }
            return false;
        }
        public void Grab(Collider2D wall) {
            if (wall.gameObject.layer == PlatformConstants.L_DynamicBlock) {
                Controller.SetFollower(wall.attachedRigidbody);
            }

            m_isClimb = true;
            m_climbWall = wall;
            Controller.SetUseGravity(false);
        }
        public void Release() {
            if (!m_isClimb) return;

            if(m_climbWall.gameObject.layer == PlatformConstants.L_DynamicBlock) {
                Controller.SetFollower(null);
            }

            m_isClimb = false;
            m_climbWall = null;
            Controller.SetUseGravity(true);
        }
        public void Climb(float velocity) {
            if (!m_isClimb) return;

            m_velocity = Controller.Velocity;
            m_velocity.y = velocity;
            Controller.SetVelocity(m_velocity);
        }
        #endregion

        #region Private
        private const float MIN_JUMP_TIME = 0.1f;
        private static readonly Vector2 R_SLOPE_DIR = new Vector2(1f, -1f).normalized;
        private static readonly Vector2 L_SLOPE_DIR = new Vector2(-1f, -1f).normalized;
        private static readonly Vector2 BR_OFFSET = new Vector2(0.5f, -0.5f);
        private static readonly Vector2 BL_OFFSET = new Vector2(-0.5f, -0.5f);
        private static readonly Vector2 TR_OFFSET = new Vector2(0.5f, 0.5f);
        private static readonly Vector2 TL_OFFSET = new Vector2(-0.5f, 0.5f);
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


        // Life cycle
        private void OnDisable() {
            m_velocity = Vector2.zero;
            Controller.SetGravityScale(EntityConstants.DefaultGravityScale);
            // Reset Actions
            Run(0f);
            BreakJump(1f);
            Release();
            // Reset Contacts
            ResetGroundInfo();
            ResetWallInfo();
            ResetCellInfo();
        }
        protected override void Awake() {
            base.Awake();

            // Cache size of entity
            Controller.OnResized += CacheSize;
            if (Controller.Size != Vector2.zero) {
                CacheSize(Controller);
            }
        }


        private void Update() {
            float deltaTime = Time.deltaTime;
            HandleMovement(deltaTime);
        }

        private void HandleMovement(float deltaTime) {
            Vector2 origin = Controller.Position;
            bool wasGround = m_isGround;
            m_velocity = Controller.Velocity;

            if (m_isJump) m_elapsedJump += deltaTime;

            CheckCollision(origin);

            // Recovery slope bouncing
            {
                if (!m_isGround && wasGround && m_lastPos != origin && !m_isJump && !m_isClimb) {
                    Debug.LogWarning($"name: {gameObject.name}");
                    //Debug.Break();
                    RaycastHit2D hitBlock = Physics2D.BoxCast(origin, m_size, 0f, Vector2.down, Mathf.Infinity, 0x01 << PlatformConstants.L_StaticBlock);
                    if (UpdateGroundInfo(hitBlock)) {
                        if (m_velocity.y > 0f && hitBlock.distance <= origin.y - m_lastPos.y + EntityConstants.MaxContactOffset
                            || m_velocity.y <= 0f && hitBlock.distance <= Mathf.Tan(m_contactRadian) * (m_lastPos.x - origin.x) + EntityConstants.MaxContactOffset) {
                            Controller.SetPosition(origin + Vector2.down * (hitBlock.distance - EntityConstants.MinContactOffset));
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
                            if (m_contactRadian != 0f && (!m_isWall || !((m_runSpeed < 0f && m_wallOnLeft) || (m_runSpeed > 0f && m_wallOnRight)) )) { // On slope; only exist climb area
                                m_velocity = m_runSpeed * new Vector2(Mathf.Cos(m_contactRadian), Mathf.Sin(m_contactRadian));
                            } else { // On plane
                                m_velocity = m_runSpeed * Vector2.right;
                            }
                        } else { // Stand
                            m_velocity = Vector2.zero;
                        }
                    } else { // Landing
                        Controller.SetGravityScale(EntityConstants.DefaultGravityScale);
                        m_velocity = Vector2.zero;
                        OnLanding?.Invoke();
                    }
                } else { // On Airbone
                    // Touch cell
                    if (m_isJump && m_isCell) m_velocity.y = 0f;

                    // Falling
                    if (m_velocity.y < 0f && (m_lastVelocity.y >= 0f || wasGround)) {
                        Controller.SetGravityScale(EntityConstants.FallGravityScale);
                        m_isJump = false;
                        OnFalling?.Invoke();
                    }
                    m_velocity.x = m_runSpeed;
                }
            }

            Controller.SetVelocity(m_velocity);

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
                && (m_isGround || m_velocity.y < (m_velocity.x < 0f ? -m_velocity.x : m_velocity.x) || Controller.ContactDynamics)) {
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
                        Controller.SetFollower(hitGround.rigidbody);
                    } else {
                        Controller.SetFollower(null);
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
            if(!m_isClimb) Controller.SetFollower(null);
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
            if (m_isWall || m_runSpeed != 0f || Controller.ContactDynamics) {
                float sign = m_runSpeed < 0f ? -1f : 1f;
                if (m_runSpeed * m_contactRadian > 0f) {
                    if (UpdateWallInfo(WallCast(origin, sign, false))) return;
                    if (UpdateWallInfo(WallCast(origin, sign, true))) return;
                    if (UpdateWallInfo(WallCast(origin, -sign, false))) return;
                    if (UpdateWallInfo(WallCast(origin, -sign, true))) return;
                } else {
                    if (UpdateWallInfo(WallCast(origin, sign, true))) return;
                    if (UpdateWallInfo(WallCast(origin, -sign, true))) return;
                }
            }
            ResetWallInfo();
        }
        private RaycastHit2D WallCast(Vector2 origin, float dir, bool box) {
            if (box) {
                return Physics2D.BoxCast(origin, m_size, 0f, dir * Vector2.right, EntityConstants.MaxContactOffset, PlatformConstants.L_BlockMask);
            } else {
                return Physics2D.Raycast(origin + m_size * (dir < 0f ? TL_OFFSET : TR_OFFSET),
                    dir * Vector2.right, EntityConstants.MaxContactOffset, PlatformConstants.L_BlockMask);
            }            
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

        // Event Driven
        private void CacheSize(EntityController ec) => m_size = ec.Size;
        #endregion
    }
}