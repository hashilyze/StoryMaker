using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Stroy {
    [ExecuteInEditMode]
    public class CameraArm : MonoBehaviour {
        [SerializeField] private Vector2 m_offset;
        [SerializeField] private Transform m_target;

        private void Update() {
            transform.position = new Vector3(m_offset.x, m_target.position.y + m_offset.y, transform.position.z);
        }
    }
}
