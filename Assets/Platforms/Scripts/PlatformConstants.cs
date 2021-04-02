using UnityEngine;

namespace Stroy.Platforms {
    public static class PlatformConstants {
        public const float NearByZero = 0.001f;

        #region Platform Tag & Layer
        public static readonly int L_StaticBlock = LayerMask.NameToLayer("Static Block");
        public static readonly int L_DynamicBlock = LayerMask.NameToLayer("Dynamic Block");
        public static readonly int L_BlockMask = 0x01 << L_StaticBlock | 0x01 << L_DynamicBlock;
        #endregion
    }
}