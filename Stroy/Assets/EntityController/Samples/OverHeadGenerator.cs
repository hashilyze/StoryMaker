using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stroy.EC.Samples {
    public class OverHeadGenerator : MonoBehaviour {
        public int Number;


        void Update() {
            GameObject[] gos = new GameObject[Number];
            for(int n = 0; n != Number; ++n) {
                gos[n] = new GameObject();
            }
            for(int n = 0; n != Number; ++n) {
                Destroy(gos[n]);
            }
        }
    }
}