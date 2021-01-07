namespace Stroy {

    namespace EC {
        public static class ECConstants {
            /// <summary>Min offset for EC to interact with others</summary>
            public const float MinContactOffset = 0.01f;
            /// <summary>Max offset for EC to interact with others</summary>
            public const float MaxContactOffset = 0.02f;

            
            /// <summary>Layer of static block</summary>
            public static int StaticBlock => GameLayer.StaticBlock;
            /// <summary>Layer of dynamic block</summary>
            public static int DynamicBlock => GameLayer.DynamicBlock;
            /// <summary>Layer of entity</summary>
            public static int Entity => GameLayer.Entity;

            public static int BlockMask => 0x01 << StaticBlock | 0x01 << DynamicBlock;


            ///<summary>Default gravity scale</summary>
            public const float Gravity = 10f;
            ///<summary>Max degree of climbable slope</summary>
            public const float SlopeLimit = 50f;
            /// <summary>Max fall speed</summary>
            public const float FallLimit = -12f;

        }
    }
}
