using UnityEngine;

namespace Stroy.Entities {
    [RequireComponent(typeof(EntityController))]
    public class Entity : MonoBehaviour {
        public EntityData Data;
        public EntityState State;
        public EntityController Controller => m_controller;

        private EntityController m_controller;

        protected virtual void Awake() {
            m_controller = GetComponent<EntityController>();
        }
    }
}
