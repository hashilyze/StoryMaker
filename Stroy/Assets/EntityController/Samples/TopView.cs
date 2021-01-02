﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Stroy {

    namespace EC.Samples {
        [RequireComponent(typeof(EntityController))]
        public class TopView : MonoBehaviour {
            public float Speed;

            private EntityController m_ec;
            private MyInputAction m_inputAction;

            private void Awake() {
                m_ec = GetComponent<EntityController>();
                m_inputAction = new MyInputAction();
            }
            private void Update() {
                m_ec.SetVelocity(Speed * m_inputAction.TopView.Move.ReadValue<Vector2>());
            }

            private void OnEnable() {
                m_ec.Enable();
                m_inputAction.Enable();
            }
            private void OnDisable() {
                m_ec.Disable();
                m_inputAction.Disable();
            }
        }
    }
}