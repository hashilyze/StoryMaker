// GENERATED AUTOMATICALLY FROM 'Assets/Input/PlayerInputActions.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @PlayerInputActions : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInputActions"",
    ""maps"": [
        {
            ""name"": ""Walker"",
            ""id"": ""b69bce3d-a3f7-44af-91ac-57c2a17665f4"",
            ""actions"": [
                {
                    ""name"": ""Run"",
                    ""type"": ""Value"",
                    ""id"": ""e7fa65f3-2b27-4fb8-8d8f-83666c19eb23"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""63a25680-9d3a-4bec-9d54-6a82ca87cc51"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""9d70b8e4-a6c7-4d69-a9a7-74681a0f2329"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Run"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""51967e26-a61a-418e-aad3-6b4f3109cbda"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Run"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""c47b2e3d-7c7b-4f9e-adea-46080d4944b4"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Run"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""f3768129-b014-49ce-b16b-893245195c6f"",
                    ""path"": ""<Keyboard>/k"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Jump"",
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
        // Walker
        m_Walker = asset.FindActionMap("Walker", throwIfNotFound: true);
        m_Walker_Run = m_Walker.FindAction("Run", throwIfNotFound: true);
        m_Walker_Jump = m_Walker.FindAction("Jump", throwIfNotFound: true);
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

    // Walker
    private readonly InputActionMap m_Walker;
    private IWalkerActions m_WalkerActionsCallbackInterface;
    private readonly InputAction m_Walker_Run;
    private readonly InputAction m_Walker_Jump;
    public struct WalkerActions
    {
        private @PlayerInputActions m_Wrapper;
        public WalkerActions(@PlayerInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Run => m_Wrapper.m_Walker_Run;
        public InputAction @Jump => m_Wrapper.m_Walker_Jump;
        public InputActionMap Get() { return m_Wrapper.m_Walker; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(WalkerActions set) { return set.Get(); }
        public void SetCallbacks(IWalkerActions instance)
        {
            if (m_Wrapper.m_WalkerActionsCallbackInterface != null)
            {
                @Run.started -= m_Wrapper.m_WalkerActionsCallbackInterface.OnRun;
                @Run.performed -= m_Wrapper.m_WalkerActionsCallbackInterface.OnRun;
                @Run.canceled -= m_Wrapper.m_WalkerActionsCallbackInterface.OnRun;
                @Jump.started -= m_Wrapper.m_WalkerActionsCallbackInterface.OnJump;
                @Jump.performed -= m_Wrapper.m_WalkerActionsCallbackInterface.OnJump;
                @Jump.canceled -= m_Wrapper.m_WalkerActionsCallbackInterface.OnJump;
            }
            m_Wrapper.m_WalkerActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Run.started += instance.OnRun;
                @Run.performed += instance.OnRun;
                @Run.canceled += instance.OnRun;
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
            }
        }
    }
    public WalkerActions @Walker => new WalkerActions(this);
    private int m_KeyboardSchemeIndex = -1;
    public InputControlScheme KeyboardScheme
    {
        get
        {
            if (m_KeyboardSchemeIndex == -1) m_KeyboardSchemeIndex = asset.FindControlSchemeIndex("Keyboard");
            return asset.controlSchemes[m_KeyboardSchemeIndex];
        }
    }
    public interface IWalkerActions
    {
        void OnRun(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
    }
}
