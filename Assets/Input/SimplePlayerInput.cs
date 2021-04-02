using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using StroyMaker.Physics;

namespace StroyMaker.Input {
    public class SimplePlayerInput : MonoBehaviour {
        [SerializeField] private KinematicCharacterController2D m_playerCharacter;

        [SerializeField] private float m_runVelocity;
        [SerializeField] private float m_maxJumpHeight;
        [SerializeField] private float m_minJumpHeight;
        private float m_axis;


        public void OnRun (InputAction.CallbackContext context) {
            m_axis = context.ReadValue<float>();
        }
        public void OnJump (InputAction.CallbackContext context) {
            switch (context.phase) {
            case InputActionPhase.Performed: {
                float jumpVelocity = Mathf.Sqrt(2f * PhysicsConstants.k_Gravity * m_maxJumpHeight);
                m_playerCharacter.Jump(jumpVelocity);
                break;
            }
            case InputActionPhase.Canceled: {
                float breakScale = m_maxJumpHeight / m_minJumpHeight;

                m_playerCharacter.BreakJump(breakScale);
                break;
            }
            }
        }

        private void Update () {
            if(m_playerCharacter != null) {
                m_playerCharacter.Run(m_runVelocity * m_axis);
            }
        }
    }
}