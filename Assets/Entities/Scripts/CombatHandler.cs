using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stroy.Entities {
    public abstract class CombatHandler : MonoBehaviour {
        public abstract void Combat(Character character);
        public abstract bool Monitor(Character character);
    }
}
