using UnityEngine;
using Stroy.Platforms;

namespace Stroy.Entities {
    [RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
    public sealed class EntityBody : MonoBehaviour {
        #region Public
        // Component
        public Rigidbody2D Rigidbody => m_rigidbody;
        public BoxCollider2D Collider => m_collider;
        // Entity State
        public Vector2 Size => m_size;
        public Vector2 Velocity { get => m_velocity; set => SetVelocity(value); }
        public Vector2 Position => m_rigidbody.position;
        public Vector2 Center => m_collider.bounds.center;
        public bool UseGravity { get => m_useGravity; set => SetUseGravity(value); }
        public float GravityScale { get => m_gravityScale; set => SetGravityScale(value); }
        // Contact Dynamics
        public bool ContactDynamics => m_dynamicBlockNum > 0;
        public PlatformController FollowPlatform { get => m_followPlatform; set => SetFollower(value); }
        // Event
        /// <summary>Entity can't react because of stucking</summary>
        public System.Action<EntityBody> OnFrozen;
        /// <summary>Changed size of body</summary>
        public System.Action<EntityBody, Vector2> OnResized;


        // State Control Command
        public void SetVelocity(Vector2 velocity) { m_velocity = velocity; }
        public void SetPosition(Vector2 position, bool safe = false) {
            m_rigidbody.MovePosition(position);
            m_executedSetPos = true;
            if (!safe) m_executedUnsafe = true;
        }
        [ContextMenu("UpdateSize")]
        public void UpdateSize() {
            Vector2 curSize = m_collider.bounds.size;
            if (m_size == curSize) return;
            
            m_size = curSize;
            m_executedUnsafe = true;
            OnResized?.Invoke(this, m_size);
        }
        [ContextMenu("Recovery")] public void Recovery() { m_executedUnsafe = true; }
        public void SetUseGravity(bool active) { m_useGravity = active; }
        public void SetGravityScale(float scale) { m_gravityScale = scale; }
        // Follow platform
        public void SetFollower(PlatformController followPlatform, bool followIsBlock = true) {
            m_followPlatform = followPlatform;
            m_existFollow = followPlatform != null;
            m_followIsBlock = m_existFollow && followIsBlock;
        }
        #endregion

        #region Private
        // Buffer
        private const int HIT_BUFFER_SIZE = 8;
        private const int COLLIDER_BUFFER_SIZE = 8;
        private readonly RaycastHit2D[] m_bufferHit = new RaycastHit2D[HIT_BUFFER_SIZE];
        private readonly Collider2D[] m_bufferCollider = new Collider2D[COLLIDER_BUFFER_SIZE];
        // Components
        [SerializeField] private Rigidbody2D m_rigidbody;
        [SerializeField] private BoxCollider2D m_collider;
        // State
        [SerializeField] private Vector2 m_velocity;
        [SerializeField] private Vector2 m_size;
        [SerializeField] private bool m_useGravity = true;
        [SerializeField] private float m_gravityScale = EntityConstants.DefaultGravityScale;
        [HideInInspector] private bool m_executedUnsafe;                // State flag; if flag up, should strictly check
        [HideInInspector] private bool m_executedSetPos;                // State flag; if flag up, current step execute SetPosition, otherwise apply velocity
        // Contact dynamics
        private int m_dynamicBlockNum;                                  // The number of dynamic blocks which entity touch or overlap
        [SerializeField] private PlatformController m_followPlatform;   // Current connected follower; it may not be block;
        [HideInInspector] private bool m_existFollow;                   // Whether exist follow
        [HideInInspector] private bool m_followIsBlock;                 // Whether follow platform is block


        // Activation
        private void Reset() {
            //// Setup rigidbody
            m_rigidbody = GetComponent<Rigidbody2D>();
            m_rigidbody.isKinematic = true;
            m_rigidbody.useFullKinematicContacts = true;
            m_rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            //// Setup body
            m_collider = GetComponent<BoxCollider2D>();
            UpdateSize();
        }
        private void Awake() {
            UpdateSize();
        }
        private void OnEnable() {
            m_executedUnsafe = true;    // When active, always check state
        }
        private void OnDisable() {
            m_velocity = Vector2.zero;
            m_executedUnsafe = false;
            m_executedSetPos = false;
        }

        // Physics Update
        private void FixedUpdate() {
            // Move by position
            {
                if (m_executedSetPos) {
                    m_executedSetPos = false;
                    return;
                }
            }
            // Move by velocity
            {
                float deltaTime = Time.fixedDeltaTime;
                // Initialize propertie
                Vector2 destination = m_rigidbody.position;

                if (m_useGravity) {
                    if (m_velocity.y > -EntityConstants.FallLimit) { // Apply gravity
                        m_velocity.y -= EntityConstants.Gravity * deltaTime * m_gravityScale;
                    } else { // Limit fall speed
                        m_velocity.y = -EntityConstants.FallLimit;
                    }
                }

                // Compute destination
                ReactBlock(destination, out Vector2 preDist, out Vector2 postDist, deltaTime);
                if (preDist != MoveToward(ref destination, preDist)) {
                    OnFrozen?.Invoke(this);
                }
                
                m_velocity = MoveToward(ref destination, m_velocity * deltaTime) / deltaTime;
                destination += MoveToward(ref destination, postDist);

                // Apply destination
                m_rigidbody.MovePosition(destination);
            }
        }

        // Collision Events
        private void OnCollisionEnter2D(Collision2D collision) {
            if (collision.gameObject.layer == PlatformConstants.L_DynamicBlock) ++m_dynamicBlockNum;
        }
        private void OnCollisionExit2D(Collision2D collision) {
            if (collision.gameObject.layer == PlatformConstants.L_DynamicBlock) --m_dynamicBlockNum;
        }

        private bool ReactBlock(Vector2 origin, out Vector2 preDistance, out Vector2 postDistance, float deltaTime) {
            preDistance = postDistance = Vector2.zero;

            // Fast check
            if (m_dynamicBlockNum == 0 && !m_executedUnsafe && !m_existFollow) return false;
            m_executedUnsafe = false; // Off button

            Vector2 penetration, followDistance;
            bool pushedByFollowX = false, pushedByFollowY = false;
            // Calculate penetration and push distance
            {
                // Query pushing blocks
                int numBlock = Physics2D.OverlapBoxNonAlloc(origin, m_size, 0f, m_bufferCollider, PlatformConstants.L_BlockMask);
                // Calcuate Push
                if (numBlock > 0) {
                    // Compute reaction values
                    Vector2 maxPenetration = Vector2.zero;  // Query result.1 : Recovery value to overlapped blocks

                    for (int n = 0; n != numBlock; ++n) {
                        Collider2D detectedCollider = m_bufferCollider[n];
                        ColliderDistance2D colliderDist = m_collider.Distance(detectedCollider);

                        Vector2 newPenetration = colliderDist.distance * colliderDist.normal;

                        // If gap between EC and query-target is greater than min-contact-offset, don't react
                        if (colliderDist.distance > -EntityConstants.MinContactOffset) continue;
                        // If penetrations are contrasted then freeze EC
                        if (maxPenetration.x * newPenetration.x < 0f || maxPenetration.y * newPenetration.y < 0f) {
                            OnFrozen?.Invoke(this);
                            return false;
                        }
                        // Update penetration
                        if (Mathf.Abs(maxPenetration.x) < Mathf.Abs(newPenetration.x)) {
                            maxPenetration.x = newPenetration.x;
                            pushedByFollowX = detectedCollider.gameObject.layer == PlatformConstants.L_DynamicBlock
                                && m_followIsBlock && detectedCollider.attachedRigidbody == m_followPlatform.Rigidbody;
                        }
                        if (Mathf.Abs(maxPenetration.y) < Mathf.Abs(newPenetration.y)) {
                            maxPenetration.y = newPenetration.y;

                            pushedByFollowY = detectedCollider.gameObject.layer == PlatformConstants.L_DynamicBlock
                                && m_followIsBlock && detectedCollider.attachedRigidbody == m_followPlatform.Rigidbody;
                        }
                    }
                    // Make gap between EC and collision-target
                    if (maxPenetration.x != 0f) maxPenetration.x -= (maxPenetration.x < 0f ? -1f : 1f) * (EntityConstants.MinContactOffset - EntityConstants.CastBuffer);
                    if (maxPenetration.y != 0f) maxPenetration.y -= (maxPenetration.y < 0f ? -1f : 1f) * (EntityConstants.MinContactOffset - EntityConstants.CastBuffer);

                    penetration = maxPenetration;
                } else {
                    penetration = Vector2.zero;
                }
            }

            // Calcuate Follow distance
            {
                if (m_existFollow) {
                    followDistance = m_followPlatform.Velocity * deltaTime;
                } else {
                    followDistance = Vector2.zero;
                }
            }

            // Calcuate total pre and post distance
            {
                // Set pre-distance
                preDistance = penetration;

                // Set post-distance
                postDistance.x = GetPostDistance(penetration.x, followDistance.x, pushedByFollowX);
                postDistance.y = GetPostDistance(penetration.y, followDistance.y, pushedByFollowY);
            }
            return true;
        }
        private float GetPostDistance(float pushDist, float followDist, bool pushedByFollow) {
            float postDist;
            if (!pushedByFollow && (pushDist == 0f || (pushDist * followDist > 0f && (followDist - pushDist) * followDist > 0f))) {
                postDist = followDist - pushDist;
            } else {
                postDist = 0f;
            }
            return postDist;
        }

        private Vector2 MoveToward(ref Vector2 destination, Vector2 distance) {
            if (distance == Vector2.zero) return distance;

            Vector2 befPos = destination;
            if (distance.y > 0f) {
                if (distance.y != 0f) AddDistance(ref destination, distance.y, Vector2.up);
                if (distance.x != 0f) AddDistance(ref destination, distance.x, Vector2.right);
            } else {
                if (distance.x != 0f) AddDistance(ref destination, distance.x, Vector2.right);
                if (distance.y != 0f) AddDistance(ref destination, distance.y, Vector2.up);
            }
            return destination - befPos;
        }

        private void AddDistance(ref Vector2 destination, float distance, Vector2 axis) {
            float clampedDist = (distance < 0 ? -distance : distance) + EntityConstants.MinContactOffset;
            float sign = distance < 0 ? -1f : 1f;
            // Query blocks which are obstacle for moving
            int numBlock = Physics2D.BoxCastNonAlloc(destination, m_size, 0f, axis * sign, m_bufferHit, clampedDist, PlatformConstants.L_BlockMask);
            // Clamp distance
            for (int n = 0; n != numBlock; ++n) {
                RaycastHit2D blockHit = m_bufferHit[n];
                // Update add-distance
                if (blockHit.distance < clampedDist) clampedDist = blockHit.distance;
            }
            // Add clamped distance
            (axis.x != 0f ? ref destination.x : ref destination.y) += (clampedDist - EntityConstants.MinContactOffset) * sign;
        }
        #endregion
#if UNITY_EDITOR_
        #region DEBUG
        [HideInInspector] private Vector2 editor_center;
        [HideInInspector] private Vector2 editor_size;
        [HideInInspector] private Vector2 editor_tr;
        [HideInInspector] private Vector2 editor_br;
        [HideInInspector] private Vector2 editor_tl;
        [HideInInspector] private Vector2 editor_bl;

        private void OnDrawGizmos() {
            if (Application.isEditor) {
                if (m_collider == null) m_collider = GetComponent<BoxCollider2D>();
                if (m_rigidbody == null) m_rigidbody = GetComponent<Rigidbody2D>();
            }


            
            editor_center = m_collider.bounds.center;
            editor_size = m_collider.bounds.size;
            editor_tr = editor_size * new Vector2(0.5f, 0.5f);
            editor_br = editor_size * new Vector2(0.5f, -0.5f);
            editor_tl = editor_size * new Vector2(-0.5f, 0.5f);
            editor_bl = editor_size * new Vector2(-0.5f, -0.5f);
            

            // Visualize body
            //DrawEdge(0f, Color.green);
            // Visualize max contact offset
            //DrawEdge(EntityConstants.MaxContactOffset, Color.blue);
            // Visualized min contact offset
            DrawEdge(EntityConstants.MinContactOffset, Color.red);
            // Visualized good constact offset
            //DrawEdge((EntityConstants.MaxContactOffset + EntityConstants.MinContactOffset) * 0.5f, Color.magenta);
        }
        private void DrawEdge(float edgeOffset, Color color) {
            Gizmos.color = color;
            Gizmos.DrawWireCube(editor_center, (Vector2)m_collider.bounds.size + 2 * edgeOffset * Vector2.one);
            if (edgeOffset > 0f) {
                Gizmos.DrawWireSphere(editor_center + editor_tr, edgeOffset);
                Gizmos.DrawWireSphere(editor_center + editor_br, edgeOffset);
                Gizmos.DrawWireSphere(editor_center + editor_tl, edgeOffset);
                Gizmos.DrawWireSphere(editor_center + editor_bl, edgeOffset);
            }
        }
        #endregion
#endif
    }
}
