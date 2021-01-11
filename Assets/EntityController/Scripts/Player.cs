using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Stroy {
    namespace EC {
        [RequireComponent(typeof(PlatformerController))]
        public class Player : MonoBehaviour {
            #region Public
            // Input Binding
            public void OnMove(InputAction.CallbackContext context) {
                m_inputAxis = context.ReadValue<Vector2>();
            }
            public void OnJump(InputAction.CallbackContext context) {
                switch (context.phase) {
                case InputActionPhase.Performed:
                    if (m_enableWallJump) {
                        Vector2 velocity = new Vector2(m_wallJumpVelocity.x * m_inputAxis.x, m_wallJumpVelocity.y);
                        m_platformer.Jump(velocity);
                        return;
                    }

                    if (m_platformer.IsGround || m_leftJumpCount > 0) {
                        m_platformer.Jump(m_jumpVelocity * Vector2.up);
                        --m_leftJumpCount;
                        m_jumpDelay = false;
                    } else {
                        m_jumpDelay = true;
                        m_elapsedJumpDelay = 0f;
                    }
                    break;
                case InputActionPhase.Canceled:
                    m_platformer.PauseJump(m_jumpPauseSacler);
                    m_jumpDelay = false;
                    break;
                }
            }
            public void OnDash(InputAction.CallbackContext context) {
                if (context.performed) {
                    if (m_isDash || m_leftDashCount <= 0) return;
                    StartCoroutine(Dash(m_inputAxis.normalized));
                    --m_leftDashCount;
                }
            }
            public void OnAttack(InputAction.CallbackContext context) {

            }

            // State command
            public void SetJump(float maxHeight, float minHeight) {
                m_jumpVelocity = Mathf.Sqrt(maxHeight * 2f * ECConstants.Gravity);
                m_jumpPauseSacler = maxHeight / minHeight;

                m_maxJumpHeight = maxHeight;
                m_minJumpHeight = minHeight;
            }
            public void SetDash(float distance, float speed) {
                m_dashDistance = distance;
                m_dashSpeed = speed;
                m_dashTime = distance / speed;
            }
            #endregion

            #region Private
            private const float JUMP_DELAY_LIMIT = 0.1f;

            // Component
            [HideInInspector] private PlatformerController m_platformer;
            // Run
            [SerializeField] private float m_topSpeed;
            [SerializeField] private float m_acc;
            [SerializeField] private float m_dec;
            [SerializeField] private float m_turnDec;
            private float m_currentSpeed;
            // Jump
            [SerializeField] private float m_maxJumpHeight;
            [SerializeField] private float m_minJumpHeight;
            [SerializeField] private int m_maxJumpCount;
            private int m_leftJumpCount;
            private bool m_jumpDelay;
            private float m_elapsedJumpDelay;
            [HideInInspector] private float m_jumpVelocity;
            [HideInInspector] private float m_jumpPauseSacler;
            // Dash
            [SerializeField] private float m_dashSpeed;
            [SerializeField] private float m_dashDistance;
            [SerializeField] private int m_maxDashCount;
            private int m_leftDashCount;
            [HideInInspector] private float m_dashTime;
            private bool m_isDash;
            // Wall Jump
            [SerializeField] private Vector2 m_wallJumpVelocity;
            private bool m_enableWallJump;
            // Input
            private Vector2 m_inputAxis;


            private void Awake() {
                m_platformer = GetComponent<PlatformerController>();

                SetJump(m_maxJumpHeight, m_minJumpHeight);
                SetDash(m_dashDistance, m_dashSpeed);

                ResetDashCount();
                ResetJumpCount();

                m_platformer.OnGround += ResetJumpCount;
                m_platformer.OnGround += ResetDashCount;
            }
            private void Update() {
                if (m_isDash) return;

                
                if (m_jumpDelay) {
                    m_elapsedJumpDelay += Time.deltaTime;
                    // Delay time over
                    if (m_elapsedJumpDelay > JUMP_DELAY_LIMIT) {
                        m_jumpDelay = false; 
                    }
                    
                    if (m_platformer.IsGround) {
                        m_jumpDelay = false;
                        m_platformer.Jump(m_jumpVelocity * Vector2.up);
                        --m_leftJumpCount;
                    }
                }

                Move(m_inputAxis.x);

                Vector2 origin = m_platformer.Controller.Position;
                if (!m_platformer.IsGround) {
                    RaycastHit2D hitWall = Physics2D.BoxCast(origin, m_platformer.Controller.Size, 0f, m_inputAxis.x * Vector2.right, ECConstants.MaxContactOffset, ECConstants.BlockMask);
                    if (hitWall) {
                        float angle = Vector2.Angle(Vector2.up, hitWall.normal);
                        if (89f < angle && angle < 91f) {
                            m_enableWallJump = true;
                            return;
                        }
                    }
                }
                m_enableWallJump = false;
            }

            private void Move(float inputDir) {
                float magnitude = m_currentSpeed < 0f ? -m_currentSpeed : m_currentSpeed;
                float dir = m_currentSpeed < 0f ? -1f : 1f;

                if (inputDir != 0f) {
                    if (inputDir * m_currentSpeed < 0f) { // Turn
                        magnitude -= m_turnDec * Time.deltaTime;
                        if (magnitude < 0f) magnitude = 0f;
                        m_currentSpeed = magnitude * dir;
                    } else { // Run
                        if(magnitude < m_topSpeed) { // Accelerate
                            magnitude += m_acc * Time.deltaTime;
                            if (magnitude > m_topSpeed) magnitude = m_topSpeed;
                            m_currentSpeed = magnitude * inputDir;
                        }
                    }
                } else { // Break
                    if (magnitude != 0f) {
                        magnitude -= m_dec * Time.deltaTime;
                        if(magnitude < 0f) magnitude = 0f;
                        m_currentSpeed = magnitude * dir;
                    }
                }

                m_platformer.Move(m_currentSpeed);
            }


            private void ResetJumpCount() { m_leftJumpCount = m_maxJumpCount; }
            private void ResetDashCount() { m_leftDashCount = m_maxDashCount; }

            private IEnumerator Dash(Vector2 direction) {
                m_isDash = true;

                Vector2 velocity = m_dashSpeed * direction;
                float elapsedTime = 0f;
                m_dashTime = m_dashDistance / m_dashSpeed;

                if (m_platformer.IsGround) {
                    while (elapsedTime < m_dashTime) {
                        elapsedTime += Time.deltaTime;
                        m_platformer.Move(velocity.x);
                        yield return null;
                    }
                    ResetDashCount();
                } else {
                    m_platformer.enabled = false;
                    EntityController controller = m_platformer.Controller;
                    while (elapsedTime < m_dashTime) {
                        elapsedTime += Time.deltaTime;
                        controller.SetVelocity(in velocity);
                        yield return null;
                    }
                    m_platformer.enabled = true;
                }
                
                m_isDash = false;
                yield break;
            }
            #endregion
        }
    }
}
