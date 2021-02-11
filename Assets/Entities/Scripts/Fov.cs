using UnityEngine;


namespace Stroy.Entities {
    public abstract class Fov : MonoBehaviour {
        public abstract int Monitor(int targetMask, int obstacleMask, Collider2D[] results);
        public abstract bool IsVisible(Collider2D target, int obstacleMask);
    }
}
