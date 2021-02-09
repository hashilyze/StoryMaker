using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Stroy.Platforms;

namespace Stroy.Entities {
    [RequireComponent(typeof(Character))]
    public class Player : MonoBehaviour, MyInputAction.IPlayerActions {
        public Transform target;

        #region Input Binding
        public void OnMove(InputAction.CallbackContext context) {
            m_inputAxis = context.ReadValue<Vector2>();
        }
        public void OnJump(InputAction.CallbackContext context) {
            switch (context.phase) {
            case InputActionPhase.Performed:
                if (WallJump()) return;
                else if (Jump()) return;
                else {
                    m_autoJump = true;
                    StartCoroutine(StopAutoJump());
                }
                return;
            case InputActionPhase.Canceled:
                if (m_isWallJump) return;

                BreakJump();
                return;
            }
        }
        public void OnDash(InputAction.CallbackContext context) {
            if (context.performed) {
                if (m_isDash || m_leftDashCount <= 0) return;
                StartCoroutine(Dash(m_inputAxis != Vector2.zero ? m_inputAxis.normalized : m_character.Face * Vector2.right));
                --m_leftDashCount;
            }
        }
        public void OnAttack(InputAction.CallbackContext context) {
            switch (context.phase) {
            case InputActionPhase.Performed:
                int colliderNum = m_attackArea.OverlapCollider(ATTACK_FILTER, m_bufferCollider);
                for(int n = 0; n != colliderNum; ++n) {
                    Collider2D enemy = m_bufferCollider[n];
                    Debug.Log(enemy.name);
                    if(enemy.TryGetComponent(out Health health)) {
                        health.TakeDamage(m_damage);
                    }
                }
                break;
            }
        }
        #endregion

        #region State
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
        private const float DEFAULT_FRICTION = 1f;
        private const float AUTO_JUMP_LIMIT = 3f;        
        private static readonly WaitForSeconds AUTO_JUMP_TIMER = new WaitForSeconds(AUTO_JUMP_LIMIT);
        private const float MIN_WALL_JUMP = 0.1f;
        private const int COLLIDER_BUFFER_SIZE = 8;
        private readonly Collider2D[] m_bufferCollider = new Collider2D[COLLIDER_BUFFER_SIZE];
        private static readonly ContactFilter2D ATTACK_FILTER = new ContactFilter2D() { useLayerMask = true, layerMask = 0x01 << EntityConstants.L_Enemy };

        // Component
        [HideInInspector] private Character m_character;
        [Header("Run")]
        [SerializeField] private float m_topSpeed;
        [SerializeField] private float m_acc;
        [SerializeField] private float m_dec;
        [SerializeField] private float m_turnDec;
        [SerializeField] private float m_firction = DEFAULT_FRICTION;
        private float m_currentSpeed;
        [Header("Jump")]
        [SerializeField] private float m_maxJumpHeight;
        [SerializeField] private float m_minJumpHeight;
        [HideInInspector] private float m_jumpVelocity;
        [HideInInspector] private float m_jumpPauseSacle;
        [SerializeField] private int m_maxMorerJumpCount;
        private int m_leftMorerJumpCount;
        private bool m_autoJump;
        [Header("Slide")]
        [SerializeField] private float m_slideLimit;
        private bool m_isSlide;
        [Header("WallJump")]
        [SerializeField] private Vector2 m_wallJumpVelocity;
        private bool m_isWallJump;
        private float m_elapsedWallJump;
        [Header("Dash")]
        [SerializeField] private float m_dashSpeed;
        [SerializeField] private float m_dashDistance;
        [SerializeField] private int m_maxDashCount;
        private int m_leftDashCount;
        [HideInInspector] private float m_dashTime;
        private bool m_isDash;
        [Header("Attack")]
        [SerializeField] private float m_damage;
        [SerializeField] private Collider2D m_attackArea;
        // Input
        private Vector2 m_inputAxis;
        // Platform
        private ConveyorBelt m_conveyorBelt;
        private bool m_hasConveyorBelt;

        [Header("Combat")]
        [SerializeField] private float m_invincibleTime;
        [SerializeField] private Color m_invincibleColor;
        #endregion

