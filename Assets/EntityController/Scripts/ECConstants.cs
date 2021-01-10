namespace Stroy { 
   namespace EC { 
       public static class ECConstants { 
           public const int StaticBlock = 9; 
           public const int DynamicBlock = 10; 
           public const int Entity = 8; 
           public const int BlockMask = 0x01 << StaticBlock | 0x01 << DynamicBlock; 

           public const float MinContactOffset = 0.01f; 
           public const float MaxContactOffset = 0.02f; 

           public const float Gravity = 10f; 
           public const float FallLimit = 12f;
           public const float SlopeLimit = 50f; 
       } 
   } 
} 
