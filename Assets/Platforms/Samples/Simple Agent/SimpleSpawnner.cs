using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Stroy {
    namespace Platforms.Sample {
        public class SimpleSpawnner : MonoBehaviour {
            public float Duraction;
            public Vector2 Velocity;
            public SimpleAgent Prefab;

            private float m_nextSpwanTime;
            private float m_elapsedTime;

            public void Update() {
                m_elapsedTime += Time.deltaTime;

                if(m_elapsedTime >= m_nextSpwanTime) {
                    m_nextSpwanTime += Duraction;

                    SimpleAgent newAgent = Instantiate(Prefab, transform.position, Quaternion.identity);
                    newAgent.Velocity = Velocity;
                }
            }
        }
    }
}
