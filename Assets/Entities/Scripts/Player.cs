﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Stroy.Platforms;

namespace Stroy.Entities {
    [RequireComponent(typeof(Character), typeof(PlayerInput))]
    public class Player : MonoBehaviour {
        #region Input Binding
        public void OnMove(InputAction.CallbackContext context) {
            m_inputAxis = context.ReadValue<Vector2>();
        }
        public void OnJump(InputAction.CallbackContext context) {
            switch (context.phase) {
            case InputActionPhase.Performed:
                if (WallJump() == false && Jump() == false) {
                    m_autoJump = true;
                    m_elapsedAutoJump = 0f;
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
                StartCoroutine(Dash(m_inputAxis != Vector2.zero ? m_inputAxis.normalized : m_face * Vector2.right));
                --m_leftDashCount;
            }
        }
        public void OnAttack(InputAction.CallbackContext context) {

        }
        #endregion

        #region State
        public void SetFace(float dir) {
            m_face = dir;
        }

        public void SetJump(float maxHeight, float minHeight) {
            m_jumpVelocity = Mathf.Sqrt(maxHeight * 2f * EntityConstants.Gravity);
            m_jumpPauseSacle = maxHeight / minHeight;

            m_maxJumpHeight = maxHeight;
            m_minJumpHeight = minHeight;
        }
        public void ResetJumpCount() { m_leftMorerJumpCount = m_maxMorerJumpCount; }
        public void SetJumpCount(int count) { m_leftMorerJumpCount = Mathf.Clamp(count, 0, m_maxMorerJumpCount); }

        public void SetDash(float distance, float speed) {
            m_dashDistance = distance;
            m_dashSpeed = speed;
            m_dashTime = distance / speed;
        }
        public void ResetDashCount() { m_leftDashCount = m_maxDashCount; }
        public void SetDashCount(int count) { m_leftDashCount = Mathf.Clamp(count, 0, m_maxDashCount); }
        #endregion

        #region Variable
        private const float AUTO_JUMP_LIMIT = 0.1f;
        private const float MIN_WALL_JUMP = 0.1f;
        private const float DEFAULT_FRICTION = 1f;

        // Component
        [HideInInspector] private Character m_platformer;
        // State
        private float m_face;
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
        // Auto jump
        private bool m_autoJump;
        private float m_elapsedAutoJump;
        // Slide
        [SerializeField] private float m_slideLimit;
        private bool m_isSlide;

        // Wall Jump
        [SerializeField] private Vector2 m_wallJumpVelocity;
        private bool m_isWallJump;
        private float m_elapsedWallJump;
        // Dash
        [SerializeField] private float m_dashSpeed;
        [SerializeField] private float m_dashDistance;
        [SerializeField] private int m_maxDashCount;
        private int m_leftDashCount;
        [HideInInspector] private float m_dashTime;
        private bool m_isDash;
        // Input
        private Vector2 m_inputAxis;
        // Platform
        private ConveyorBelt m_conveyorBelt;
        private bool m_hasConveyorBelt;
        #endregion

        #region Component
        private void Awake() {
            m_platformer = GetComponent<Character>();

            SetFace(1f);
            SetJump(m_maxJumpHeight, m_minJumpHeight);
            SetDash(m_dashDistance, m_dashSpeed);

            ResetDashCount();
            ResetJumpCount();

            m_platformer.OnLanding += ResetJumpCount;
            m_platformer.OnLanding += ResetDashCount;
        }

        private void Update() {
            // Dash routine
            {
                if (m_isDash) {
                    return;
                }
            }
            // Wall Jump routine
            {
                if (m_isWallJump) {
                    m_elapsedWallJump += Time.deltaTime;
                    if (m_platformer.IsGround || m_platformer.IsFall || (m_elapsedWallJump > MIN_WALL_JUMP && m_platformer.IsWall)) {
                        m_isWallJump = false;
                    } else {
                        return;
                    }
                }
            }
            // Normal routine: Run-Jump-Slide
            {
                float deltaTime = Time.deltaTime;
                if (m_autoJump) {
                    m_elapsedAutoJump += deltaTime;
                    // Auto jump time over or Success
                    if (m_elapsedAutoJump > AUTO_JUMP_LIMIT || Jump()) {
                        m_autoJump = false;
                    }
                }

                bool wasSlide = m_isSlide;
                if (m_platformer.IsWall && m_platformer.IsFall
                    && (m_inputAxis.x < 0f && m_platformer.WallOnLeft || m_inputAxis.x > 0f && m_platformer.WallOnRight)) {
                    m_isSlide = true;
                    if (!wasSlide) {
                        m_platformer.Grab(m_platformer.ContactWall);
                    }
                    Slide(deltaTime);
                } else if (wasSlide) {
                    m_isSlide = false;
                    m_platformer.Release();
                }
                if (m_inputAxis.x != 0f) SetFace(m_inputAxis.x);
                Run(m_inputAxis.x, deltaTime);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision) {
            if (m_platformer.IsGround) {
                Collider2D ground = m_platformer.ContactGround;
                if (ground == collision.collider) {
                    if (ground.CompareTag(PlatformConstants.T_Belt)) {
                        m_conveyorBelt = ground.GetComponent<ConveyorBelt>();
                        m_currentSpeed += m_conveyorBelt.Speed;
                        m_hasConveyorBelt = true;
                    }
                }
            }
        }
        private void OnCollisionExit2D(Collision2D collision) {
            if (m_hasConveyorBelt && m_conveyorBelt.gameObject == collision.gameObject) {
                m_conveyorBelt = null;
                m_hasConveyorBelt = false;
            }
        }
        #endregion

        #region Normal Mode
        private void OffNormalMode() {
            m_currentSpeed = 0f;
            BreakJump();
            m_isSlide = false;
            m_platformer.Release();            
        }
        #endregion

        #region Run
        /// <summary>Run or AirControl with acceleration</summary>
        private void Run(float inputDir, float deltaTime) {
            if (m_hasConveyorBelt) m_currentSpeed -= m_conveyorBelt.Speed;
            // Run speed update
            {
                float magnitude = m_currentSpeed < 0f ? -m_currentSpeed : m_currentSpeed;
                float curDir = m_currentSpeed < 0f ? -1f : 1f;

                if (inputDir != 0f) {
                    if (inputDir * m_currentSpeed < 0f) { // Turn
                        magnitude -= m_turnDec * deltaTime * m_firction;
                        if (magnitude < 0f) magnitude = 0f;
                        m_currentSpeed = magnitude * curDir;
                    } else { // Run
                        if (magnitude < m_topSpeed) { // Accelerate
                            magnitude += m_acc * deltaTime * m_firction;
                            if (magnitude > m_topSpeed) magnitude = m_topSpeed;
                            m_currentSpeed = magnitude * inputDir;
                        } else {
                            // Damp exceed speed only on ground
                            if (m_platformer.IsGround) {
                                if (m_topSpeed < magnitude) {
                                    magnitude -= m_dec * deltaTime * m_firction;
                                }
                                m_currentSpeed = magnitude * inputDir;
                            }
                        }
                    }
                } else {
                    if (magnitude != 0f) { // Break
                        magnitude -= m_dec * deltaTime * m_firction;
                        if (magnitude < 0f) magnitude = 0f;
                        m_currentSpeed = magnitude * curDir;
                    }
                }
            }
            if (m_hasConveyorBelt) m_currentSpeed += m_conveyorBelt.Speed;
            m_platformer.Run(m_currentSpeed);
        }
        #endregion

        #region Jump
        private bool Jump() {
            if (m_platformer.IsGround) { // Ground jump
                m_platformer.Jump(m_jumpVelocity);
            } else if (m_leftMorerJumpCount > 0) { // Morer jump
                m_platformer.Jump(m_jumpVelocity);
                --m_leftMorerJumpCount;
            } else { // Jump fail 
                return false;
            }
            m_isWallJump = false;
            m_autoJump = false;
            return true;
        }
        private void BreakJump() {
            m_platformer.BreakJump(m_jumpPauseSacle);
            m_autoJump = false;
        }
        #endregion

        #region Slide
        private void Slide(float deltaTime) {
            float speed = m_platformer.Velocity.y - EntityConstants.Gravity * deltaTime;
            if(speed < -m_slideLimit) {
                speed = -m_slideLimit;
            }
            m_platformer.Climb(speed);
        }
        #endregion




        #region Wall Jump
        private bool WallJump() {
            if (!m_platformer.IsGround && m_platformer.IsWall) {
                OffNormalMode();

                m_platformer.Jump(m_wallJumpVelocity.y);
                m_currentSpeed = m_wallJumpVelocity.x * (m_platformer.WallOnLeft ? 1f : -1f);
                m_platformer.Run(m_currentSpeed);

                SetFace(m_currentSpeed < 0f ? -1f : 1f);

                m_isWallJump = true;
                m_elapsedWallJump = 0f;
                return true;
            }
            return false;
        }
        #endregion

        #region Dash
        private IEnumerator Dash(Vector2 direction) {
            m_isWallJump = false;
            m_isDash = true;
            OffNormalMode();


            Vector2 velocity = m_dashSpeed * direction;
            float elapsedTime = 0f;
            m_dashTime = m_dashDistance / m_dashSpeed;

            if (m_platformer.IsGround) {
                while (elapsedTime < m_dashTime) {
                    elapsedTime += Time.deltaTime;
                    m_platformer.Run(velocity.x);
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
                controller.SetVelocity(Vector2.zero);
            }
            
            m_isDash = false;
            yield break;
        }
        #endregion
    }
}