using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stroy {
    namespace Platforms {
        // Do not modify in play-mode
        public class DynamicPlatformTrack : MonoBehaviour {
            // Track state
            public int Size => m_nodes.Length;
            public Vector2 GetNode(int index) => m_nodes[index];
            public Vector2[] GetNodes => m_nodes.Clone() as Vector2[];
            public bool IsPoint => m_nodes.Length == 1;

            
            public Vector2 GetClosetPoint(Vector2 point) {
                if(m_nodes.Length < 1) { // No track
                    throw new System.NullReferenceException();
                } else if(m_nodes.Length == 1) { // Track is a point
                    GCD_Result result = GCD_Point(point - m_localZero, 0);
                    return result.Point + m_localZero;
                } else { // Track is a multi-line
                    Vector2 localTarget = point - m_localZero;
                    Vector2 bruteforce;

                    {
                        GCD_Result closest = GCD_BruteForce(localTarget, 0, m_nodes.Length - 1);
                        bruteforce = closest.Point;
                    }
                    {
                        GCD_Result closest;
                        if (m_delimeters[1] - m_delimeters[0] < 2) {
                            closest = GCD_BruteForce(localTarget, m_delimeters[0], m_delimeters[1]);
                        } else {
                            closest = GCD_GJK(localTarget, m_delimeters[0], m_delimeters[1]);
                            if (!closest.Success) {
                                closest = GCD_BruteForce(localTarget, m_delimeters[0], m_delimeters[1]);
                            }
                        }
                        for (int d = 1, dEnd = m_delimeters.Count - 1; d != dEnd; ++d) {
                            GCD_Result newClosest;
                            if (m_delimeters[d + 1] - m_delimeters[d] < 2) {
                                newClosest = GCD_BruteForce(localTarget, m_delimeters[d], m_delimeters[d + 1]);
                            } else {
                                newClosest = GCD_GJK(localTarget, m_delimeters[d], m_delimeters[d + 1]);
                                if (newClosest.Success == false) {
                                    newClosest = GCD_BruteForce(localTarget, m_delimeters[d], m_delimeters[d + 1]);
                                }
                            }

                            if (newClosest.SqrDistance < closest.SqrDistance) {
                                closest = newClosest;
                                if(closest.SqrDistance == 0f) break;
                            }
                        }
                        if (bruteforce != closest.Point) throw new System.Exception();

                        return closest.Point + m_localZero;
                    }
                    
                }
            }
            // ---------------------------------------------------------------------------------------
            // State
            [SerializeField] private Vector2[] m_nodes;                     // Node which is vertex of lines
            private readonly List<int> m_delimeters = new List<int>();      // Groupping line's start and end nodes
            private Vector2 m_localZero;                                    // Cached world position of track
            private bool m_isCycle;
            private readonly List<int> m_simplex = new List<int>(3);

            private void Initialize() {
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

            #region Support
            [ContextMenu("Check Overlap")]
            private void RemoveOverlappedPoints() {
                if (IsPoint) return;

                List<int> newNodeIndex = new List<int>();
                HashSet<Vector2> nodeSet = new HashSet<Vector2>();
                
                for(int n = 0, nLen = m_nodes.Length; n != nLen; ++n) {
                    if (nodeSet.Contains(m_nodes[n])) {
                        continue;
                    }
                    newNodeIndex.Add(n);
                    nodeSet.Add(m_nodes[n]);
                }
                if(newNodeIndex.Count != m_nodes.Length) {
                    Vector2[] newNodes = new Vector2[newNodeIndex.Count];
                    for(int n = 0, nLen = newNodeIndex.Count; n != nLen; ++n) {
                        newNodes[n] = m_nodes[newNodeIndex[n]];
                    }
                    m_nodes = newNodes;
                }
            }
            #endregion

            #region GCD: GetClosestDistance
            private readonly struct GCD_Result {
                public GCD_Result(float sqrDistance, Vector2 point, int closestNode, int otherClosestNode = EmptyNode) {
                    Success = true;
                    SqrDistance = sqrDistance;
                    Point = point;
                    ClosestNode = closestNode;
                    OtherClosestNode = otherClosestNode;
                    OnPoint = otherClosestNode == EmptyNode;
                }

                public const int EmptyNode = -1;

                public readonly bool Success;               // Find closest distance
                public readonly float SqrDistance;          // Squared the closest distance
                public readonly Vector2 Point;              // The closest point
                public readonly int ClosestNode;            // The closest node from point
                public readonly int OtherClosestNode;       // Other closest node; it only use point on line
                public readonly bool OnPoint;               // The closest point on node, not line
            }

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
                // Clockwise of lines are same
                Vector2 lineA = nodeB - nodeA;
                Vector2 lineB = nodeC - nodeB;
                Vector2 lineC = nodeA - nodeC;

                float area = Vector3.Cross(lineA, lineB).z;
                float areaRateA = Vector3.Cross(lineA, localTarget - nodeA).z / area;
                float areaRateB = Vector3.Cross(lineB, localTarget - nodeB).z / area;
                float areaRateC = Vector3.Cross(lineC, localTarget - nodeC).z / area;

                // Point inside polygon
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

            private GCD_Result GCD_BruteForce(Vector2 localTarget, int indexStart, int indexend) {
                // Sub-track is point
                if (indexStart == indexend) return GCD_Point(localTarget, indexStart);

                // Compare distance of all of lines
                GCD_Result closestResult = GCD_Line(localTarget, indexStart, indexStart + 1);
                for (int p = indexStart + 1; p != indexend; ++p) {
                    GCD_Result newResult = GCD_Line(localTarget, p, p + 1);
                    if (newResult.SqrDistance < closestResult.SqrDistance) {
                        closestResult = newResult;
                        // Target on line
                        if(closestResult.SqrDistance == 0f) break;
                    }
                }
                return closestResult;
            }
            private GCD_Result GCD_GJK(Vector2 localTarget, int pointStart, int pointEnd) {
                // GJK compute only polygone
                if(pointEnd - pointStart < 2) return new GCD_Result();

                int count = 0;
                bool isNotPolygon = m_nodes[pointStart] != m_nodes[pointEnd];
                // Except end node because overlapped with start node
                if (isNotPolygon == false) --pointEnd;
                
                m_simplex.Clear();
                m_simplex.Add(pointStart);
                int lastOmit = -1;
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
                            } else {
                                return new GCD_Result();
                            }
                        }
                    }
                    // Get furthest node
                    {
                        Vector2 normal = localTarget - closestPointToSimplex;
                        float maxValue = Vector2.Dot(normal, m_nodes[pointStart]);
                        int maxPoint = pointStart;
                        for (int p = pointStart + 1, pEnd = pointEnd + 1; p != pEnd; ++p) {
                            float newValue = Vector2.Dot(normal, m_nodes[p]);

                            if (maxValue < newValue) {
                                maxValue = newValue;
                                maxPoint = p;
                            }
                        }
                        furthestNodeToSimplex = maxPoint;
                    }
                    // If the closest point already was added to simplex, that mean current simplex is the closest simplex from target
                    if (furthestNodeToSimplex == lastOmit || m_simplex.Contains(furthestNodeToSimplex)) {
                        // At 2d, the closest point always on point or line
                        if(m_simplex.Count == 1) { 
                            ref Vector2 closestNode = ref m_nodes[m_simplex[0]];
                            return new GCD_Result((localTarget - closestNode).sqrMagnitude, closestNode, m_simplex[0]);
                        } else { 
                            if(m_simplex[0] - m_simplex[1] != 1
                                || isNotPolygon && (m_simplex[0] == pointStart && m_simplex[1] == pointEnd || m_simplex[1] == pointStart && m_simplex[0] == pointEnd)) {
                                return new GCD_Result();
                            }
                            return GCD_Line(localTarget, m_simplex[0], m_simplex[1]);
                        }
                    }
                    m_simplex.Add(furthestNodeToSimplex);
                    if (++count > 100) throw new System.NotImplementedException();
                }
            }
            #endregion

            #region DEBUG
