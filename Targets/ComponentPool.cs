using System.Collections.Generic;
using UnityEngine;

namespace DuckShooting
{
    /// <summary>
    /// Object pool đơn giản, generic cho các Component (Target, FloatingScore...).
    /// Giảm cấp phát/GC khi spawn liên tục — quan trọng trên mobile.
    /// </summary>
    public class ComponentPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Stack<T> _idle = new Stack<T>();

        public ComponentPool(T prefab, Transform parent, int prewarm = 0)
        {
            _prefab = prefab;
            _parent = parent;
            for (int i = 0; i < prewarm; i++)
            {
                var item = CreateNew();
                item.gameObject.SetActive(false);
                _idle.Push(item);
            }
        }

        private T CreateNew()
        {
            return Object.Instantiate(_prefab, _parent);
        }

        public T Get()
        {
            T item = _idle.Count > 0 ? _idle.Pop() : CreateNew();
            item.gameObject.SetActive(true);
            return item;
        }

        public void Release(T item)
        {
            if (item == null) return;
            item.gameObject.SetActive(false);
            _idle.Push(item);
        }
    }
}
