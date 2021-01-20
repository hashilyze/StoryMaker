﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stroy {
    namespace Platforms {
        public abstract class DynamicPlatformAgent : MonoBehaviour {
            public abstract void HandlePlatform();


            public DynamicPlatform Platform => m_platform;
            private DynamicPlatform m_platform;


            private void Awake() {
                m_platform = GetComponent<DynamicPlatform>();
            }
        }
    }
}