#if UNITY_EDITOR
            [SerializeField] private Transform m_target;

            [ContextMenu("ShowDelimeter")]
            private void ShowDelimeter() {
                string line = "";
                for(int d = 0, dLen = m_delimeters.Count; d != dLen; ++d) {
                    line += m_delimeters[d] + "\n";
                }
                Debug.Log(line);
            }

            private void OnDrawGizmos() {
                Initialize();
                if (m_nodes == null) return;
                DrawTrack();
                if (m_target == null) return;
                DrawClosetDistance();
            }

            private void DrawTrack() {
                // Draw node
                Gizmos.color = Color.gray;
                float radius = 0.1f;
                for (int n = 0; n != m_nodes.Length; ++n) {
                    Vector2 node = m_localZero + m_nodes[n];
                    Gizmos.DrawWireSphere(node, radius);
                }
                // Drwa line
                Gizmos.color = Color.white;
                Color colorA = Color.yellow;
                Color colorB = Color.blue;

                int delimeterCursor = 0;
                for (int n = 0; n != m_nodes.Length - 1; ++n) {
                    if(m_delimeters[delimeterCursor] == n) {
                        Gizmos.color = delimeterCursor++ % 2 == 0 ? colorA : colorB;
                    }
                    Vector2 nodeA = m_localZero + m_nodes[n];
                    Vector2 nodeB = m_localZero + m_nodes[n + 1];

                    Gizmos.DrawLine(nodeA, nodeB);
                }
            }

            private void DrawClosetDistance() {
                Vector2 point = GetClosetPoint(m_target.position);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(point, m_target.position);
            }
#endif
            #endregion
        }

    }
}