        #region Component
        private void Awake() {
            m_character = GetComponent<Character>();

            SetJump(m_maxJumpHeight, m_minJumpHeight);
            SetDash(m_dashDistance, m_dashSpeed);

            ResetDashCount();
            ResetJumpCount();

            m_character.OnLanding += TryAutoJump;
            m_character.OnLanding += ResetJumpCount;
            m_character.OnLanding += ResetDashCount;

            Input.InputManager.Bind(this);
        }
        private void Start() {
            m_character.Health.OnDamaged += (Health health, float damage) => {
                health.SetInvincible(true);
                StartCoroutine(InvincibleTimer(m_invincibleTime));
                StartCoroutine(InvincibleEffect());
            };
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
                    if (m_character.IsGround || m_character.IsFall || (m_elapsedWallJump > MIN_WALL_JUMP && m_character.IsWall)) {
                        m_isWallJump = false;
                    } else {
                        return;
                    }
                }
            }
            // Normal routine: Run-Jump-Slide
            {
                float deltaTime = Time.deltaTime;

                bool wasSlide = m_isSlide;
                if (m_character.IsFall && m_character.IsWall
                    && (m_inputAxis.x < 0f && m_character.WallOnLeft || m_inputAxis.x > 0f && m_character.WallOnRight)) {
                    m_isSlide = true;
                    if (!wasSlide) {
                        m_character.Grab(m_character.ContactWall, true);
                    }
                    Slide();
                } else if (wasSlide) {
                    m_isSlide = false;
                    m_character.Release();
                }
                Run(m_inputAxis.x, deltaTime);

                m_character.SpriteRenderer.flipX = m_character.Face < 0f;
                m_character.Animator.SetBool("IsGround", m_character.IsGround);
                m_character.Animator.SetFloat("VelocityX", Mathf.Abs(m_character.Velocity.x));
                m_character.Animator.SetFloat("VelocityY", m_character.Velocity.y);
            }
        }
        
        // Collision Events
        private void OnCollisionEnter2D(Collision2D collision) {
            if (m_character.IsGround) {
                Collider2D ground = m_character.ContactGround;
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
        private IEnumerator InvincibleTimer(float time) {
            yield return new WaitForSeconds(time);
            if(m_character != null) {
                m_character.Health.SetInvincible(false);
            }
        }
        private IEnumerator InvincibleEffect() {
            SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();
            Color original = renderer.color;
            while (m_character.Health.Invincible) {
                if(renderer.color == original) {
                    renderer.color = m_invincibleColor;
                } else {
                    renderer.color = original;
                }
                yield return new WaitForSeconds(0.2f);
            }
            renderer.color = original;
            yield break;
        }

        private void OffNormalMode() {
            m_currentSpeed = 0f;
            BreakJump();
            m_isSlide = false;
            m_character.Release();            
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
                            if (m_character.IsGround) {
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
            m_character.Run(m_currentSpeed);
        }
        #endregion

        #region Jump
        private bool Jump() {
            if (m_character.IsGround) { // Ground jump
                m_character.Jump(m_jumpVelocity);
            } else if (m_leftMorerJumpCount > 0) { // Morer jump
                m_character.Jump(m_jumpVelocity);
                --m_leftMorerJumpCount;
            } else { // Jump fail 
                return false;
            }
            m_isWallJump = false;
            m_autoJump = false;
            return true;
        }
        private void BreakJump() {
            m_character.BreakJump(m_jumpPauseSacle);
            m_autoJump = false;
        }
        private void TryAutoJump() {
            if (m_autoJump) {
                Jump();
                m_autoJump = false;
            }
        }
        private IEnumerator StopAutoJump() {
            yield return AUTO_JUMP_TIMER;
            TryAutoJump();
        }
        #endregion

        #region Slide
        private void Slide() {
            if(m_character.Velocity.y < -m_slideLimit) {
                m_character.Climb(-m_slideLimit);
            }
        }
        #endregion

        #region Wall Jump
        private bool WallJump() {
            if (!m_character.IsGround && m_character.IsWall) {
                OffNormalMode();

                m_character.Jump(m_wallJumpVelocity.y);
                if (m_character.WallOnLeft) {
                    m_currentSpeed = m_wallJumpVelocity.x;
                    m_character.SpriteRenderer.flipX = false;
                } else {
                    m_currentSpeed = -m_wallJumpVelocity.x;
                    m_character.SpriteRenderer.flipX = true;
                }
                m_character.Run(m_currentSpeed);

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

            if (m_character.IsGround) {
                while (elapsedTime < m_dashTime) {
                    elapsedTime += Time.deltaTime;
                    m_character.Run(velocity.x);
                    yield return null;
                }
                ResetDashCount();
            } else {
                m_character.enabled = false;
                EntityController controller = m_character.Controller;
                while (elapsedTime < m_dashTime) {
                    elapsedTime += Time.deltaTime;
                    controller.SetVelocity(velocity);
                    yield return null;
                }
                m_character.enabled = true;
                controller.SetVelocity(Vector2.zero);
            }
            
            m_isDash = false;
            yield break;
        }
        #endregion
    }
}