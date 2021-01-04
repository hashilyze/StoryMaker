using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Stroy {
    namespace EC.Samples {
        [RequireComponent(typeof(Collider2D))]
        public class Resizer : MonoBehaviour {
            public Vector2 size;

            private Collider2D triggerZone;

            private void Awake() {
                triggerZone = GetComponent<Collider2D>();
                triggerZone.isTrigger = true;
            }

            private void OnTriggerEnter2D(Collider2D collision) {
                if (collision.gameObject.layer == ECConstants.Entity) {
                    EntityController ec = collision.GetComponent<EntityController>();
                    SpriteRenderer sprite = collision.GetComponent<SpriteRenderer>();

                    sprite.size = size;
                    ec.SetSize(size);
                    
                }
            }
        }
    }
}
