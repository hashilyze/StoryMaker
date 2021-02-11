using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Stroy.Entities {
    public class Npc_Fish : Npc {
        [SerializeField] private float m_speed;
        private float m_dir = 1f;
        [SerializeField] private float m_visibleRangle;
        [SerializeField] private Transform m_target;


        protected override void UpdateTransaction() {
            switch (State) {
            case EState.Travel:
                if(Vector2.SqrMagnitude(m_target.position - transform.position) < m_visibleRangle * m_visibleRangle) {
                    State = EState.Follow;
                }
                break;
            case EState.Follow:
                if (Vector2.SqrMagnitude(m_target.position - transform.position) > m_visibleRangle * m_visibleRangle) {
                    State = EState.Travel;
                }
                break;
            }
        }

        protected override void UpdateState() {
            switch(State){
                case EState.Travel:
                if (Character.IsWall && (Character.WallOnLeft && m_dir < 0f || Character.WallOnRight && m_dir > 0f)) {
                    m_dir *= -1f;
                }
                Character.Run(m_speed * m_dir);
                break;
            case EState.Follow:
                if(transform.position.x < m_target.position.x) {
                    m_dir = 1f;
                } else{
                    m_dir = -1f;
                }
                Character.Run(m_speed * m_dir);
                break;
            default:
                Character.Run(0f);
                break;
            }
        }
    }
}
