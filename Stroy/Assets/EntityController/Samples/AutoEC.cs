using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Stroy.EC.Samples {
    [RequireComponent(typeof(EntityController))]
    public class AutoEC : MonoBehaviour {
        public EntityController EC;
        public float Speed;

        private void Awake() {
            EC = GetComponent<EntityController>();
        }
        private void Update() {
            EC.SetVelocity(Speed * Vector2.right);
        }
    }
}