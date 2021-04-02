using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StroyMaker.Physics {
    public enum EMovementType { None, Collide, Slide }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public class KinematicCharacterController2D : MonoBehaviour {
        #region Control Interfaces
        /// <summary>Move character with given motion</summary>
        /// <param name="motion">Delta distance to move when next fixed step updated</param>
        public void Move (Vector2 motion) {
            m_motion = motion;
        }


        /// <summary>Horizontal movement</summary>
        /// <param name="velocity">Run velocity; detailed velocity is determinded at applied</param>
        public void Run (float velocity) {
            m_horVelocity = velocity;
        }
        /// <param name="velocity">Intial velocity to jump</param>
        public void Jump (float velocity) {
            m_isJump = true;
            m_gravityScale = PhysicsConstants.k_DefaultGravityScale;
            m_velocity = new Vector2(m_velocity.x, velocity);
        }
        public void Jump (Vector2 velocity) {
            m_isJump = true;
            m_gravityScale = PhysicsConstants.k_DefaultGravityScale;

            m_velocity = velocity;
            m_horVelocity = velocity.x;
        }
        /// <summary>Stop to jump; Character's Vertical velocity smoothly decrease</summary>
        /// <param name="gravityScale">Gravity weight appied until fall</param>
        public void BreakJump (float gravityScale) {
            if (m_isJump) {
                m_isJump = false;
                m_gravityScale = gravityScale;
            }
        }
        /// <summary>Stop to jump; Character immediatlly fall without deaccelleration</summary>
        public void BreakJumpImmediate () {
            if (m_isJump) {
                m_isJump = false;
                m_gravityScale = PhysicsConstants.k_FallGravityScale;
                if (m_velocity.y > 0f) m_velocity.y = 0f;
            }
        }
        #endregion

        #region Properties
        // Components
        public Rigidbody2D Rigidbody => m_rigidbody;
        public Collider2D Collider => m_shape.Collider;
        public Shape2D Shape => m_shape;
        // Transform
        /// <summary>Position after last updated</summary>
        public Vector2 Position { get => m_position; set => SetPosition(value); }
        /// <summary>Rotation after last updated</summary>
        public float Rotation { get => m_rotation; set => SetRotation(value); }
        // Movement
        //public EMovementType MovementType { get => m_movementType; set => m_movementType = value; }
        public bool IsWalker { get => m_isWalker; set => SetWalker(value); }
        public Vector2 Velocity { get => m_velocity; }
        // Collision Response: Kinematic Platform

        // Environment Context
        public bool IsGround => m_isGround;
        public Vector2 GroundNormal => m_groundNormal;
        public bool IsClimbale => m_isClimbable;
        public Collider2D GroundCollider => m_groundCollider;


        private void SetPosition (Vector2 position) {
            // Passed value already set
            if (m_position == position && m_rigidbody.position == position) {
                return;
            }
            m_rigidbody.position = m_position = position;
            m_recoveryAtNextStep = true;
        }
        private void SetRotation (float rotation) {
            // Passed value already set
            if (m_rotation == rotation && m_rigidbody.rotation == rotation) {
                return;
            }
            m_rigidbody.rotation = m_rotation = rotation;
            m_recoveryAtNextStep = true;
        }

        private void SetWalker (bool flag) {
            // Passed value already set
            if (m_isWalker == flag) {
                return;
            }

            m_isWalker = flag;
            if (flag) {
                m_motion = Vector2.zero;
            } else {
                m_horVelocity = 0f;
                m_gravityScale = PhysicsConstants.k_DefaultGravityScale;
                m_isJump = false;
            }
        }
        #endregion

        #region Variables
        [Header("Query Buffter")]   // All buffers are reused
        private const int k_HitBufferSize = 8;
        private const int k_ColliderBufferSize = 8;
        private const int k_ContactBufferSize = k_ColliderBufferSize * 2;
        private static readonly RaycastHit2D[] m_bufferHit = new RaycastHit2D[k_HitBufferSize];
        private static readonly Collider2D[] m_bufferCollider = new Collider2D[k_ColliderBufferSize];
        private static readonly ContactPoint2D[] m_bufferContact = new ContactPoint2D[k_ContactBufferSize];
        [Header("Components")]
        [SerializeField] private Rigidbody2D m_rigidbody;
        [SerializeField] private Shape2D m_shape;
        [Header("Transform")]
        private Vector2 m_position;
        private float m_rotation;
        [Header("Movement")]
        //[SerializeField] private EMovementType m_movementType = EMovementType.Slide;
        [SerializeField] private bool m_isWalker = true;
        private bool m_isAwake = true;
        private Vector2 m_velocity;
        private Vector2 m_impactedMotion;
        [Header("Collision Response")]  // Default response to kinematic platform
        private readonly Queue<Collider2D> m_reservedSeperatePlatforms = new Queue<Collider2D>(k_ColliderBufferSize);
        private readonly List<Collider2D> m_ignoredColliders = new List<Collider2D>(k_ColliderBufferSize);
        private int m_movingCount;
        private bool m_recoveryAtNextStep;
        [Header("Riding")]  // Special case of response to kinematic platform
        [SerializeField] private KinematicPlatformController2D m_riding;
        private bool m_hasRiding;
        [Header("Environment Context")]
        private bool m_isGround;
        private Vector2 m_groundNormal;
        private bool m_isClimbable;
        private Collider2D m_groundCollider;
        [Header("Walker Configures")]
        private float m_horVelocity;
        private float m_gravityScale = PhysicsConstants.k_DefaultGravityScale;
        private bool m_isJump;
        [Header("Custom Configures")]
        private Vector2 m_motion;
        #endregion

        #region Activation
        private void Reset () {
            // Setup rigidbody
            m_rigidbody = GetComponent<Rigidbody2D>();
            m_rigidbody.isKinematic = true;
            m_rigidbody.useFullKinematicContacts = true;
            // Setup shape
            m_shape = GetComponent<Shape2D>();
        }

        private void Awake () {
            // If didn't reseted, reset now
            if (m_rigidbody == null) {
                m_rigidbody = GetComponent<Rigidbody2D>();
                m_rigidbody.isKinematic = true;
                m_rigidbody.useFullKinematicContacts = true;
            }
            if (m_shape == null) {
                m_shape = GetComponent<Shape2D>();
            }

            CharacterHolder.Add(this);
        }
        private void OnDestroy () {
            CharacterHolder.Remove(this);
        }

        private void OnEnable () {
            // Reset variables
            m_isAwake = true;
            m_velocity = Vector2.zero;

            m_horVelocity = 0f;
            m_gravityScale = PhysicsConstants.k_DefaultGravityScale;
            m_isJump = false;

            m_motion = Vector2.zero;

            OverlapRecovery();
        }
        private void OnDisable () {
            // Dircard values
            m_velocity = Vector2.zero;

            m_movingCount = 0;

            if (m_isGround) ResetGround();
        }
        #endregion

        #region Update
        /// <summary>
        /// Update state whether Awake or Asleep
        /// If have 'motion' or trigger 'response' awake; otherwise asleep
        /// </summary>
        private void UpdateState () {
            m_isAwake = false;

            if (m_velocity != Vector2.zero || m_movingCount > 0 || m_impactedMotion != Vector2.zero || m_hasRiding) {
                m_isAwake = true;
                return;
            }

            if (m_isWalker) {
                if (m_horVelocity != 0f || !m_isGround || !m_isClimbable) {
                    m_isAwake = true;
                }
            } else {
                if (m_motion != Vector2.zero) {
                    m_isAwake = true;
                }
            }
        }

        private void FixedUpdate () {
            // --- Inititialize --- 
            // Synchronize transform with kinematic actor
            {
                m_position = m_rigidbody.position;
                m_rotation = m_rigidbody.rotation;
            }
            // Fetch transform
            Vector2 position = m_position;
            float rotation = m_rotation;

            // If sleep, check state before execute
            if (!m_isAwake) {
                UpdateState();
                // Early exit if sleep
                if (!m_isAwake) return;
            }
            // Fetch deltaTime
            float deltaTime = Time.fixedDeltaTime;

            // --- Overlap Recovery ----
            // Seperate with overlapped platforms
            {
                // Reserve all surround platforms to seperate target
                if (m_recoveryAtNextStep) {
                    m_recoveryAtNextStep = false;

                    int platformNum = m_shape.Overlap(position, rotation, m_bufferCollider, PhysicsConstants.k_Mask_Platform);
                    for (int n = 0; n != platformNum; ++n) {
                        Collider2D platform = m_bufferCollider[n];
                        m_reservedSeperatePlatforms.Enqueue(platform);
                    }
                }

                while (m_reservedSeperatePlatforms.Count > 0) {
                    Seperate(ref position, rotation, m_reservedSeperatePlatforms.Dequeue());
                }
            }

            // --- Movement Control ---
            if (m_isWalker) {
                // Compute new internal velocity
                GetClosestHit(position, rotation, Vector2.down, out RaycastHit2D downHit, PhysicsConstants.k_MaxContactOffset);
                UpdateGroundContext(downHit);
                UpdateVelocity(deltaTime);

                // Apply internal velocity
                Vector2 beforePosition = position;
                ApplySafetyMovement(ref position, rotation, m_velocity * deltaTime, out RaycastHit2D hit, EMovementType.Slide);
                m_velocity = (position - beforePosition) / deltaTime;

                // Update Ground Context
                if (m_isGround) {
                    if (m_isJump) { // Jump
                        ResetGround();
                    } else if (m_velocity.x != 0f) { // Walk
                        // Snap to ground
                        float groundOffset = (m_velocity.y > 0f ? m_velocity.y : Mathf.Abs(m_velocity.x)) * deltaTime;
                        bool isHit = GetClosestHit(position, rotation, Vector2.down, out RaycastHit2D groundHit, groundOffset + PhysicsConstants.k_MaxContactOffset);
                        if (isHit && groundHit.distance >= PhysicsConstants.k_MaxContactOffset && Vector2.Angle(Vector2.up, groundHit.normal) <= PhysicsConstants.k_SlopeLimit) {
                            ApplySafetyMovement(ref position, rotation, (groundHit.distance + PhysicsConstants.k_CastBuffer) * Vector2.down + (PhysicsConstants.k_CastBuffer + PhysicsConstants.k_MinContactOffset) * groundHit.normal, out _, EMovementType.None);
                        }
                        UpdateGroundContext(groundHit);
                    } else { // Stand

                    }
                } else {
                    if (m_velocity.y > 0f && m_velocity.x == 0f) { // Only move up
                        ResetGround();
                    } else {
                        UpdateGroundContext(hit);
                    }
                }

                // Synchronize horizontal velocity 
                {
                    if (m_isGround && m_isClimbable) {
                        // Use all aixs for horizontal movement
                        m_horVelocity = m_velocity.magnitude * Mathf.Sign(m_velocity.x);
                    } else {
                        // Don't use y-axis for horizontal movement
                        m_horVelocity = m_velocity.x;
                    }
                }

                // Update riding
                {
                    if (m_isGround && PhysicsUtility.IsDynamic(m_groundCollider.gameObject)) {
                        // Call Ride if require to change
                        if (!m_hasRiding || m_riding.Rigidbody != m_groundCollider.attachedRigidbody) {
                            if (PlatformHolder.Find(m_groundCollider.gameObject, out KinematicPlatformController2D platform)) {
                                Ride(platform);
                            }
                        }
                    } else {
                        Ride(null);
                    }
                }
            } else {
                // Consume passed motion
                Vector2 beforePosition = position;
                ApplySafetyMovement(ref position, rotation, m_motion, out _, EMovementType.Slide);
                m_velocity = (position - beforePosition) / deltaTime;
                m_motion = Vector2.zero;
            }

            // --- After behaviors ----
            {
                if (m_impactedMotion != Vector2.zero) {
                    ApplySafetyMovement(ref position, rotation, m_impactedMotion, out _, EMovementType.Slide);
                    m_impactedMotion = Vector2.zero;
                }
                if (m_hasRiding) {
                    MoveOnRiding(ref position, rotation, deltaTime);
                }
                ResponseToKinematicPlatform(ref position, rotation, deltaTime);
            }

            // --- Terminate ---
            m_position = position;
            // Update position of rigidbody in physics scene to inverse kinematics
            m_rigidbody.MovePosition(position);

            // If awake, check sleep after execute
            if (m_isAwake) {
                UpdateState();
            }
        }
        #endregion

        #region Query : GetClosestHit
        /// <summary>Find the closest hit platform with attached shape</summary>
        /// <returns>If find valid hit, return true, otherwise false</returns>
        public bool GetClosestHit (Vector2 position, float rotation, Vector2 direction, out RaycastHit2D result, float distance = Mathf.Infinity) {
            int hitNum = m_shape.Sweep(position, rotation, direction, m_bufferHit, distance, PhysicsConstants.k_Mask_Platform);
            // Early exit if nothing hit
            if (hitNum == 0) {
                result = new RaycastHit2D();
                return false;
            }

            bool hasValidHit = false;
            int closestHit = -1;

            for (int n = 0; n != hitNum; ++n) {
                ref RaycastHit2D sweptHit = ref m_bufferHit[n];
                if (/*m_rigidbody == sweptHit.rigidbody ||*/
                    (Vector2.Dot(sweptHit.normal, direction) >= 0f) ||
                    //Physics2D.GetIgnoreCollision(m_shape.Collider, sweptHit.collider) ||
                    m_ignoredColliders.Contains(sweptHit.collider) ||
                    !IsTouchedPlatform(sweptHit.collider, sweptHit.normal)) {
                    continue;
                }

                // Update closest hit
                if (!hasValidHit || sweptHit.distance < distance) {
                    hasValidHit = true;
                    closestHit = n;
                    distance = sweptHit.distance;

                    // Stop query if hit inside collider
                    if (sweptHit.distance <= 0f) {
                        break;
                    }
                }
            }

            result = hasValidHit ? m_bufferHit[closestHit] : new RaycastHit2D();
            return hasValidHit;
        }

        /// <summary>Sweep or Raycast filter to find touched platform</summary>
        private static bool IsTouchedPlatform (Collider2D collider, Vector2 normal) {
            GameObject gameObject = collider.gameObject;
            if (PhysicsUtility.IsBlock(gameObject)) {
                return true;
            } else if (PhysicsUtility.IsOneway(gameObject)) {
                return Vector2.Dot(gameObject.transform.up, normal) > 0f;
            }

            throw new System.ArgumentException("Passed object is not platform or undefined");
        }
        #endregion

        #region Solver: Movement
        /// <summary>Move the character safety</summary>
        /// <param name="position">Position of character</param>
        /// <param name="rotation">Rotation of character</param>
        /// <param name="distance">Movement distance</param>
        /// <param name="collision">The last collide infomation</param>
        /// <param name="movementType">Movement solve type</param>
        /// <returns>If moved, return true otehrwise false</returns>
        private bool ApplySafetyMovement (ref Vector2 position, float rotation, Vector2 distance, out RaycastHit2D collision, EMovementType movementType) {
            collision = new RaycastHit2D();
            bool hasMoved = false;

            // Early exit if no move 
            if (distance.sqrMagnitude < PhysicsConstants.k_MinDistance * PhysicsConstants.k_MinDistance) return hasMoved;

            switch (movementType) {
            case EMovementType.None: {
                // Don't require to query objectcles
                position += distance;
                hasMoved = true;
                break;
            }
            case EMovementType.Collide: {
                float magnitude = distance.magnitude;
                Vector2 direction = distance.normalized;

                if (GetClosestHit(position, rotation, direction, out RaycastHit2D hit, magnitude + PhysicsConstants.k_MinContactOffset)) {
                    collision = hit;

                    float margin = PhysicsConstants.k_MinContactOffset / -Vector2.Dot(hit.normal, direction);
                    float moveDistance = hit.distance - margin;

                    if (moveDistance > PhysicsConstants.k_MinDistance) {
                        position += moveDistance * direction;
                        hasMoved = true;
                    }
                } else {
                    position += distance;
                    hasMoved = true;
                }
                break;
            }
            case EMovementType.Slide: {
                // Try move until not collided or over interation count
                for (int iteration = 0; iteration != PhysicsConstants.k_MaxIteration; ++iteration) {
                    float magnitude = distance.magnitude;
                    Vector2 direction = distance.normalized;

                    // If collide then slide surface of object
                    if (GetClosestHit(position, rotation, direction, out RaycastHit2D hit, magnitude + PhysicsConstants.k_MinContactOffset)) {
                        collision = hit;

                        // Move until collide
                        float margin = PhysicsConstants.k_MinContactOffset / -Vector2.Dot(hit.normal, direction);
                        float moveDistance = hit.distance - margin;
                        if (moveDistance > PhysicsConstants.k_MinDistance) {
                            position += moveDistance * direction;
                            hasMoved = true;
                        }

                        // If over iteraction count; don't slide and just collide
                        if (iteration >= PhysicsConstants.k_MaxIteration - 1) {
                            break;
                        }

                        // Slide surface of object
                        magnitude -= moveDistance;
                        Vector2 tangent = new Vector2(hit.normal.y, -hit.normal.x);
                        float newMagnitude; // (*)
                        if (!m_isWalker) {
                            newMagnitude = Vector2.Dot(tangent, direction) * magnitude;
                        } else if (Vector2.Angle(Vector2.up, hit.normal) < PhysicsConstants.k_SlopeLimit) {
                            newMagnitude = Vector2.Dot(tangent, new Vector2(direction.x, 0f)) * magnitude;
                        } else {
                            newMagnitude = Vector2.Dot(tangent, new Vector2(0f, direction.y)) * magnitude;
                        }

                        // Stop if left distance below min distance
                        if (-PhysicsConstants.k_MinDistance < newMagnitude && newMagnitude < PhysicsConstants.k_MinDistance) {
                            break;
                        }
                        distance = newMagnitude * tangent;

                    } else { // If not collide, no require more try
                        position += distance;
                        hasMoved = true;
                        break;
                    }
                }
                break;
            }
            default: {
                throw new System.NotImplementedException("Current movement type is undefined");
            }
            }

            return hasMoved;
        }
        #endregion


        #region Query: Contact Context
        private void OnCollisionEnter2D (Collision2D collision) {
            if (PhysicsUtility.IsPlatform(collision.gameObject)) {
                Collider2D platform = collision.collider;

                if (PhysicsUtility.IsDynamicBlock(platform.gameObject)) {
                    ++m_movingCount;
                    m_reservedSeperatePlatforms.Enqueue(platform);
                } else if (PhysicsUtility.IsDynamicOneway(platform.gameObject)) {
                    ++m_movingCount;
                    if (Vector2.Dot(platform.transform.up, collision.GetContact(0).normal) > 0f) {
                        m_reservedSeperatePlatforms.Enqueue(platform);
                    } else {
                        m_ignoredColliders.Add(collision.collider);
                    }
                } else if (PhysicsUtility.IsStaticOneway(platform.gameObject)) {
                    if (Vector2.Dot(platform.transform.up, collision.GetContact(0).normal) > 0f) {

                    } else {
                        m_ignoredColliders.Add(collision.collider);
                    }
                }
            }
        }
        private void OnCollisionExit2D (Collision2D collision) {
            if (PhysicsUtility.IsPlatform(collision.gameObject)) {
                Collider2D platform = collision.collider;
                if (PhysicsUtility.IsDynamic(platform.gameObject)) {
                    --m_movingCount;
                }
                if (PhysicsUtility.IsOneway(platform.gameObject)) {
                    m_ignoredColliders.Remove(platform);
                }
            }
        }
        #endregion

        #region Solver: Overlap Recovery
        [ContextMenu("OverlapRecovery")]
        public void OverlapRecovery () {
            m_isAwake = true;
            m_recoveryAtNextStep = true;
        }
        private void Seperate (ref Vector2 position, float rotation, Collider2D collider) {
            if (m_shape.ComputePenetration(position, rotation, collider, out Vector2 direction, out float distance, out _, out _)
                && distance > -PhysicsConstants.k_MinContactOffset) {
                ApplySafetyMovement(ref position, rotation, (distance + PhysicsConstants.k_MinContactOffset + PhysicsConstants.k_CastBuffer) * direction, out _, EMovementType.Collide);
            }
        }
        #endregion

        #region Solver: Collision Response to Kinematic Platform
        private void ResponseToKinematicPlatform (ref Vector2 position, float rotation, float deltaTime) {
            if (m_movingCount == 0) {
                return;
            }

            Vector2 pushedDistance = Vector2.zero;
            //int num = m_shape.GetContacts(position, rotation, m_bufferContact, m_movingPlatformFilter);
            int num = m_shape.GetContacts(m_position, m_rotation, m_bufferContact, PhysicsConstants.k_Mask_Dynamic);

            Rigidbody2D prevPlatform_rigidbody = null;
            for (int n = 0; n != num; ++n) {
                ref ContactPoint2D point = ref m_bufferContact[n];
                Collider2D platform_collider = point.collider;
                Rigidbody2D platform_rigidbody = point.rigidbody;

                // Filtering platforms to response
                {
                    // Response apllied one per platform
                    if (prevPlatform_rigidbody == platform_rigidbody) {
                        continue;
                    }

                    // Riding is ignored
                    if (m_hasRiding && m_riding.Rigidbody == platform_rigidbody) {
                        continue;
                    }
                    // Ignore ignore-colliders
                    if (m_ignoredColliders.Contains(platform_collider)) {
                        continue;
                    }
                    // Check is oneway contacted
                    if (PhysicsUtility.IsDynamicOneway(platform_collider.gameObject) && Vector2.Dot(platform_collider.transform.up, point.normal) <= 0f) {
                        continue;
                    }
                }

                // Update Pushed-Distance
                Vector2 normal = point.normal;
                Vector2 velocity = platform_rigidbody.velocity;
                // Pushing apply toward platform's normal of surface
                if (velocity != Vector2.zero && Vector2.Dot(normal, velocity) > 0f) {
                    // Push character from position after character's velocity applied
                    float magnitude = deltaTime * Mathf.Max(Vector2.Dot(normal, velocity - m_velocity), 0f);
                    // Pushed-Distance is combined with current Pushed-Distance
                    pushedDistance += Mathf.Max(magnitude - Vector2.Dot(normal, pushedDistance), 0f) * normal;
                }

                prevPlatform_rigidbody = platform_rigidbody;
            }

            ApplySafetyMovement(ref position, rotation, pushedDistance, out _, EMovementType.Collide);
        }
        #endregion

        #region Solver: Ride
        /// <summary>Stick to platform</summary>
        public void Ride (KinematicPlatformController2D riding) {
            m_riding = riding;
            m_hasRiding = riding != null;
        }

        private void MoveOnRiding (ref Vector2 position, float rotation, float deltaTime) {
            // Apply linear velocity
            Vector2 targetLinearVelocity = m_riding.Velocity;
            if (targetLinearVelocity != Vector2.zero) {
                for (int beg = 0, end = m_riding.ColliderCount; beg != end; ++beg) {
                    m_ignoredColliders.Add(m_riding.GetCollider(beg));
                }
                ApplySafetyMovement(ref position, rotation, deltaTime * targetLinearVelocity, out _, EMovementType.Collide);
                m_ignoredColliders.RemoveRange(m_ignoredColliders.Count - m_riding.ColliderCount, m_riding.ColliderCount);
            }
        }
        #endregion


        #region Query: Environment Context
        private void UpdateGroundContext (RaycastHit2D downHit) {
            bool wasGround = m_isGround;

            if (downHit) {
                float angle = Vector2.Angle(Vector2.up, downHit.normal);
                if (angle <= PhysicsConstants.k_SlopeLimit) {
                    SetGround(downHit, true);
                } else if (angle < 90f) {
                    SetGround(downHit, false);
                } else {
                    if (m_isGround) ResetGround();
                }
            } else {
                if (m_isGround) ResetGround();
            }

            // Event: On Land
            if (m_isGround && !wasGround) {
                m_isJump = false;
                m_gravityScale = PhysicsConstants.k_DefaultGravityScale;
                // Discard jump or fall velocity
                if (m_isClimbable) {
                    m_velocity.y = 0f;
                }
            }
        }
        private void SetGround (RaycastHit2D groundHit, bool isClimbable) {
            m_isGround = true;
            m_groundNormal = m_groundNormal = groundHit.normal;
            m_isClimbable = isClimbable;
            m_groundCollider = groundHit.collider;
        }
        private void ResetGround () {
            m_isGround = false;
            m_groundNormal = Vector2.zero;
            m_isClimbable = false;
            m_groundCollider = null;
        }
        #endregion

        #region Solver: Compute Internal Velocity for Walker
        /// <summary>Update velocity by control commands</summary>
        private void UpdateVelocity (float deltaTime) {
            // Fetch lastest velocity
            Vector2 velocity = m_velocity;

            if (m_isJump || !m_isGround) {
                // Vertical velocity controled by gravity or jumping
                {
                    //velocity.y = Mathf.Max(fallSpeed - PhysicsConstants.k_Gravity * deltaTime * m_gravityScale, -PhysicsConstants.k_FallLimit);

                    // Add gravity until vertical velocity reach to fall-limit
                    float fallSpeed = velocity.y;
                    if (fallSpeed > -PhysicsConstants.k_FallLimit) {
                        fallSpeed -= PhysicsConstants.k_Gravity * deltaTime * m_gravityScale;

                        if (fallSpeed < -PhysicsConstants.k_FallLimit) {
                            fallSpeed = -PhysicsConstants.k_FallLimit;
                        }
                    } else {
                        fallSpeed = -PhysicsConstants.k_FallLimit;
                    }
                    velocity.y = fallSpeed;
                }
                // Horizontal move smae as walk on plane
                {
                    velocity.x = m_horVelocity;
                }

                // While falling
                if (m_velocity.y < 0f) {
                    m_isJump = false;
                    m_gravityScale = PhysicsConstants.k_FallGravityScale;
                }
            } else if (m_isClimbable) {
                // Move on ground, snapping surface of ground
                velocity = m_horVelocity * new Vector2(m_groundNormal.y, -m_groundNormal.x);
            } else {
                // Slide tangent of ground
                {
                    Vector2 tangentVector = m_groundNormal.x < 0f ? new Vector2(m_groundNormal.y, -m_groundNormal.x) : new Vector2(-m_groundNormal.y, m_groundNormal.x);

                    float fallSpeed = velocity.y;
                    if (fallSpeed > -PhysicsConstants.k_FallLimit) {
                        fallSpeed -= PhysicsConstants.k_Gravity * deltaTime * m_gravityScale * tangentVector.y;
                        if (fallSpeed < -PhysicsConstants.k_FallLimit) {
                            fallSpeed = -PhysicsConstants.k_FallLimit;
                        }
                        velocity = fallSpeed * new Vector2(tangentVector.x / tangentVector.y, 1f);
                    } else {
                        velocity = -PhysicsConstants.k_FallLimit * new Vector2(tangentVector.x / tangentVector.y, 1f);
                    }
                }

                // Apply horizontal velocity if detached to wall
                if (m_horVelocity * m_groundNormal.x > 0f && Mathf.Abs(m_horVelocity) > Mathf.Abs(velocity.x)) {
                    velocity.x = m_horVelocity;
                }

                // While sliding
                if (velocity.y < 0f) {
                    m_isJump = false;
                    m_gravityScale = PhysicsConstants.k_FallGravityScale;
                }
            }

            m_velocity = velocity;
        }
        #endregion


        #region After Behavior: External motion
        public void AddMotion (Vector2 motion) {
            m_impactedMotion += motion;
        }
        #endregion

#if UNITY_EDITOR
        #region Inspector
        private void OnValidate () {
            Ride(m_riding);
        }
        #endregion

        #region DEBUG
        private void OnDrawGizmos () {
            if (m_shape == null) return;

            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;
            if (m_shape.Collider is BoxCollider2D box) {
                AdvandcedGizmos.DrawWireCube(position, rotation, box.size, Color.green);
                AdvandcedGizmos.DrawWireRoundCube(position, rotation, box.size + 2f * PhysicsConstants.k_MaxContactOffset * Vector2.one, PhysicsConstants.k_MaxContactOffset, Color.blue);
            } else if (m_shape.Collider is CapsuleCollider2D capsule) {
                AdvandcedGizmos.DrawWireCapsule(position, rotation, capsule.size, Color.green);
                AdvandcedGizmos.DrawWireCapsule(position, rotation, capsule.size + 2f * PhysicsConstants.k_MaxContactOffset * Vector2.one, Color.blue);
            } else if (m_shape.Collider is CircleCollider2D circle) {
                AdvandcedGizmos.DrawWireSphere(position, circle.radius, Color.green);
                AdvandcedGizmos.DrawWireSphere(position, circle.radius + PhysicsConstants.k_MaxContactOffset, Color.blue);
            }
        }
        #endregion
#endif
    }


}