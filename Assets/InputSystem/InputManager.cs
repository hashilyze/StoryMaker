using UnityEngine;
using UnityEngine.InputSystem;

namespace Stroy.Input {
    public class InputManager : MonoBehaviour {
        public static void Bind(MyInputAction.IPlayerActions player) {
            if (player == null) return;

            InputManager instance = Instance;

            instance.m_action_Move.started += player.OnMove;
            instance.m_action_Move.performed += player.OnMove;
            instance.m_action_Move.canceled += player.OnMove;

            instance.m_action_Attack.started += player.OnAttack;
            instance.m_action_Attack.performed += player.OnAttack;
            instance.m_action_Attack.canceled += player.OnAttack;

            instance.m_action_Jump.started += player.OnJump;
            instance.m_action_Jump.performed += player.OnJump;
            instance.m_action_Jump.canceled += player.OnJump;

            instance.m_action_Dash.started += player.OnDash;
            instance.m_action_Dash.performed += player.OnDash;
            instance.m_action_Dash.canceled += player.OnDash;

        }
        public static void Debind(MyInputAction.IPlayerActions player) {
            if (player == null) return;

            InputManager instance = Instance;

            instance.m_action_Move.started -= player.OnMove;
            instance.m_action_Move.performed -= player.OnMove;
            instance.m_action_Move.canceled -= player.OnMove;

            instance.m_action_Attack.started -= player.OnAttack;
            instance.m_action_Attack.performed -= player.OnAttack;
            instance.m_action_Attack.canceled -= player.OnAttack;

            instance.m_action_Jump.started -= player.OnJump;
            instance.m_action_Jump.performed -= player.OnJump;
            instance.m_action_Jump.canceled -= player.OnJump;

            instance.m_action_Dash.started -= player.OnDash;
            instance.m_action_Dash.performed -= player.OnDash;
            instance.m_action_Dash.canceled -= player.OnDash;
        }

        private static InputManager m_instance;
        private MyInputAction m_myInputAction;
        private MyInputAction.PlayerActions m_player;
        private InputAction m_action_Move;
        private InputAction m_action_Attack;
        private InputAction m_action_Jump;
        private InputAction m_action_Dash;


        private static InputManager Instance {
            get {
                if (m_instance == null) {
                    m_instance = FindObjectOfType<InputManager>();
                    m_instance.Initialize();
                }
                return m_instance;
            }
        }

        private void OnEnable() { m_myInputAction.Enable(); }
        private void OnDisable() { m_myInputAction.Disable(); }

        private void Awake() {
            if (m_instance == null) {
                m_instance = this;
                Initialize();
            }
        }
        private void Initialize() {
            m_myInputAction = new MyInputAction();
            m_player = m_myInputAction.Player;

            m_action_Move = m_player.Move;
            m_action_Attack = m_player.Attack;
            m_action_Jump = m_player.Jump;
            m_action_Dash = m_player.Dash;
        }
    }
}
