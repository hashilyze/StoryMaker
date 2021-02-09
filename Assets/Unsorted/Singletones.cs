using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stroy {
    public class Singletones : MonoBehaviour {
        private void Awake() { DontDestroyOnLoad(gameObject); }
    }
}
