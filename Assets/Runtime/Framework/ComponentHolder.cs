using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StroyMaker.Framework {
    /// <summary>Component holder to fast randomly access to component</summary>
    public class ComponentHolder<T> where T : Component {
        // Singleton
        private static ComponentHolder<T> m_instance;

        private static ComponentHolder<T> GetInstance () {
            if (m_instance == null) {
                m_instance = new ComponentHolder<T>();
            }

            return m_instance;
        }

        private readonly Dictionary<int, T> m_componentTable;

        protected ComponentHolder () {
            m_componentTable = CreateTable();
        }

        protected virtual Dictionary<int, T> CreateTable () {
            return new Dictionary<int, T>();
        }


        // Interface
        public static void Add (T component) {
            ComponentHolder<T> instance = GetInstance();
            instance.m_componentTable.Add(component.gameObject.GetInstanceID(), component);
        }
        public static void Remove (T component) {
            ComponentHolder<T> instance = GetInstance();
            instance.m_componentTable.Remove(component.gameObject.GetInstanceID());
        }
        public static bool Find (GameObject gameObject, out T component) {
            ComponentHolder<T> instance = GetInstance();
            return instance.m_componentTable.TryGetValue(gameObject.GetInstanceID(), out component);
        }
    }
}