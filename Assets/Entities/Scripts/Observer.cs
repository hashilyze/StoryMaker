using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stroy.Entities {
    public class Observer : MonoBehaviour {
        public LayerMask Targets;
        public LayerMask Obstacles;
        public Collider2D target;

        public Fov Fov;
        

        private readonly Collider2D[] colliders = new Collider2D[16];
        private void Update() {
            int count = Fov.Monitor(Targets, Obstacles, colliders);
            for (int c = 0; c != count; ++c) {
                Debug.DrawLine(colliders[c].transform.position, Fov.transform.position, Color.red);
            }

            if (Fov.IsVisible(target, Obstacles)) {
                Debug.DrawLine(target.transform.position, Fov.transform.position, Color.blue);
            }
        }
    }
}
