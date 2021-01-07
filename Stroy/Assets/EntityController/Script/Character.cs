using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Stroy {
    
    namespace EC {
        [RequireComponent(typeof(EntityController))]
        public class Character : MonoBehaviour {
            public EntityController Controller => m_controller;



            // State command
            public void SetJumpHeight(float maxHeight, float minHeight) {
                m_jumpVelocity = Mathf.Sqrt(maxHeight * 2f * ECConstants.Gravity);
                m_jumpPauseSacler = maxHeight / minHeight;
                m_jumpPauseSacler = maxHeight / minHeight;

                m_maxHeight = maxHeight;
                m_minHeight = minHeight;
            }
            // Action Command

            // Input Binding
            public void OnMove(InputAction.CallbackContext ctx) {
                m_moveDir = ctx.ReadValue<float>();
            }
            public void OnJump(InputAction.CallbackContext ctx) {
                switch (ctx.phase) {
                case InputActionPhase.Performed:
                    if (m_isGround) {
                        m_velocity.y = m_jumpVelocity;
                        m_isJump = true;
                    }
                    break;
                case InputActionPhase.Canceled:
                    if (m_isJump) {
                        m_gravityScaler = m_jumpPauseSacler;
                    }
                    break;
                }
            }

            // ====================================================
            
            private const float DEFAULT_GRAVITY = 1f;
            private const float FALL_GRAVITY = 1.5f;

            // Component
            [HideInInspector] private EntityController m_controller;
            // State
            [SerializeField] private float m_speed;
            [SerializeField] private float m_maxHeight;
            [SerializeField] private float m_minHeight;
            [HideInInspector] private float m_jumpVelocity;
            [HideInInspector] private float m_jumpPauseSacler;

            private Vector2 m_velocity;
            private bool m_isGround;
            private float m_contactAngle;
            private bool m_isJump;
            private float m_gravityScaler = DEFAULT_GRAVITY;
            [HideInInspector] private Vector2 m_prevPos;

            // Cache
            private static readonly Vector2 RIGIT_SIDE = new Vector2(1f, -1f).normalized;
            private static readonly Vector2 LEFT_SIDE = new Vector2(-1f, -1f).normalized;
            private static readonly Vector2 BOTTOM_RIGHT = new Vector2(0.5f, -0.5f);
            private static readonly Vector2 BOTTOM_LEFT = new Vector2(-0.5f, -0.5f);

            // Input
            private float m_moveDir;


            private void Awake() {
                m_controller = GetComponent<EntityController>();
                m_controller.SetFollowDistanceGenerator(GetFollowDistance);
                SetJumpHeight(m_maxHeight, m_minHeight);
            }
            private void Update() {
                Move();
            }

            private void Move() {
                Vector2 origin = m_controller.Rigidbody.position;

                bool wasGround = m_isGround;
                CheckGround(in origin);

                // Revise conner bouncing when character wanted moving on ground at previouse step
                if (!m_isGround && wasGround && m_prevPos != origin && !m_isJump) {
                    RaycastHit2D hitBlock = Physics2D.BoxCast(origin, m_controller.Size, 0f, Vector2.down, Mathf.Infinity, 0x01 << ECConstants.StaticBlock);
                    if (TryGetGroundAngle(in hitBlock, out float angle)) {
                        if (m_velocity.y > 0f && hitBlock.distance <= origin.y - m_prevPos.y + ECConstants.MaxContactOffset
                            || m_velocity.y <= 0f && hitBlock.distance <= Mathf.Tan(angle * Mathf.Deg2Rad) * (m_prevPos.x - origin.x) + ECConstants.MaxContactOffset
                            ) {
                            // On dynamic block, don't support RCB
                            m_controller.SetPosition(origin + Vector2.down * (hitBlock.distance - ECConstants.MinContactOffset));
                            m_contactAngle = angle;
                            m_isGround = true;
                            m_prevPos = origin;
                            return;
                        }
                    }
                }

                if (m_isGround && !wasGround) { // Landing
                    m_velocity = Vector2.zero;
                    m_gravityScaler = DEFAULT_GRAVITY;
                } else if (!m_isGround || m_isJump) { // Airbone
                    if (m_isJump) { // Jumping
                        // Check touch to cell
                        RaycastHit2D hitCell = Physics2D.BoxCast(origin, m_controller.Size, 0f, Vector2.up, ECConstants.MaxContactOffset, ECConstants.BlockMask);
                        if (hitCell) {
                            m_velocity.y = 0f;
                        }
                        if(m_velocity.y <= 0f) {
                            m_isJump = false;
                        }
                    }
                    if(m_velocity.y < 0f) { // Falling
                        m_gravityScaler = FALL_GRAVITY;
                    }
                    // Apply gravity
                    if(m_velocity.y > ECConstants.FallLimit) {
                        m_velocity.y -= ECConstants.Gravity * Time.deltaTime * m_gravityScaler;
                    } else {
                        m_velocity.y = ECConstants.FallLimit;
                    }
                    
                    m_velocity.x = m_moveDir * m_speed;
                } else { // Walking or Standing
                    if (m_contactAngle != 0f) {
                        m_velocity = m_moveDir * m_speed * new Vector2(Mathf.Cos(m_contactAngle * Mathf.Deg2Rad), Mathf.Sin(m_contactAngle * Mathf.Deg2Rad));
                    } else {
                        m_velocity = m_moveDir * m_speed * Vector2.right;
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
                    if(m_moveDir < 0f) {
                        hitBlock = Physics2D.Raycast(origin + size * BOTTOM_LEFT, LEFT_SIDE, ECConstants.MaxContactOffset, ECConstants.BlockMask);
                        if (TryUpdateGroundInfo(in hitBlock)) return;

                        hitBlock = Physics2D.Raycast(origin + size * BOTTOM_RIGHT, RIGIT_SIDE, ECConstants.MaxContactOffset, ECConstants.BlockMask);
                        if (TryUpdateGroundInfo(in hitBlock)) return;
                    } else if(m_moveDir > 0f){                        
                        hitBlock = Physics2D.Raycast(origin + size * BOTTOM_RIGHT, RIGIT_SIDE, ECConstants.MaxContactOffset, ECConstants.BlockMask);
                        if (TryUpdateGroundInfo(in hitBlock)) return;

                        hitBlock = Physics2D.Raycast(origin + size * BOTTOM_LEFT, LEFT_SIDE, ECConstants.MaxContactOffset, ECConstants.BlockMask);
                        if (TryUpdateGroundInfo(in hitBlock)) return;
                    }
                    
                    hitBlock = Physics2D.BoxCast(origin, size, 0f, Vector2.down, ECConstants.MaxContactOffset, ECConstants.BlockMask);
                    if (TryUpdateGroundInfo(in hitBlock)) return;
                }

                // No ground
                m_isGround = false;
                m_contactAngle = 0f;
                m_controller.SetFollowBlock(null);
            }
            private bool TryUpdateGroundInfo(in RaycastHit2D hitBlock) {
                if (hitBlock) {
                    float angle = Vector2.SignedAngle(Vector2.up, hitBlock.normal);
                    if (-ECConstants.SlopeLimit < angle && angle < ECConstants.SlopeLimit) {
                        m_isGround = true;
                        m_contactAngle = angle;

                        if(hitBlock.collider.gameObject.layer == ECConstants.DynamicBlock) {
                            m_controller.SetFollowBlock(hitBlock.rigidbody);
                        }
                        return true;
                    }
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
            private static Vector2 GetFollowDistance(Rigidbody2D followBlock) {
                Vector2 followDistance = Vector2.zero;
                followDistance.x = followBlock.velocity.x * Time.fixedDeltaTime;
                if (followBlock.velocity.y < 0f) {
                    followDistance.y = followBlock.velocity.y * Time.fixedDeltaTime;
                }
                return followDistance;
            }
        }
    }
}
