using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StroyMaker.Physics;

namespace StroyMaker.Gizmos {
    [RequireComponent(typeof(KinematicPlatformController2D))]
    public class SimpleLift : MonoBehaviour {
        public float Speed { get => m_speed; set => m_speed = value; }
        public Vector2 Distance { get => m_distance; set => m_distance = value; }
        public AnimationCurve Ease { get => m_ease; set => m_ease = value; }
        public Vector2 Anchor { get => m_anchor; set => m_anchor = value; }

        [SerializeField] private KinematicPlatformController2D m_platformController;
        [SerializeField] private float m_speed;
        [SerializeField] private Vector2 m_distance;
        [SerializeField] AnimationCurve m_ease;
        private Vector2 m_anchor;
        private float m_easeValue;

        private void Reset () {
            m_platformController = GetComponent<KinematicPlatformController2D>();
        }
        private void Awake () {
            if(m_platformController == null) {
                m_platformController = GetComponent<KinematicPlatformController2D>();
            }
            m_anchor = transform.position;
        }

        private void FixedUpdate () {
            if (m_platformController != null) {
                m_easeValue += Time.fixedDeltaTime * m_speed;
                Vector2 destination = m_anchor + m_ease.Evaluate(m_easeValue) * m_distance;
                Vector2 currentPos = m_platformController.Rigidbody.position;
                m_platformController.Velocity = (destination - currentPos) / Time.fixedDeltaTime;
            }
        }

        private void OnDrawGizmos () {
            if (Application.isPlaying) {
                Color backup = UnityEngine.Gizmos.color;
                UnityEngine.Gizmos.color = Color.green;
                UnityEngine.Gizmos.DrawLine(m_anchor, m_anchor + m_distance);
                UnityEngine.Gizmos.color = backup;
            } else {
                Color backup = UnityEngine.Gizmos.color;
                UnityEngine.Gizmos.color = Color.green;
                UnityEngine.Gizmos.DrawLine((Vector2)transform.position, (Vector2)transform.position + m_distance);
                UnityEngine.Gizmos.color = backup;
            }
        }
    }
}