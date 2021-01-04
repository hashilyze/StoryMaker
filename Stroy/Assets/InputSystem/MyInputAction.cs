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
            ""name"": ""TopView"",
            ""id"": ""6327a4d4-2fa0-4b7a-b714-6d6b2b2ea35a"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""5285b698-0c7a-4319-b365-3818cba09983"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""WASD"",
                    ""id"": ""64954056-db48-4222-9e9f-80dc2727f2d0"",
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
                    ""id"": ""0c37a130-7f27-4282-a0a8-5ca0a0e8b914"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""23b319a8-389c-47a4-8eba-8e5fc8f79816"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""1b577cd9-50e0-445f-ba1a-d946f472ca40"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""7fe20fe1-946c-43d3-8e13-6ae7b0df8693"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
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
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""0fecd4f5-035f-4f00-9f5f-15b5d9e12479"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""35aea2f3-1abf-4878-a33e-a6304aaa20a3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""eee8c0d4-fcf3-4360-b67e-2e1d6c81885f"",
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
                    ""id"": ""f98da49b-e7f2-41db-8ccb-ee941d0db60c"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""092718cb-745a-4957-8ac5-c0bc986b7ea0"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""73044721-dac5-4a18-ad70-7b062f8e0886"",
                    ""path"": ""<Keyboard>/j"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""KeyboardMouse"",
            ""bindingGroup"": ""KeyboardMouse"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // TopView
        m_TopView = asset.FindActionMap("TopView", throwIfNotFound: true);
        m_TopView_Move = m_TopView.FindAction("Move", throwIfNotFound: true);
        // SideView
        m_SideView = asset.FindActionMap("SideView", throwIfNotFound: true);
        m_SideView_Move = m_SideView.FindAction("Move", throwIfNotFound: true);
        m_SideView_Jump = m_SideView.FindAction("Jump", throwIfNotFound: true);
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
    private readonly InputAction m_SideView_Move;
    private readonly InputAction m_SideView_Jump;
    public struct SideViewActions
    {
        private @MyInputAction m_Wrapper;
        public SideViewActions(@MyInputAction wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_SideView_Move;
        public InputAction @Jump => m_Wrapper.m_SideView_Jump;
        public InputActionMap Get() { return m_Wrapper.m_SideView; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(SideViewActions set) { return set.Get(); }
        public void SetCallbacks(ISideViewActions instance)
        {
            if (m_Wrapper.m_SideViewActionsCallbackInterface != null)
            {
                @Move.started -= m_Wrapper.m_SideViewActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_SideViewActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_SideViewActionsCallbackInterface.OnMove;
                @Jump.started -= m_Wrapper.m_SideViewActionsCallbackInterface.OnJump;
                @Jump.performed -= m_Wrapper.m_SideViewActionsCallbackInterface.OnJump;
                @Jump.canceled -= m_Wrapper.m_SideViewActionsCallbackInterface.OnJump;
            }
            m_Wrapper.m_SideViewActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
            }
        }
    }
    public SideViewActions @SideView => new SideViewActions(this);
    private int m_KeyboardMouseSchemeIndex = -1;
    public InputControlScheme KeyboardMouseScheme
    {
        get
        {
            if (m_KeyboardMouseSchemeIndex == -1) m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("KeyboardMouse");
            return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
        }
    }
    public interface ITopViewActions
    {
        void OnMove(InputAction.CallbackContext context);
    }
    public interface ISideViewActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
    }
}
