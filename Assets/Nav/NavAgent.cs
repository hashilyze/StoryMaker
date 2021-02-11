using UnityEngine;

namespace Stroy.Nav {
    public abstract class NavAgent : MonoBehaviour {
        public abstract void SetDestination(Vector2 destination);
        public abstract void Pause();
    }
}
