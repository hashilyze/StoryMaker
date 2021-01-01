using System.Collections;
using System.Collections.Generic;
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
            public void Enable() {
                if (m_enabled) return;
                m_enabled = true;
            }
            public void Disable() {
                if (!m_enabled) return;
                m_enabled = false;
            }
            // Base Action (Command)
            public void SetVelocity(in Vector2 velocity) {
                m_velocity = velocity;
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
            private bool m_enabled = true;
            private Vector2 m_size;
            private Vector2 m_velocity;


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

                // Initialize properties
                Vector2 origin = m_rigidbody.position;
                Vector2 destination = origin;
                // Compute destination
                if(ReactBlock(in origin, out Vector2 pushDistance, out Vector2 penetration)) {
                    destination += penetration;
                    AddVelocity(ref destination, in m_velocity);
                    destination += pushDistance;
                } else {
                    AddVelocity(ref destination, in m_velocity);
                }
                // Apply destination
                m_velocity = (destination - origin) / Time.fixedDeltaTime;
                m_rigidbody.MovePosition(destination);
            }


            /// <summary>Compute reaction value to blocks</summary>
            private bool ReactBlock(in Vector2 origin, out Vector2 pushDistance, out Vector2 penetration) {
                pushDistance = penetration = Vector2.zero;

                // Global search for reaction
                int numBlock = Physics2D.OverlapBoxNonAlloc(origin, m_size, 0f, m_bufferCollider, ECConstant.BlockMask);

                // No react
                if (numBlock == 0) return false;

                // Compute reaction values
                Vector2 maxPushVelocity = Vector2.zero; // Query result.1
                Vector2 maxPenetration = Vector2.zero;  // Query result.2

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
            /// <summary>Add velocity to lastest destination</summary>
            private void AddVelocity(ref Vector2 destination, in Vector2 velocity) {
                Vector2 distance = velocity * Time.fixedDeltaTime;

                if (distance.y > 0f) {
                    if (distance.y != 0f) AddDistance(ref destination, distance.y, Vector2.up);
                    if (distance.x != 0f) AddDistance(ref destination, distance.x, Vector2.right);
                } else {
                    if (distance.x != 0f) AddDistance(ref destination, distance.x, Vector2.right);
                    if (distance.y != 0f) AddDistance(ref destination, distance.y, Vector2.up);
                }
            }
            /// <summary>Add distance to lastest destination</summary>
            private void AddDistance(ref Vector2 destination, float distance, in Vector2 axis) {
                float clampedDist = Mathf.Abs(distance) + ECConstant.MinContactOffset;
                float sign = Mathf.Sign(distance);

                // Query blocks which are obstacle for moving
                int numBlock = Physics2D.BoxCastNonAlloc(destination, m_size, 0f, axis * sign, m_bufferHit, clampedDist, ECConstant.BlockMask);
                // Clamp distance
                for (int n = 0; n != numBlock; ++n) {
                    RaycastHit2D blockHit = m_bufferHit[n];
                    // Adjust blockHit.distance
                    if (blockHit.collider.gameObject.layer == ECConstant.DynamicBlock) {
                        float dynamicVelocity = axis.x != 0f ? blockHit.rigidbody.velocity.x : blockHit.rigidbody.velocity.y;
                        if (sign * dynamicVelocity > 0f) {
                            blockHit.distance += Mathf.Abs(dynamicVelocity) * Time.fixedDeltaTime;
                        }
                    }
                    // Update add-distance
                    if (blockHit.distance < clampedDist) clampedDist = blockHit.distance;
                }
                // Add clamped distance
                (axis.x != 0f ? ref destination.x : ref destination.y) += (clampedDist - ECConstant.MinContactOffset) * sign;
            }
        }
    }
}
