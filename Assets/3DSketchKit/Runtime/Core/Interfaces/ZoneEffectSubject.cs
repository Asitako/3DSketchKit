using UnityEngine;

namespace ThreeDSketchKit.Core.Interfaces
{
    /// <summary>
    /// Minimal view of an object inside a trigger for zone modules.
    /// </summary>
    public sealed class ZoneEffectSubject
    {
        public ZoneEffectSubject(GameObject gameObject, Collider collider)
        {
            GameObject = gameObject;
            Collider = collider;
        }

        public GameObject GameObject { get; }
        public Collider Collider { get; }

        public T GetComponent<T>() where T : Component => GameObject != null ? GameObject.GetComponent<T>() : null;
    }
}
