using UnityEngine;

namespace Stroy.Entity {
    public static class EntityConstants {
        #region Contact Configures
        public const float MinContactOffset = 0.01f;
        public const float MaxContactOffset = 0.02f;
        #endregion

        #region Tag & Layer
        public const string T_Player = "Player";

        public static readonly int L_Entity = LayerMask.NameToLayer("Entity");
        #endregion

        #region Character Configures
        public const float Gravity = 10f;
        public const float FallLimit = 12f;
        public const float SlopeLimit = 50f;
        public const float WallLimit = 0.1f;

        public const float DefaultGravityScale = 1f;
        public const float FallGravityScale = 1.5f;
        #endregion
    }
}