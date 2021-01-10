using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Stroy {
    namespace EC {
        [RequireComponent(typeof(Character))]
        public class Player : MonoBehaviour {
            // Input Binding
            public void OnMove(InputAction.CallbackContext context) {
                m_inputAxis = context.ReadValue<Vector2>();
            }
            public void OnJump(InputAction.CallbackContext context) {
                switch (context.phase) {
                case InputActionPhase.Performed:
                    if (m_enableWallJump) {
                        Vector2 velocity = new Vector2(m_wallJumpVelocity.x * m_inputAxis.x, m_wallJumpVelocity.y);
                        m_character.Jump(velocity);
                        return;
                    }
                    
                    if (m_character.IsGround) {
                        m_character.Jump(m_jumpVelocity * Vector2.up);
                    } else if (m_leftJumpCount > 0) { // Morer Jumping
                        m_character.Jump(m_jumpVelocity * Vector2.up);
                    }
                    --m_leftJumpCount;
                    break;
                case InputActionPhase.Canceled:
                    m_character.PauseJump(m_jumpPauseSacler);
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
                m_jumpPauseSacler = maxHeight / minHeight;

                m_maxJumpHeight = maxHeight;
                m_minJumpHeight = minHeight;
            }
            public void SetDash(float distance, float speed) {
                m_dashDistance = distance;
                m_dashSpeed = speed;
                m_dashTime = distance / speed;
            }
            // -----------------------------------------------------------------------------

            // Component
            [HideInInspector] private Character m_character;
            // State
            // Run
            [SerializeField] private float m_speed;
            // Jump
            [SerializeField] private float m_maxJumpHeight;
            [SerializeField] private float m_minJumpHeight;
            [SerializeField] private int m_maxJumpCount;
            private int m_leftJumpCount;
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
                m_character = GetComponent<Character>();

                SetJump(m_maxJumpHeight, m_minJumpHeight);
                SetDash(m_dashDistance, m_dashSpeed);

                ResetDashCount();
                ResetJumpCount();

                m_character.OnGround += ResetJumpCount;
                m_character.OnGround += ResetDashCount;
            }
            private void Update() {
                if (m_isDash) return;
                
                m_character.Move(m_inputAxis.x * m_speed);

                Vector2 origin = m_character.Controller.Position;
                if (!m_character.IsGround) {
                    RaycastHit2D hitWall = Physics2D.BoxCast(origin, m_character.Controller.Size, 0f, m_inputAxis.x * Vector2.right, ECConstants.MaxContactOffset, ECConstants.BlockMask);
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


            private void ResetJumpCount() { m_leftJumpCount = m_maxJumpCount; }
            private void ResetDashCount() { m_leftDashCount = m_maxDashCount; }

            private IEnumerator Dash(Vector2 direction) {
                m_isDash = true;

                Vector2 velocity = m_dashSpeed * direction;
                float elapsedTime = 0f;
                m_dashTime = m_dashDistance / m_dashSpeed;

                if (m_character.IsGround) {
                    while (elapsedTime < m_dashTime) {
                        elapsedTime += Time.deltaTime;
                        m_character.Move(velocity.x);
                        yield return null;
                    }
                    ResetDashCount();
                } else {
                    m_character.EnabledMovement = false;
                    EntityController controller = m_character.Controller;
                    while (elapsedTime < m_dashTime) {
                        elapsedTime += Time.deltaTime;
                        controller.SetVelocity(in velocity);
                        yield return null;
                    }
                    m_character.EnabledMovement = true;
                }
                
                m_isDash = false;
                yield break;
            }

        }
    }
}
