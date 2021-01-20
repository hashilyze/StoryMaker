namespace Stroy { 
	namespace EC { 
		public static class ECConstants{ 
			public const int StaticBlock = 8; 
			public const int DynamicBlock = 9; 
			public const int Entity = 10; 
			public const int BlockMask = 768; 

			public const float MinContactOffset = 0.01f; 
			public const float MaxContactOffset = 0.02f; 

			public const float Gravity = 10f; 
			public const float FallLimit = 12f; 
			public const float SlopeLimit = 50f; 
			public const float WallLimit = 0.1f; 

			public const float DefaultGravityScale = 1f; 
			public const float FallGravityScale = 1.5f; 
		} 
	} 
} 
