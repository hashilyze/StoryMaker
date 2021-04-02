using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StroyMaker.Physics {
    public class PhysicsUtility : MonoBehaviour {
        #region Platform Prediction
        public static bool IsStaticBlock (GameObject gameObject) => gameObject.layer == PhysicsConstants.k_Layer_StaticBlock;
        public static bool IsDynamicBlock (GameObject gameObject) => gameObject.layer == PhysicsConstants.k_Layer_DynamicBlock;
        public static bool IsStaticOneway (GameObject gameObject) => gameObject.layer == PhysicsConstants.k_Layer_StaticOneway;
        public static bool IsDynamicOneway (GameObject gameObject) => gameObject.layer == PhysicsConstants.k_Layer_DynamicOneway;

        public static bool IsStatic (GameObject gameObject) => ((0x01 << gameObject.layer) & PhysicsConstants.k_Mask_Static) != 0;
        public static bool IsDynamic (GameObject gameObject) => ((0x01 << gameObject.layer) & PhysicsConstants.k_Mask_Dynamic) != 0;
        public static bool IsBlock (GameObject gameObject) => ((0x01 << gameObject.layer) & PhysicsConstants.k_Mask_Block) != 0;
        public static bool IsOneway (GameObject gameObject) => ((0x01 << gameObject.layer) & PhysicsConstants.k_Mask_Oneway) != 0;
        public static bool IsPlatform (GameObject gameObject) => ((0x01 << gameObject.layer) & PhysicsConstants.k_Mask_Platform) != 0;
        #endregion

        #region Entity Prediction
        public static bool IsPlayer (GameObject gameObject) => gameObject.layer == PhysicsConstants.k_Layer_Player;
        public static bool IsFriend (GameObject gameObject) => gameObject.layer == PhysicsConstants.k_Layer_Friend;
        public static bool IsEnemy (GameObject gameObject) => gameObject.layer == PhysicsConstants.k_Layer_Enemy;
        public static bool IsNeutral (GameObject gameObject) => gameObject.layer == PhysicsConstants.k_Layer_Neutral;

        public static bool IsEntity (GameObject gameObject) => ((0x01 << gameObject.layer) & PhysicsConstants.k_Mask_Entity) != 0;
        #endregion
    }
}