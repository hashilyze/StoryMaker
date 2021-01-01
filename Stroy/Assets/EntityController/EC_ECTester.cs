using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stroy {
    namespace EC {
        [RequireComponent(typeof(EntityController))]
        public class EC_ECTester : MonoBehaviour {
            public EntityController EntityController;
            public MyInputAction InputActions;
            public float Speed = 5f;

            private void Awake() {
                InputActions = new MyInputAction();
                InputActions.Enable();
                
                EntityController = GetComponent<EntityController>();
            }

            private void Update() {
                //EntityController.SetVelocity(InputActions.Player.Move.ReadValue<float>() * Speed * Vector2.right);
                EntityController.SetVelocity(Speed * Vector2.right);
            }

        }
    }
}
