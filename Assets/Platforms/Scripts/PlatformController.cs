using UnityEngine;


namespace Stroy.Platforms {
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlatformController : MonoBehaviour {
        #region Public
        public Rigidbody2D Rigidbody => m_rigidbody;

        public Vector2 Velocity { get => m_velocity; set => m_velocity = value; }
        

        public Track JointedTrack { get => m_track; set => Joint(value); }
        public float FollowSpeed { get => m_followSpeed; set => m_followSpeed = value; }
        public bool IsAsscending { get => m_asscending; set => SetFollowDirection(value); }

        
        public void Joint(Track track) {
            // Remove track
            if (track == null) {
                m_isFollow = false;
                m_track = null;
                return;
            }

            m_isFollow = true;
            m_track = track;

            GCD_Result result = m_track.GetClosest(transform.position);
            transform.position = result.Point + m_track.LocalZero;
            
            int size = m_track.Size;
            // Get current node
            if (result.OnPoint) {
                m_destIndex = result.ClosestNode;
            } else {
                if (m_track.IsCycle && Mathf.Abs(result.ClosestNode - result.OtherClosestNode) != 1) {
                    m_destIndex = m_asscending ? size - 1 : 0;
                } else {
                    var comparer = m_asscending ? (System.Func<int, int, int>)Mathf.Min : Mathf.Max;
                    m_destIndex = comparer(result.ClosestNode, result.OtherClosestNode);
                }
            }
            UpdateNode();
        }
        #endregion

        #region Private
        // Component
        [HideInInspector] private Rigidbody2D m_rigidbody;
        // Free State
        [SerializeField] private Vector2 m_velocity;
        // Follow state
        [SerializeField] private bool m_isFollow;               // Follow track mode
        [SerializeField] private Track m_track;                 // Current jointed track
        [SerializeField] private float m_followSpeed;           // Follow speed
        [SerializeField] private bool m_asscending = true;      // If true, Next node index greater than current node
        private int m_destIndex;                                // Destination node's index
        private Vector2 m_destPos;                              // World position of destination node


        private void Awake() {
            m_rigidbody = GetComponent<Rigidbody2D>();
        }
        private void Start() {
            if (m_isFollow && m_track != null) {
                Joint(m_track);
            }
        }

        private void FixedUpdate() {
            float deltaTime = Time.fixedDeltaTime;
            if (m_isFollow) FollowMove(deltaTime);
            else FreeMove(deltaTime);
        }

        private void FreeMove(float deltaTime) {
            m_rigidbody.position = m_velocity * deltaTime;
        }
        private void FollowMove(float deltaTime) {
            Vector2 current = m_rigidbody.position;
            if (current == m_destPos) {
                UpdateNode();
            }

            Vector2 destination = Vector2.MoveTowards(current, m_destPos, m_followSpeed * deltaTime);
            m_velocity = (destination - current) / deltaTime;
            m_rigidbody.position = destination;
        }
        private void UpdateNode() {
            if (m_track.IsCycle) {
                m_destIndex = (m_destIndex + m_track.Size + (m_asscending ? 1 : -1)) % m_track.Size;
            } else {
                if (m_asscending && m_destIndex == m_track.Size - 1) m_asscending = false;
                else if (!m_asscending && m_destIndex == 0) m_asscending = true;

                if (m_asscending) ++m_destIndex;
                else --m_destIndex;
            }
            m_destPos = m_track.GetNode(m_destIndex) + m_track.LocalZero;
        }

        private void SetFollowDirection(bool asscending) {
            if (m_asscending == asscending) return;

            m_asscending = asscending;
            UpdateNode();
        }
        #endregion
    }
}
