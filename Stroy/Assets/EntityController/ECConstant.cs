using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Stroy {

    namespace EC {
        /// <summary>
        /// Configurations for EC package
        /// </summary>
        public static class ECConstant {
            public const float MinContactOffset = 0.01f;
            public const float MaxContactOffset = 0.02f;

            public static int BlockMask => 0x01 << GameLayer.StaticBlock | 0x01 << GameLayer.DynamicBlock;

            public static int StaticBlock => GameLayer.StaticBlock;
            public static int DynamicBlock => GameLayer.DynamicBlock;
            public static int Entity => GameLayer.Entity;

        }
    }
}
