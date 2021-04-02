using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stroy {
    [ExecuteAlways]
    public class CameraArm : MonoBehaviour {
        public Transform Target;
        public float Depth = 10f;

        private void LateUpdate() {
            if (Target) {
                transform.position = Target.position + Vector3.back * Depth;
            }
        }
    }
}
