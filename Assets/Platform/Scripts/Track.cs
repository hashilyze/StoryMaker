using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stroy {
    namespace Platform {
        [System.Serializable]
        public readonly struct GCD_Result {
            public GCD_Result(float sqrDistance, Vector2 point, int closestNode, int otherClosestNode = EmptyNode) {
                Success = true;
                SqrDistance = sqrDistance;
                Point = point;
                ClosestNode = closestNode;
                OtherClosestNode = otherClosestNode;
                OnPoint = otherClosestNode == EmptyNode;
            }

            public const int EmptyNode = -1;

            public readonly bool Success;                       // Find closest distance
            public readonly float SqrDistance;                  // Squared the closest distance
            public float Distance => Mathf.Sqrt(SqrDistance);   // The closest distance
            public readonly Vector2 Point;                      // The closest point
            public readonly int ClosestNode;                    // The closest node from point
            public readonly int OtherClosestNode;               // Other closest node; it only use point on line
            public readonly bool OnPoint;                       // The closest point on node, not line
        }

        public class Track : MonoBehaviour {
            #region Public
            // Track state
            public int Size => m_nodes.Length;
            public Vector2 GetNode(int index) => m_nodes[index];
            public Vector2[] GetNodes => m_nodes.Clone() as Vector2[];
            public bool IsPoint => m_nodes.Length == 1;
            public bool IsCycle => m_isCycle;
            public Vector2 LocalZero => m_localZero;


            public GCD_Result GetClosest(Vector2 target) {
                if (m_nodes.Length < 1) { // No track
                    throw new System.NullReferenceException("Track has not nodes");
                }

                GCD_Result result;
                // GetClosestDistance result
                {
                    if (m_nodes.Length == 1) { // Track is a point
                        result = GCD_Point(target - m_localZero, 0);
                    } else { // Track is a line or lines
                        Vector2 localTarget = target - m_localZero;
                        GCD_Result closest = InfResult;
                        for (int d = 0, dEnd = m_delimeters.Count - 1; d != dEnd; ++d) {
                            GCD_Result newClosest;
                            if (m_delimeters[d + 1] - m_delimeters[d] < 2) { // Grouping track is a point or a line
                                newClosest = GCD_BruteForce(localTarget, m_delimeters[d], m_delimeters[d + 1]);
                            } else {
                                newClosest = GCD_GJK(localTarget, m_delimeters[d], m_delimeters[d + 1]);
                                if (newClosest.Success == false) { // Target inside polygon of grouping track
                                    newClosest = GCD_BruteForce(localTarget, m_delimeters[d], m_delimeters[d + 1]);
                                }
                            }
                            if (newClosest.SqrDistance < closest.SqrDistance) {
                                closest = newClosest;
                                // Target on track
                                if (closest.SqrDistance < PlatformConstants.NearByZero) break;
                            }
                        }
                        result = closest;
                    }
                }
                return result;
            }
            public void Bake() {
                m_localZero = transform.position;
                m_delimeters.Clear();

                if (m_nodes.Length >= 2) {
                    m_isCycle = m_nodes[0] == m_nodes[m_nodes.Length - 1];

                    // Grouping lines
                    {
                        // First delimeter
                        m_delimeters.Add(0);
                        // Seperate lines
                        if (m_nodes.Length > 3) {
                            float currentClockWise = Vector3.Cross(m_nodes[1] - m_nodes[0], m_nodes[2] - m_nodes[1]).z < 0f ? -1f : 1f;
                            Vector2 startNode = m_nodes[0];
                            Vector2 startVector = m_nodes[1] - startNode;

                            for (int n = 1, nEnd = m_nodes.Length - 2; n != nEnd; ++n) {
                                if (currentClockWise * Vector3.Cross(m_nodes[n + 1] - m_nodes[n], m_nodes[n + 2] - m_nodes[n + 1]).z < 0f) {
                                    // Check changed clock wise
                                    currentClockWise *= -1f;
                                } else if (startNode != m_nodes[n + 2] && currentClockWise * Vector3.Cross(startNode - m_nodes[n + 2], startVector).z < 0f) {
                                    // Check start point inside convex hell
                                } else {
                                    continue;
                                }
                                m_delimeters.Add(n + 1);
                                startVector = m_nodes[n + 2] - m_nodes[n + 1];
                                startNode = m_nodes[n + 1];

                            }

                        }
                        // Last delimeter
                        m_delimeters.Add(m_nodes.Length - 1);
                    }
                }
            }

            #endregion

            #region Private
            private static readonly GCD_Result InfResult = new GCD_Result(Mathf.Infinity, Vector2.zero, 0);
            private const int GJK_LIMIT_FACTOR = 2;

            // State
            [SerializeField] private Vector2[] m_nodes;                     // Node which is vertex of lines
            private readonly List<int> m_delimeters = new List<int>();      // Groupping line's start and end nodes
            private Vector2 m_localZero;                                    // Cached world position of track
            private bool m_isCycle;
            private readonly List<int> m_simplex = new List<int>(3);

            private void Awake() { Bake(); }

            #region GCD: GetClosestDistance
            // GCD Simplex
            private GCD_Result GCD_Point(Vector2 localTarget, int index) {
                return new GCD_Result((localTarget - m_nodes[index]).sqrMagnitude, m_nodes[index], index);
            }
            private GCD_Result GCD_Line(Vector2 localTarget, int indexA, int indexB) {
                ref Vector2 nodeA = ref m_nodes[indexA];
                ref Vector2 nodeB = ref m_nodes[indexB];

                float rate = Vector2.Dot(localTarget - nodeA, nodeB - nodeA) / (nodeB - nodeA).sqrMagnitude;

                Vector2 point;
                int closestNode;
                int otherClosestNode = GCD_Result.EmptyNode;

                if(rate <= 0f) { // Point on nodeA
                    point = nodeA;
                    closestNode = indexA;
                } else if(rate < 1f) { // Point on line
                    point = (1 - rate) * nodeA + rate * nodeB;
                    if(rate < 0.5f) {
                        closestNode = indexA;
                        otherClosestNode = indexB;
                    } else {
                        closestNode = indexB;
                        otherClosestNode = indexA;
                    }
                } else { // Point on nodeB
                    point = nodeB;
                    closestNode = indexB;
                }
                return new GCD_Result((localTarget - point).sqrMagnitude, point, closestNode, otherClosestNode);
            }
            private GCD_Result GCD_Triangle(Vector2 localTarget, int indexA, int indexB, int indexC) {
                ref Vector2 nodeA = ref m_nodes[indexA];
                ref Vector2 nodeB = ref m_nodes[indexB];
                ref Vector2 nodeC = ref m_nodes[indexC];
                // All of vector of line are same clockwise
                Vector2 lineA = nodeB - nodeA;
                Vector2 lineB = nodeC - nodeB;
                Vector2 lineC = nodeA - nodeC;
                
                float area = Vector3.Cross(lineA, lineB).z;
                float areaRateA = Vector3.Cross(lineA, localTarget - nodeA).z / area;
                float areaRateB = Vector3.Cross(lineB, localTarget - nodeB).z / area;
                float areaRateC = Vector3.Cross(lineC, localTarget - nodeC).z / area;
                
                // Target inside polygon
                if (0f < areaRateA && 0f < areaRateB && 0f < areaRateC) return new GCD_Result();
               
                float lineRateA = Vector2.Dot(lineA, localTarget - nodeA) / lineA.sqrMagnitude;
                float lineRateB = Vector2.Dot(lineB, localTarget - nodeB) / lineB.sqrMagnitude;
                float lineRateC = Vector2.Dot(lineC, localTarget - nodeC) / lineC.sqrMagnitude;

                Vector2 point;
                int closestNode;
                int otherClosestNode = GCD_Result.EmptyNode;
                if (lineRateA <= 0 && 1 <= lineRateC) { // Point on nodeA
                    point = nodeA;
                    closestNode = indexA;
                } else if (lineRateB <= 0 && 1 <= lineRateA) { // Point on nodeB
                    point = nodeB;
                    closestNode = indexB;
                } else if (lineRateC <= 0 && 1 <= lineRateB) { // Point on nodeC
                    point = nodeC;
                    closestNode = indexC;
                } else if (0 < lineRateA && lineRateA < 1 && areaRateA <= 0) { // Point on lineA
                    point = (1 - lineRateA) * nodeA + lineRateA * nodeB;
                    if(lineRateA < 0.5f) {
                        closestNode = indexA;
                        otherClosestNode = indexB;
                    } else {
                        closestNode = indexB;
                        otherClosestNode = indexA;
                    }
                } else if (0 < lineRateB && lineRateB < 1 && areaRateB <= 0) { // Point on lineB
                    point =  (1 - lineRateB) * nodeB + lineRateB * nodeC;
                    if (lineRateB < 0.5f) {
                        closestNode = indexB;
                        otherClosestNode = indexC;
                    } else {
                        closestNode = indexC;
                        otherClosestNode = indexB;
                    }
                } else { // Point on lineC
                    point =  (1 - lineRateC) * nodeC + lineRateC * nodeA;
                    if (lineRateC < 0.5f) {
                        closestNode = indexC;
                        otherClosestNode = indexA;
                    } else {
                        closestNode = indexA;
                        otherClosestNode = indexC;
                    }
                }
                return new GCD_Result((localTarget - point).sqrMagnitude, point, closestNode, otherClosestNode);
            }
            // GCD Polygon
            private GCD_Result GCD_BruteForce(Vector2 localTarget, int indexStart, int indexEnd) {
                // Sub-track is point
                if (indexStart == indexEnd) return GCD_Point(localTarget, indexStart);

                // Compare distance of all of lines
                GCD_Result closest = InfResult;
                for (int i = indexStart; i != indexEnd; ++i) {
                    GCD_Result newClosest = GCD_Line(localTarget, i, i + 1);
                    if (newClosest.SqrDistance < closest.SqrDistance) {
                        closest = newClosest;
                        // Target on line
                        if(closest.SqrDistance < PlatformConstants.NearByZero) break;
                    }
                }
                return closest;
            }
            private GCD_Result GCD_GJK(Vector2 localTarget, int indexStart, int indexEnd) {
                // GJK is for polygon
                if(indexEnd - indexStart < 2) return new GCD_Result();

                int count = 0;
                int limit = (indexEnd - indexStart + 1) * GJK_LIMIT_FACTOR;

                bool isNotPolygon = m_nodes[indexStart] != m_nodes[indexEnd];
                // Except end node because overlapped with start node
                if (isNotPolygon == false) --indexEnd;
                
                m_simplex.Clear();
                m_simplex.Add(indexStart);  // Randomally determind the start point
                int lastOmit = -1;          // Check repeact same situation
                while (true) {
                    Vector2 closestPointToSimplex;   // Closest point from target to simplex
                    int furthestNodeToSimplex;

                    // Get closest point from target to simplex
                    {
                        if (m_simplex.Count == 1) { // Simplex is point
                            closestPointToSimplex = m_nodes[m_simplex[0]];
                        } else if (m_simplex.Count == 2) { // Simplex is line
                            GCD_Result result = GCD_Line(localTarget, m_simplex[0], m_simplex[1]);
                            closestPointToSimplex = result.Point;
                            // Remove unused node for closestPointToSimplex
                            if (result.OnPoint) {
                                m_simplex.Remove(result.ClosestNode);
                                lastOmit = m_simplex[0];
                                m_simplex.RemoveAt(0);
                                m_simplex.Add(result.ClosestNode);
                            }
                        } else { // Simplex is triangle
                            GCD_Result result = GCD_Triangle(localTarget, m_simplex[0], m_simplex[1], m_simplex[2]);
                            closestPointToSimplex = result.Point;
                            if (result.Success) {
                                if (result.OnPoint) {
                                    m_simplex.Remove(result.ClosestNode);
                                    lastOmit = m_simplex[0];
                                    m_simplex.RemoveAt(0);
                                    m_simplex.Add(result.ClosestNode);
                                } else {
                                    m_simplex.Remove(result.ClosestNode);
                                    m_simplex.Remove(result.OtherClosestNode);
                                    lastOmit = m_simplex[0];
                                    m_simplex.RemoveAt(0);
                                    m_simplex.Add(result.ClosestNode);
                                    m_simplex.Add(result.OtherClosestNode);
                                }
                            } else { // Target inside polygon;
                                return new GCD_Result();
                            }
                        }
                    }
                    // Get furthest node
                    {
                        Vector2 normal = localTarget - closestPointToSimplex;
                        if(normal != Vector2.zero) { // Target is outside simplex
                            float maxValue = Vector2.Dot(normal, m_nodes[indexStart]);
                            int maxPoint = indexStart;
                            for (int i = indexStart + 1; i <= indexEnd; ++i) {
                                float newValue = Vector2.Dot(normal, m_nodes[i]);
                                
                                if (maxValue < newValue) {
                                    maxValue = newValue;
                                    maxPoint = i;
                                }
                            }
                            furthestNodeToSimplex = maxPoint;
                        } else { // Target is on outline of simplex
                            furthestNodeToSimplex = m_simplex[0];
                        }
                    }
                    // Check simplex
                    {
                        if (furthestNodeToSimplex == lastOmit || m_simplex.Contains(furthestNodeToSimplex)) {
                            // At 2d, the closest point always on point or line
                            if (m_simplex.Count == 1) {
                                ref Vector2 closestNode = ref m_nodes[m_simplex[0]];
                                return new GCD_Result((localTarget - closestNode).sqrMagnitude, closestNode, m_simplex[0]);
                            } else {
                                if (m_simplex[0] - m_simplex[1] != 1
                                    || isNotPolygon && (m_simplex[0] == indexStart && m_simplex[1] == indexEnd || m_simplex[1] == indexStart && m_simplex[0] == indexEnd)) {
                                    return new GCD_Result();
                                }
                                return GCD_Line(localTarget, m_simplex[0], m_simplex[1]);
                            }
                        }
                        m_simplex.Add(furthestNodeToSimplex);
                    }

                    if (++count > limit) throw new System.TimeoutException("GJK Algorithm time-out");
                }
            }
            #endregion

            #endregion

#if UNITY_EDITOR
            #region DEBUG
            [Header("Debug")]
            [SerializeField] private bool m_visualGroup = true;
            [SerializeField] private Transform m_target;
            [SerializeField] private float m_nodeRadius = 0.1f;

            private void OnDrawGizmos() {
                if (m_nodes == null) return;
                Bake();
                if (m_nodes.Length < 1) return;
                
                DrawTrack();
                if (m_target == null) return;
                DrawClosetDistance();
            }

            private void DrawTrack() {
                // Draw node
                Gizmos.color = Color.gray;
                for (int n = 0; n != m_nodes.Length; ++n) {
                    Vector2 node = m_localZero + m_nodes[n];
                    Gizmos.DrawWireSphere(node, m_nodeRadius);
                }

                // Draw line
                if (m_visualGroup) {
                    Color colorA = Color.yellow;
                    Color colorB = Color.blue;
                    int delimeterCursor = 0;
                    for (int n = 0; n != m_nodes.Length - 1; ++n) {
                        if (m_delimeters[delimeterCursor] == n) {
                            Gizmos.color = delimeterCursor++ % 2 == 0 ? colorA : colorB;
                        }
                        Vector2 nodeA = m_localZero + m_nodes[n];
                        Vector2 nodeB = m_localZero + m_nodes[n + 1];

                        Gizmos.DrawLine(nodeA, nodeB);
                    }
                } else {
                    Gizmos.color = Color.white;
                    for (int n = 0; n != m_nodes.Length - 1; ++n) {
                        Vector2 nodeA = m_localZero + m_nodes[n];
                        Vector2 nodeB = m_localZero + m_nodes[n + 1];
                        Gizmos.DrawLine(nodeA, nodeB);
                    }
                }
            }

            private void DrawClosetDistance() {
                GCD_Result result = GetClosest(m_target.position);
                Vector2 point = result.Point + m_localZero;
                Gizmos.color = Color.red;
                Gizmos.DrawLine(point, m_target.position);

                if (result.OnPoint) {
                    Gizmos.color = Color.cyan;
                    Vector2 node = m_localZero + m_nodes[result.ClosestNode];
                    Gizmos.DrawWireSphere(node, m_nodeRadius);
                } else {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(m_nodes[result.ClosestNode] + m_localZero, m_nodes[result.OtherClosestNode] + m_localZero);
                }
            }
            #endregion
#endif
        }

    }
}
