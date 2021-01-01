using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stroy {
    public static class GameLayer {
        public const string STATIC_BLOCK_NAME = "StaticBlock";
        public const string DYNAMIC_BLOCK_NAME = "DynamicBlock";
        public const string ENTITY_NAME = "Entity";

        private static int s_staticBlock;
        private static int s_dynamicBlock;
        private static int s_entity;

        private static bool s_uncachedStaticBlock = true;
        private static bool s_uncachedDynamicBlock = true;
        private static bool s_uncachedEntity = true;

        public static int StaticBlock {
            get {
                if (s_uncachedStaticBlock) {
                    s_staticBlock = LayerMask.NameToLayer(STATIC_BLOCK_NAME);
                    s_uncachedStaticBlock = false;
                }
                return s_staticBlock;
            }
        }
        public static int DynamicBlock {
            get {
                if (s_uncachedDynamicBlock) {
                    s_dynamicBlock = LayerMask.NameToLayer(DYNAMIC_BLOCK_NAME);
                    s_uncachedDynamicBlock = false;
                }
                return s_dynamicBlock;
            }
        }
        public static int Entity {
            get {
                if (s_uncachedEntity) {
                    s_entity = LayerMask.NameToLayer(ENTITY_NAME);
                    s_uncachedEntity = false;
                }
                return s_entity;
            }
        }
    }
}