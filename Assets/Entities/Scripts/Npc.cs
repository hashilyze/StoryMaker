using UnityEngine;

namespace Stroy.Entities {
    public enum EState { Idle, Travel, Follow, Combat }

    [RequireComponent(typeof(Character))]
    public class Npc : MonoBehaviour {
        public Character Character => m_character;
        public EState State { get => m_state; set => m_state = value; }

        [SerializeField] private NpcData m_data;
        [SerializeField] private Character m_character;
        [SerializeField] private EState m_state;

        // Activation
        private void Reset() {
            m_character = GetComponent<Character>();
        }
        protected virtual void Update() {
            UpdateTransaction();
            UpdateState();
        }
        protected virtual void UpdateTransaction() { }
        protected virtual void UpdateState() { }
    }
}