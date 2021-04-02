using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StroyMaker.Framework;

namespace StroyMaker.Physics {
    public class CharacterHolder : ComponentHolder<KinematicCharacterController2D> {
        protected override Dictionary<int, KinematicCharacterController2D> CreateTable () {
            return new Dictionary<int, KinematicCharacterController2D>(PhysicsConstants.k_CharacterLimit);
        }
    }
}