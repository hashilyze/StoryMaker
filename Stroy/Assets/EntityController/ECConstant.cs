using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Stroy {

    namespace EC {
        public static class ECConstant {
            public const float MinContactOffset = 0.01f;
            public const float MaxContactOffset = 0.02f;

            public const float Gravity = 10f;
            public const float FALL_SPEED = -12f;

            public const float MaxSlopeDeg = 80f;
            public const float MaxSlopeRad = MaxSlopeDeg * Mathf.Deg2Rad;

            public static int BlockMask => 0x01 << GameLayer.StaticBlock | 0x01 << GameLayer.DynamicBlock;

            public static int StaticBlock => GameLayer.StaticBlock;
            public static int DynamicBlock => GameLayer.DynamicBlock;
            public static int Entity => GameLayer.Entity;

        }
    }
}
