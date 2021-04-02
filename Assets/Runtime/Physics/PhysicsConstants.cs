using UnityEngine;

namespace StroyMaker.Physics {
    public static class PhysicsConstants {
        // --- Object type ---
        // World Static (Never change transform)
        public static readonly int k_Layer_StaticBlock = LayerMask.NameToLayer("Static Block");
        public static readonly int k_Layer_StaticOneway = LayerMask.NameToLayer("Static Oneway");
        // World Dynamic (Chagne transform by script or animation)
        public static readonly int k_Layer_DynamicBlock = LayerMask.NameToLayer("Dynamic Block");
        public static readonly int k_Layer_DynamicOneway = LayerMask.NameToLayer("Dynamic Oneway");
        // Character (Control by player or ai)
        public static readonly int k_Layer_Player = LayerMask.NameToLayer("Player");
        public static readonly int k_Layer_Enemy = LayerMask.NameToLayer("Enemy");
        public static readonly int k_Layer_Friend = LayerMask.NameToLayer("Friend");
        public static readonly int k_Layer_Neutral = LayerMask.NameToLayer("Neutral");

        // --- Object Limit ---
        public const int k_PlatformLimit = 127;
        public const int k_CharacterLimit = 127;


        // --- Object layer mask ---
        public static readonly int k_Mask_Static = 0x01 << k_Layer_StaticBlock | 0x01 << k_Layer_StaticOneway;
        public static readonly int k_Mask_Dynamic = 0x01 << k_Layer_DynamicBlock | 0x01 << k_Layer_DynamicOneway;
        public static readonly int k_Mask_Block = 0x01 << k_Layer_StaticBlock | 0x01 << k_Layer_DynamicBlock;
        public static readonly int k_Mask_Oneway = 0x01 << k_Layer_StaticOneway | 0x01 << k_Layer_DynamicOneway;
        public static readonly int k_Mask_Platform = 0x01 << k_Layer_StaticBlock | 0x01 << k_Layer_StaticOneway | 0x01 << k_Layer_DynamicBlock | 0x01 << k_Layer_DynamicOneway;

        public static readonly int k_Mask_Entity = 0x01 << k_Layer_Player | 0x01 << k_Layer_Friend | 0x01 << k_Layer_Enemy | 0x01 << k_Layer_Neutral;


        // --- Character Controller Preset ---
        // Movement
        /// <summary>Max interation count to resolve velocity</summary>
        public const int k_MaxIteration = 4;
        /// <summary>Min distance to resolve velocity; if lesser than it, stop to resolve</summary>
        public const float k_MinDistance = 0.001f;
        // Gravity
        /// <summary>Default gravity</summary>
        public const float k_Gravity = 10f;
        /// <summary>Default gravity scale</summary>
        public const float k_DefaultGravityScale = 1f;
        /// <summary>Gravity scale applied when cct fall</summary>
        public const float k_FallGravityScale = 1.5f;

        // --- Query ---
        /// <summary>Min contact offset; if MTD is lesser than it, overlaped</summary>
        public const float k_MinContactOffset = 0.01f;
        /// <summary>Max contact offset; Max distance to detect</summary>
        public const float k_MaxContactOffset = k_MinContactOffset * 2f;
        /// <summary>Raycasts and Sweeps buffter distance</summary>
        public const float k_CastBuffer = 0.005f;


        // --- Entity ---
        /// <summary>Max degree of walkable slope</summary>
        public const float k_SlopeLimit = 50f;
        /// <summary>Max fall speed</summary>
        public const float k_FallLimit = 12f;
    }
}