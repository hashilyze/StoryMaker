using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Stroy.Entities {
    public class WalkHandler : MovementHandler {
        public SpriteRenderer m_spriteRenderer;
        public Animator m_animator;

        public float Speed;

        private void Awake() {
            m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            m_animator = GetComponentInChildren<Animator>();
        }

        public override void HandleMovement(Character character, float deltaTIme) {
            if (character.IsWall) {
                if (character.Face > 0f && character.WallOnRight) {
                    character.Run(-Speed);
                    m_spriteRenderer.flipX = true;
                } else if (character.Face < 0f && character.WallOnLeft) {
                    character.Run(Speed);
                    m_spriteRenderer.flipX = false;
                }
                return;
            }
            if (character.Face < 0f) {
                character.Run(-Speed);
            } else {
                character.Run(Speed);
            }
            m_animator.SetBool("IsGround", character.IsGround);
            m_animator.SetFloat("HorVelocity", Speed);
        }
    }
}
