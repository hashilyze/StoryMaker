using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stroy {
    public class DrawOutline : MonoBehaviour {
        public Rigidbody2D m_rigidbody;
        public CompositeCollider2D m_composite;
        public Color m_color_ground;
        public Color m_color_wall;
        public Color m_color_ceil;

        private readonly List<Vector2> total_points = new List<Vector2>();

        private void Update() {
            for (int beg = 0, end = m_composite.pathCount; beg != end; ++beg) {
                total_points.Clear();
                int count = m_composite.GetPath(beg, total_points);
                for (int c = 0; c != count - 1; ++c) {
                    Debug.DrawLine(total_points[c], total_points[c + 1], GetColor(c, c + 1));
                }
                Debug.DrawLine(total_points[count - 1], total_points[0], GetColor(count - 1, 0));
            }
        }

        private Color GetColor(int indexA, int indexB) {
            Vector2 pointA = total_points[indexA];
            Vector2 pointB = total_points[indexB];

            if (Mathf.Abs(pointA.x - pointB.x) > 0.01f) {
                bool isGround = Physics2D.Raycast((pointA + pointB + Vector2.up) * 0.5f, Vector2.down, 1f, Platforms.PlatformConstants.L_BlockMask);
                return isGround ? m_color_ground : m_color_ceil;
            } else {
                return m_color_wall;
            }
        }
    }
}
