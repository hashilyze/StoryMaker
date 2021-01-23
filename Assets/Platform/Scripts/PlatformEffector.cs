using UnityEngine;
using Stroy.Entity;

namespace Stroy {
    namespace Platform {
        public class PlatformEffector : MonoBehaviour {
            public virtual void OnEffect(Character character) { Debug.Log("On Effect"); }
            public virtual void OffEffect(Character character) { }
        }
    }
}
