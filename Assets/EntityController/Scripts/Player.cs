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
                    if(!m_platformer.IsGround && m_platformer.IsWall) {
                        WallJump();
                    } else {
                        Jump();
                    }
                    break;
                case InputActionPhase.Canceled:
                    BreakJump();
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
                m_jumpPauseSacle = maxHeight / minHeight;

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
            private const float DEFAULT_FRICTION = 1f;

            // Component
            [HideInInspector] private PlatformerController m_platformer;
            // Run
            [SerializeField] private float m_topSpeed;
            [SerializeField] private float m_acc;
            [SerializeField] private float m_dec;
            [SerializeField] private float m_turnDec;
            [SerializeField] private float m_firction = DEFAULT_FRICTION;
            private float m_currentSpeed;
            // Jump
            [SerializeField] private float m_maxJumpHeight;
            [SerializeField] private float m_minJumpHeight;
            [HideInInspector] private float m_jumpVelocity;
            [HideInInspector] private float m_jumpPauseSacle;
            [SerializeField] private int m_maxMorerJumpCount;
            private int m_leftMorerJumpCount;
            private bool m_jumpDelay;
            private float m_elapsedJumpDelay;
            // Dash
            [SerializeField] private float m_dashSpeed;
            [SerializeField] private float m_dashDistance;
            [SerializeField] private int m_maxDashCount;
            private int m_leftDashCount;
            [HideInInspector] private float m_dashTime;
            private bool m_isDash;
            // Wall Jump
            [SerializeField] private Vector2 m_wallJumpVelocity;
            private bool m_isWallJump;
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

                if (m_isWallJump) {
                    if(!m_platformer.IsGround && m_platformer.Velocity .y > 0f) {
                        return;
                    }
                    m_isWallJump = false;
                }
                
                if (m_jumpDelay) {
                    m_elapsedJumpDelay += Time.deltaTime;
                    // Delay time over
                    if (m_elapsedJumpDelay > JUMP_DELAY_LIMIT) {
                        m_jumpDelay = false; 
                    }
                    Jump();
                }

                if (m_platformer.IsGround) {
                    if (m_platformer.ContactGround.CompareTag("Ice")) {
                        m_firction = DEFAULT_FRICTION * 0.1f;
                    } else {
                        m_firction = DEFAULT_FRICTION;
                    }
                }

                Move(m_inputAxis.x);
            }

            private void Move(float inputDir) {
                float magnitude = m_currentSpeed < 0f ? -m_currentSpeed : m_currentSpeed;
                float dir = m_currentSpeed < 0f ? -1f : 1f;

                if (inputDir != 0f) {
                    if (inputDir * m_currentSpeed < 0f) { // Turn
                        magnitude -= m_turnDec * Time.deltaTime * m_firction;
                        if (magnitude < 0f) magnitude = 0f;
                        m_currentSpeed = magnitude * dir;
                    } else { // Run
                        if(magnitude < m_topSpeed) { // Accelerate
                            magnitude += m_acc * Time.deltaTime * m_firction;
                            if (magnitude > m_topSpeed) magnitude = m_topSpeed;
                            m_currentSpeed = magnitude * inputDir;
                        } else { // Limit speed
                            m_currentSpeed = m_topSpeed * dir;
                        }
                    }
                } else { // Break
                    if (magnitude != 0f) {
                        magnitude -= m_dec * Time.deltaTime * m_firction;
                        if(magnitude < 0f) magnitude = 0f;
                        m_currentSpeed = magnitude * dir;
                    }
                }

                m_platformer.Move(m_currentSpeed);
            }

            private bool Jump() {
                if (m_platformer.IsGround) { // Ground jump
                    m_platformer.Jump(m_jumpVelocity);
                    m_jumpDelay = false;
                    return true;
                } else if (m_leftMorerJumpCount > 0) { // Morer jump
                    m_platformer.Jump(m_jumpVelocity);
                    --m_leftMorerJumpCount;
                    m_jumpDelay = false;
                    return true;
                } else { // Delay
                    m_jumpDelay = true;
                    m_elapsedJumpDelay = 0f;
                    return false;
                }
            }
            private void BreakJump() {
                m_platformer.BreakJump(m_jumpPauseSacle);
                m_jumpDelay = false;
            }
            private void ResetJumpCount() { m_leftMorerJumpCount = m_maxMorerJumpCount; }
            private void SetJumpCount(int count) { m_leftMorerJumpCount = Mathf.Clamp(count, 0, m_maxMorerJumpCount); }


            private void WallJump() {
                m_isWallJump = true;
                
                m_platformer.Jump(m_wallJumpVelocity.y);
                m_currentSpeed = m_wallJumpVelocity.x * (m_platformer.WallOnLeft ? -1f : 1f);
                m_platformer.Move(m_currentSpeed);
            }


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
                        controller.SetVelocity(velocity);
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
