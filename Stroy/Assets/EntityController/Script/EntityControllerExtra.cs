using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Stroy {

    namespace EC {
        [RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
        public class EntityControllerExtra : EntityController {
            public bool UseGravity { get => m_useGravity; set => m_useGravity = value; }
            public bool IsGround => m_isGround;
            public float ContactAngle => m_contactAngle;

            //==========================================================================================

            // State
            [SerializeField] private bool m_useGravity;
            private bool m_isGround;
            private float m_contactAngle;


            protected override  void OnDisable() {
                base.OnDisable();
                m_isGround = false;
                m_contactAngle = 0f;
            }

            protected override void FixedUpdate() {
                if (GetSkipFlag()) return;

                // Initialize properties
                Vector2 origin = Rigidbody.position;
                Vector2 destination = origin;
                Vector2 followDistance = Vector2.zero;

                // Compute destination
                // Compute reactions
                if (m_useGravity) {
                    bool wasGround = m_isGround;
                    followDistance = CheckGround(in origin);

                    if (m_isGround && !wasGround) { // Landing
                        m_velocity.y = 0;
                    } else if (!m_isGround) { // Airbone
                        m_velocity.y -= ECConstants.Gravity * Time.fixedDeltaTime;
                    }
                }
                ReactBlock(in origin, out Vector2 pushDistance, out Vector2 penetration);
                // Add pre-reaction
                destination += penetration;
                // Add velocity
                ApplyVelocity(ref destination);
                // Add post-reactions
                destination.x += pushDistance.x * followDistance.x < 0f ? pushDistance.x : pushDistance.x + followDistance.x;
                destination.y += pushDistance.y * followDistance.y < 0f ? pushDistance.y : pushDistance.y + followDistance.y;

                // Apply destination
                Rigidbody.MovePosition(destination);
            }

            private Vector2 CheckGround(in Vector2 origin) {
                Vector2 followDistance = Vector2.zero;
                // Query the closest ground
                RaycastHit2D hitBlock = Physics2D.BoxCast(origin, Size, 0f, Vector2.down, ECConstants.MaxContactOffset, ECConstants.BlockMask);
                if (hitBlock) {
                    // Check degree of ground
                    float angle = Vector2.SignedAngle(Vector2.up, hitBlock.normal);
                    if (-ECConstants.SlopeLimit < angle && angle < ECConstants.SlopeLimit) {
                        m_isGround = true;
                        m_contactAngle = angle;
                        // [Dynamic]
                        // Follow dynamic block current boarding
                        if (hitBlock.collider.gameObject.layer == ECConstants.DynamicBlock) {
                            followDistance.x = hitBlock.rigidbody.velocity.x;
                            if (hitBlock.rigidbody.velocity.y < 0f) {
                                followDistance.y = hitBlock.rigidbody.velocity.y;
                            }
                            followDistance *= Time.fixedDeltaTime;
                        }
                        return followDistance;
                    }
                }
                // No ground
                m_isGround = false;
                m_contactAngle = 0f;
                return followDistance;
            }
        }
    }
}
