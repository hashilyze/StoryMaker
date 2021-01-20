using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stroy {
    namespace Platforms.Sample {
        public class SimpleAgent : DynamicPlatformAgent {
            public Vector2 Velocity;

            public override void HandlePlatform() {
                Platform.Rigidbody.velocity = Velocity;
            }
        }
    }
}
