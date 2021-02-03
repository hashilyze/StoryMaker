using UnityEngine;

namespace Stroy.Entities {
    public static class EntityConstants {
        #region Contact Configures
        public const float MinContactOffset = 0.01f;
        public const float MaxContactOffset = 0.02f;
        public const float CastBuffer = 0.005f;
        #endregion

        #region Tag & Layer
        public const string T_Player = "Player";
        public const string T_Enemy = "Enemy";

        public static readonly int L_Player = LayerMask.NameToLayer("Player");        
        public static readonly int L_Friend = LayerMask.NameToLayer("Friend");
        public static readonly int L_Enemy = LayerMask.NameToLayer("Enemy");

        public static readonly int L_EntityMask = 0x01 << L_Player | 0x01 << L_Friend | 0x01 << L_Enemy;
        #endregion

        #region Entity Configures
        public const float Gravity = 10f;
        public const float FallLimit = 12f;
        public const float SlopeLimit = 50f;
        public const float WallLimit = 0.1f;

        public const float DefaultGravityScale = 1f;
        public const float FallGravityScale = 1.5f;
        #endregion
    }
}