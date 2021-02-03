using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stroy.Entities {
    public abstract class MovementHandler : MonoBehaviour {
        public abstract void HandleMovement(Character character, float deltaTIme);
    }
}