using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SmartDebugger
{
    internal static class ComponentExtensions
    {
        /// <summary>
        /// Instantiates the self
        /// </summary>
        /// <typeparam name="T">The </typeparam>
        /// <param name="self">The self</param>
        /// <param name="count">The count</param>
        /// <param name="action">The action</param>
        internal static void PooledInstantiate<T>(this T self, int count, Action<int, T> action = null)
            where T : Component
        {
            PooledInstantiatePrefab(self, self.transform.parent, count, action);
        }

        /// <summary>
        /// Instantiates the prefab using the specified prefab
        /// </summary>
        /// <typeparam name="T">The </typeparam>
        /// <param name="prefab">The prefab</param>
        /// <param name="parent">The parent</param>
        /// <param name="count">The count</param>
        /// <param name="action">The action</param>
        internal static void PooledInstantiatePrefab<T>(this T prefab, Transform parent, int count,
            Action<int, T> action = null)
            where T : Component
        {
            var index = 0;
            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (!child.TryGetComponent<T>(out var component) ||
                    (component == prefab && !prefab.gameObject.activeSelf)) continue;
                if (index < count)
                {
                    child.gameObject.SetActive(true);
                    action?.Invoke(index++, component);
                }
                else
                {
                    child.gameObject.SetActive(false);
                }
            }

            for (var i = index; i < count; i++)
            {
                var go = Object.Instantiate(prefab.gameObject, parent);
                if (go.TryGetComponent<T>(out var t))
                {
                    action?.Invoke(i, t);
                }
            }
        }
    }
}