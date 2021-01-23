using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Stroy {
    namespace Platform {
        public static class PlatformConstants {
            public const float NearByZero = 0.001f;

            #region Platform Tag & Layer
            public const string T_Belt = "Belt";

            public const int L_StaticBlock = 8;
            public const int L_DynamicBlock = 9;
            public const int L_BlockMask = 0x01 << L_StaticBlock | 0x01 << L_DynamicBlock;
            #endregion

            #region Effector
            public static bool IsAttackPlatform(string tag) {
                throw new System.NotImplementedException();
            }
            public static bool IsStepPlatform(string tag) {
                return tag == T_Belt;
            }
            public static bool IsGrabPlatform(string tag) {
                throw new System.NotImplementedException();
            }
            public static bool IsHeadingPlatform(string tag) {
                throw new System.NotImplementedException();
            }
            #endregion
        }
    }
}