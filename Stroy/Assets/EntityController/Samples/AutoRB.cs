using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Stroy.EC.Samples {
    [RequireComponent(typeof(Rigidbody2D))]
    public class AutoRB : MonoBehaviour {
        public Rigidbody2D RB;
        public float Speed;

        private void Awake() {
            RB = GetComponent<Rigidbody2D>();
        }
        private void FixedUpdate() {
            RB.MovePosition(RB.position + Speed * Time.fixedDeltaTime * Vector2.right);
            RB.velocity = Speed * Vector2.right;
        }
    }
}