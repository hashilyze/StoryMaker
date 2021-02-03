using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stroy.Entities {
    public class Turret : MonoBehaviour {
        public Bullet bullet;
        public float duration;

        private float m_nextShoot;
        private float m_elapsedShoot;


        private void FixedUpdate() {
            m_elapsedShoot += Time.fixedDeltaTime;

            if(m_elapsedShoot > m_nextShoot) {
                m_nextShoot = m_elapsedShoot + duration;

                Bullet newBullet = Instantiate(bullet, transform.position, Quaternion.identity);
                newBullet.TargetTag = EntityConstants.T_Player;
            }
        }
    }
}
