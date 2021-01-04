using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stroy {
    namespace EC {
        [RequireComponent(typeof(EntityController))]
        public class SidePlayer : MonoBehaviour {
            public float Speed;
            public float MaxHeight;
            public float MinHeight;

            //===========================================================
            // Buffer
            private const int HIT_BUFFER_SIZE = 8;
            private const int COLLIDER_BUFFER_SIZE = 8;
            private readonly RaycastHit2D[] m_bufferHit = new RaycastHit2D[HIT_BUFFER_SIZE];
            private readonly Collider2D[] m_bufferCollider = new Collider2D[COLLIDER_BUFFER_SIZE];

            // Input
            private MyInputAction m_inputActions;

            // State
            private EntityController m_entityController;
            private Vector2 m_velocity;
            private bool m_isGround;
            private float m_contactAngle;



            private void OnEnable() {
                m_entityController.enabled = true;
                m_inputActions.Enable();
            }
            private void OnDisable() {
                m_entityController.enabled = false;
                m_inputActions.Disable();
            }

            private void Awake() {
                m_entityController = GetComponent<EntityController>();
                m_inputActions = new MyInputAction();
            }

            private void Update() {
                bool wasGround = m_isGround;
                
                CheckGround();

                if (m_isGround && !wasGround) { // Landing 
                    m_velocity.y = 0f;
                    m_velocity = Vector2.zero;
                } else if(!m_isGround){ // Airbone
                    m_velocity = m_entityController.Velocity;
                    m_velocity.y -= ECConstants.Gravity * Time.deltaTime;
                }

                float move = m_inputActions.SideView.Move.ReadValue<float>();
                m_velocity.x = move * Speed;

                m_entityController.SetVelocity(in m_velocity);
            }

            private void CheckGround() {
                RaycastHit2D hitBlock = Physics2D.BoxCast(m_entityController.AttachedRigidbody.position, m_entityController.Size, 0f,
                    Vector2.down, ECConstants.MaxContactOffset, ECConstants.BlockMask);

                if (hitBlock) {
                    float angle = Vector2.SignedAngle(Vector2.up, hitBlock.normal);
                    if (-ECConstants.SlopeLimit < angle && angle < ECConstants.SlopeLimit) {
                        m_contactAngle = angle;
                        m_isGround = true;

                        return;
                    }
                }
                m_isGround = false;
                m_contactAngle = 0f;
            }
        }
    }
}
