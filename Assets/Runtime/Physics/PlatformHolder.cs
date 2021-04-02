using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StroyMaker.Framework;

namespace StroyMaker.Physics {
    public class PlatformHolder : ComponentHolder<KinematicPlatformController2D>{
        protected override Dictionary<int, KinematicPlatformController2D> CreateTable () {
            return new Dictionary<int, KinematicPlatformController2D>(PhysicsConstants.k_PlatformLimit);
        }
    }
}