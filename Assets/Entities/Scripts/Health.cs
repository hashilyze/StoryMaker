using UnityEngine;

namespace Stroy.Entities {
    public class Health : MonoBehaviour {
        public float Hp => m_hp;
        public bool Invincible { get => m_invincible; set => SetInvincible(value); }

        public System.Action<Health> OnDie;
        public System.Action<Health, float> OnDamaged;
        public System.Action<Health, float> OnHealed;
        public System.Action<Health> OnInvincibled;
        public System.Action<Health> OffInvincibled;


        public void SetHp(float hp) {
            m_hp = hp;
            if (hp < 0f) OnDie?.Invoke(this);
        }
        public void SetInvincible(bool active) {
            if(m_invincible ^ active) {
                m_invincible = active;
                if (active) OnInvincibled?.Invoke(this);
                else OffInvincibled?.Invoke(this);
            }
        }

        public void TakeDamage(float damage) {
            if (m_invincible) return;
            
            SetHp(m_hp - damage);
            OnDamaged?.Invoke(this, damage);
        }
        public void Heal(float heal) {
            SetHp(m_hp + heal);
            OnHealed?.Invoke(this, heal);
        }
        

        [SerializeField] private float m_hp;
        [SerializeField] private bool m_invincible;
    }
}