using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Stroy.Entities {
    [RequireComponent(typeof(Character))]
    public class Npc : MonoBehaviour {
        public MovementHandler MovementHandler { get => m_movementHandler; set => m_movementHandler = value; }
        public CombatHandler CombatHandler { get => m_combatHandler; set => m_combatHandler = value; }

        private Character m_character;
        [SerializeField] private MovementHandler m_movementHandler;
        [SerializeField] private CombatHandler m_combatHandler;

        private void Awake() {
            m_character = GetComponent<Character>();
        } 
        private void Start() {
            m_character.Health.OnDie += (x) => Destroy(gameObject);
        }
        private void Update() {
            float deltaTime = Time.deltaTime;
            if (MovementHandler != null) MovementHandler.HandleMovement(m_character, deltaTime);
            if (CombatHandler != null) CombatHandler.HandleCombat(deltaTime);
        }
    }
}
