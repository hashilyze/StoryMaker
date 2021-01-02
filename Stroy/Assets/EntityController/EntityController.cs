using UnityEngine;

namespace Stroy {

    namespace EC {
        [RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
        public class EntityController : MonoBehaviour {
            public Rigidbody2D AttachedRigidbody => m_rigidbody;
            public BoxCollider2D Body => m_body;

            public bool Enabled => m_enabled;
            public Vector2 Size => m_size;
            public Vector2 Velocity => m_velocity;


            // Activation: Enable / Disable
            public void Enable(bool safe = false) {
                if (m_enabled) return;

                m_enabled = true;
                if (safe == false) m_executedUnsafe = true;
            }
            public void Disable() {
                if (!m_enabled) return;

                m_enabled = false;
                m_velocity = Vector2.zero;
                m_executedUnsafe = false;
            }
            // Base Action (Command)
            public void SetVelocity(in Vector2 velocity) {
                m_velocity = velocity;
            }
            public void SetPosition(in Vector2 position, bool safe = false) {
                m_rigidbody.MovePosition(position);
                m_isTeleport = true;
                if (safe == false) {
                    m_executedUnsafe = true;
                }
            }
            public void SetSize(in Vector2 size, bool safe = false) {
                m_body.size = size;
                m_size = size;

                if (safe == false) {
                    m_executedUnsafe = true;
                }
            }
            //==========================================================================================


            // Buffer
            private const int HIT_BUFFER_SIZE = 8;
            private const int COLLIDER_BUFFER_SIZE = 8;
            private readonly RaycastHit2D[] m_bufferHit = new RaycastHit2D[HIT_BUFFER_SIZE];
            private readonly Collider2D[] m_bufferCollider = new Collider2D[COLLIDER_BUFFER_SIZE];
            // Cached Component
            private Rigidbody2D m_rigidbody;            // Cached rigidbody component
            private BoxCollider2D m_body;               // Cached body-collider component
            // State
            private bool m_enabled;                     // Activation of component
            private Vector2 m_size;                     // Size of body
            private Vector2 m_velocity;                 // Current velocity
            private int m_dynamicNum;                   // The number of dynamic blocks which entity touch or overlap
            private bool m_executedUnsafe;              // Whether has been executed unsafe command between current and previous steps
            private bool m_isTeleport;

            private void Awake() {
                // Setup body
                m_body = GetComponent<BoxCollider2D>();
                m_size = m_body.size;
                // Setup rigidbody
                m_rigidbody = GetComponent<Rigidbody2D>();
                m_rigidbody.isKinematic = true;
                m_rigidbody.useFullKinematicContacts = true;
                m_rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
            private void FixedUpdate() {
                if (m_enabled == false) return;

                // Don't apply velocity to movement when teleport
                if (m_isTeleport) {
                    m_isTeleport = false;
                    return;
                }

                // Initialize properties
                Vector2 origin = m_rigidbody.position;
                Vector2 destination = origin;
                // Compute destination
                if (ReactBlock(in origin, out Vector2 pushDistance, out Vector2 penetration)) {
                    destination += penetration;
                    ApplyVelocity(ref destination);
                    destination += pushDistance;
                } else {
                    ApplyVelocity(ref destination);
                }
                // Apply destination
                m_rigidbody.MovePosition(destination);
            }

            private void OnCollisionEnter2D(Collision2D collision) {
                if (collision.gameObject.layer == ECConstant.DynamicBlock) {
                    ++m_dynamicNum;
                }
            }
            private void OnCollisionExit2D(Collision2D collision) {
                if (collision.gameObject.layer == ECConstant.DynamicBlock) {
                    --m_dynamicNum;
                }
            }

            /// <summary>Compute reaction value to blocks</summary>
            private bool ReactBlock(in Vector2 origin, out Vector2 pushDistance, out Vector2 penetration) {
                pushDistance = penetration = Vector2.zero;

                // Fast check
                if (m_dynamicNum == 0 && m_executedUnsafe == false) return false;
                m_executedUnsafe = false;

                // Query blocks required reaction
                int numBlock = Physics2D.OverlapBoxNonAlloc(origin, m_size, 0f, m_bufferCollider, ECConstant.BlockMask);

                // No blocks, No react
                if (numBlock == 0) return false;

                // Compute reaction values
                Vector2 maxPenetration = Vector2.zero;  // Query result.1 : Recovery value to overlapped blocks
                Vector2 maxPushVelocity = Vector2.zero; // Query result.2 : Estimated penetration by dynamic blocks at next step

                for (int n = 0; n != numBlock; ++n) {
                    Collider2D detectedCollider = m_bufferCollider[n];
                    ColliderDistance2D coliiderDist = m_body.Distance(detectedCollider);

                    Vector2 newPenetration = coliiderDist.distance * coliiderDist.normal;

                    // Query push-distance only dynamic-block
                    if (detectedCollider.gameObject.layer == ECConstant.DynamicBlock) {
                        Vector2 newPushVelocity = detectedCollider.attachedRigidbody.velocity;
                        if (Vector2.Dot(newPushVelocity, newPenetration) > 0f) { // Query only forward objects
                            // Update push-velocity
                            if (coliiderDist.normal.x != 0f && Mathf.Abs(maxPushVelocity.x) < Mathf.Abs(newPushVelocity.x)) maxPushVelocity.x = newPushVelocity.x;
                            if (coliiderDist.normal.y != 0f && Mathf.Abs(maxPushVelocity.y) < Mathf.Abs(newPushVelocity.y)) maxPushVelocity.y = newPushVelocity.y;
                        }
                    }

                    // No require to update peneration unless overlapped
                    if (coliiderDist.distance > -ECConstant.MinContactOffset) continue;
                    // If penerations are contrasted, freezing character (Can't recovery)
                    if (maxPenetration.x * newPenetration.x < 0 || maxPenetration.y * newPenetration.y < 0f) return false;

                    // Update penetration
                    if (Mathf.Abs(maxPenetration.x) < Mathf.Abs(newPenetration.x)) maxPenetration.x = newPenetration.x;
                    if (Mathf.Abs(maxPenetration.y) < Mathf.Abs(newPenetration.y)) maxPenetration.y = newPenetration.y;
                }
                // Interpolate contact-offset
                if (maxPenetration.x != 0f) maxPenetration.x -= Mathf.Sign(maxPenetration.x) * (ECConstant.MinContactOffset - 0.005f);
                if (maxPenetration.y != 0f) maxPenetration.y -= Mathf.Sign(maxPenetration.y) * (ECConstant.MinContactOffset - 0.005f);

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
                m_velocity = (destination - befPos) / Time.fixedDeltaTime;
            }
            /// <summary>Add distance to lastest destination</summary>
            private void AddDistance(ref Vector2 destination, float distance, in Vector2 axis) {
                float clampedDist = (distance < 0 ? -distance : distance) + ECConstant.MinContactOffset;
                float sign = distance < 0 ? -1f : 1f;

                // Query blocks which are obstacle for moving
                int numBlock = Physics2D.BoxCastNonAlloc(destination, m_size, 0f, axis * sign, m_bufferHit, clampedDist, ECConstant.BlockMask);
                // Clamp distance
                for (int n = 0; n != numBlock; ++n) {
                    RaycastHit2D blockHit = m_bufferHit[n];

                    // Adjust blockHit.distance
                    if (blockHit.collider.gameObject.layer == ECConstant.DynamicBlock) {
                        float dynamicVelocity = axis.x != 0f ? blockHit.rigidbody.velocity.x : blockHit.rigidbody.velocity.y;
                        if (sign * dynamicVelocity > 0f) {
                            blockHit.distance += (dynamicVelocity < 0 ? -dynamicVelocity : dynamicVelocity) * Time.fixedDeltaTime;
                        }
                    }

                    // Update add-distance
                    if (blockHit.distance < clampedDist) clampedDist = blockHit.distance;
                }
                // Add clamped distance
                (axis.x != 0f ? ref destination.x : ref destination.y) += (clampedDist - ECConstant.MinContactOffset) * sign;
            }

            private void OnDrawGizmos() {
                return;

#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                if (Application.isEditor) {
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
                    if (m_body == null) m_body = GetComponent<BoxCollider2D>();
                    if (m_rigidbody == null) m_rigidbody = GetComponent<Rigidbody2D>();
                }

                // Visualize body
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(AttachedRigidbody.position, m_body.bounds.size);
                // Visualize max contact offset
                Gizmos.color = Color.blue;
                DrawBox(ECConstant.MaxContactOffset, Color.green);
                // Visualized min contact offset
                DrawBox(ECConstant.MinContactOffset, Color.red);
                // Visualized good constact offset
                DrawBox((ECConstant.MaxContactOffset + ECConstant.MinContactOffset) * 0.5f, Color.magenta);
            }
            private void DrawBox(float offset, Color color) {
                Vector2 tr = m_body.size * new Vector2(0.5f, 0.5f);
                Vector2 br = m_body.size * new Vector2(0.5f, -0.5f);
                Vector2 tl = m_body.size * new Vector2(-0.5f, 0.5f);
                Vector2 bl = m_body.size * new Vector2(-0.5f, -0.5f);

                Gizmos.color = color;
                Gizmos.DrawWireCube(AttachedRigidbody.position, (Vector2)m_body.bounds.size + 2 * offset * Vector2.one);

                Gizmos.DrawWireSphere(AttachedRigidbody.position + tr, offset);
                Gizmos.DrawWireSphere(AttachedRigidbody.position + br, offset);
                Gizmos.DrawWireSphere(AttachedRigidbody.position + tl, offset);
                Gizmos.DrawWireSphere(AttachedRigidbody.position + bl, offset);
            }
        }
    }
}
