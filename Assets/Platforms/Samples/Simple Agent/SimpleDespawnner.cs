using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Stroy {
    namespace Platforms.Sample {
        public class SimpleDespawnner : MonoBehaviour {
            private void OnTriggerEnter2D(Collider2D collision) {
                Destroy(collision.gameObject);
            }
        }
    }
}