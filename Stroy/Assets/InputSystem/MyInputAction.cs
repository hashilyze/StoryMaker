// GENERATED AUTOMATICALLY FROM 'Assets/InputSystem/MyInputAction.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @MyInputAction : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @MyInputAction()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""MyInputAction"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""eae3628e-8cca-4e31-a438-088ff851c44a"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Button"",
                    ""id"": ""35fd47b1-d358-4428-8d2e-9ed2074f3ca9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Hold""
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""ee55b919-0610-4f0c-8074-bedd588c5319"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""c6820486-9cae-4965-be03-3fda6fc4908f"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""69dbe076-4b6e-4d01-b7af-794e06d75afd"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        },
        {
            ""name"": ""TopView"",
            ""id"": ""6327a4d4-2fa0-4b7a-b714-6d6b2b2ea35a"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""5285b698-0c7a-4319-b365-3818cba09983"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""f0b8ec4a-6f80-4419-b7b2-a407e57e1167"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""04af9acd-23c0-486d-af67-97c390e30740"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""adb2e685-dd2c-41d4-a2e2-42fec7f6cb2c"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""fa4c0cef-73de-4f5e-9e98-7f1878845e2d"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""e76268b2-749f-43be-9d0f-cb9565eca886"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        },
        {
            ""name"": ""SideView"",
            ""id"": ""996f154e-8ec9-493b-ad0e-8cd99f4e9d56"",
            ""actions"": [
                {
                    ""name"": ""New action"",
                    ""type"": ""Button"",
                    ""id"": ""0fecd4f5-035f-4f00-9f5f-15b5d9e12479"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""a184d240-9d77-48ee-9336-3c76e79fb8d9"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""New action"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard"",
            ""bindingGroup"": ""Keyboard"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Player
        m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
        m_Player_Move = m_Player.FindAction("Move", throwIfNotFound: true);
        // TopView
        m_TopView = asset.FindActionMap("TopView", throwIfNotFound: true);
        m_TopView_Move = m_TopView.FindAction("Move", throwIfNotFound: true);
        // SideView
        m_SideView = asset.FindActionMap("SideView", throwIfNotFound: true);
        m_SideView_Newaction = m_SideView.FindAction("New action", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Player
    private readonly InputActionMap m_Player;
    private IPlayerActions m_PlayerActionsCallbackInterface;
    private readonly InputAction m_Player_Move;
    public struct PlayerActions
    {
        private @MyInputAction m_Wrapper;
        public PlayerActions(@MyInputAction wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_Player_Move;
        public InputActionMap Get() { return m_Wrapper.m_Player; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
        public void SetCallbacks(IPlayerActions instance)
        {
            if (m_Wrapper.m_PlayerActionsCallbackInterface != null)
            {
                @Move.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
            }
            m_Wrapper.m_PlayerActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
            }
        }
    }
    public PlayerActions @Player => new PlayerActions(this);

    // TopView
    private readonly InputActionMap m_TopView;
    private ITopViewActions m_TopViewActionsCallbackInterface;
    private readonly InputAction m_TopView_Move;
    public struct TopViewActions
    {
        private @MyInputAction m_Wrapper;
        public TopViewActions(@MyInputAction wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_TopView_Move;
        public InputActionMap Get() { return m_Wrapper.m_TopView; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(TopViewActions set) { return set.Get(); }
        public void SetCallbacks(ITopViewActions instance)
        {
            if (m_Wrapper.m_TopViewActionsCallbackInterface != null)
            {
                @Move.started -= m_Wrapper.m_TopViewActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_TopViewActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_TopViewActionsCallbackInterface.OnMove;
            }
            m_Wrapper.m_TopViewActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
            }
        }
    }
    public TopViewActions @TopView => new TopViewActions(this);

    // SideView
    private readonly InputActionMap m_SideView;
    private ISideViewActions m_SideViewActionsCallbackInterface;
    private readonly InputAction m_SideView_Newaction;
    public struct SideViewActions
    {
        private @MyInputAction m_Wrapper;
        public SideViewActions(@MyInputAction wrapper) { m_Wrapper = wrapper; }
        public InputAction @Newaction => m_Wrapper.m_SideView_Newaction;
        public InputActionMap Get() { return m_Wrapper.m_SideView; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(SideViewActions set) { return set.Get(); }
        public void SetCallbacks(ISideViewActions instance)
        {
            if (m_Wrapper.m_SideViewActionsCallbackInterface != null)
            {
                @Newaction.started -= m_Wrapper.m_SideViewActionsCallbackInterface.OnNewaction;
                @Newaction.performed -= m_Wrapper.m_SideViewActionsCallbackInterface.OnNewaction;
                @Newaction.canceled -= m_Wrapper.m_SideViewActionsCallbackInterface.OnNewaction;
            }
            m_Wrapper.m_SideViewActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Newaction.started += instance.OnNewaction;
                @Newaction.performed += instance.OnNewaction;
                @Newaction.canceled += instance.OnNewaction;
            }
        }
    }
    public SideViewActions @SideView => new SideViewActions(this);
    private int m_KeyboardSchemeIndex = -1;
    public InputControlScheme KeyboardScheme
    {
        get
        {
            if (m_KeyboardSchemeIndex == -1) m_KeyboardSchemeIndex = asset.FindControlSchemeIndex("Keyboard");
            return asset.controlSchemes[m_KeyboardSchemeIndex];
        }
    }
    public interface IPlayerActions
    {
        void OnMove(InputAction.CallbackContext context);
    }
    public interface ITopViewActions
    {
        void OnMove(InputAction.CallbackContext context);
    }
    public interface ISideViewActions
    {
        void OnNewaction(InputAction.CallbackContext context);
    }
}
