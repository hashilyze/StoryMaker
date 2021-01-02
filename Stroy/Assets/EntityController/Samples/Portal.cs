using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Stroy {
    namespace EC.Samples {
        [RequireComponent(typeof(Collider2D))]
        public class Portal : MonoBehaviour {
            public Portal PairPortal;
            private Collider2D triggerZone;
            private HashSet<EntityController> arrivedEntity = new HashSet<EntityController>();

            private void Awake() {
                triggerZone = GetComponent<Collider2D>();
                triggerZone.isTrigger = true;
            }

            private void OnTriggerEnter2D(Collider2D collision) {
                if (collision.gameObject.layer == ECConstant.Entity) {
                    EntityController ec = collision.GetComponent<EntityController>();

                    if (arrivedEntity.Contains(ec)) return;

                    ec.SetPosition(PairPortal.transform.position);
                    PairPortal.arrivedEntity.Add(ec);
                }
            }
            private void OnTriggerExit2D(Collider2D collision) {
                if (collision.gameObject.layer == ECConstant.Entity) {
                    EntityController ec = collision.GetComponent<EntityController>();
                    arrivedEntity.Remove(ec);
                }
            }
        }
    }
}