using System.Collections.Generic;
using ThreeDSketchKit.Core.Interfaces;
using UnityEngine;

namespace ThreeDSketchKit.Modules.Rooms
{
    /// <summary>
    /// Pure helper: aggregate world bounds for room members that have a Renderer or Collider.
    /// </summary>
    public static class RoomBoundsCalculator
    {
        public static Bounds Calculate(IReadOnlyList<IRoomMember> members)
        {
            var hasAny = false;
            var bounds = new Bounds(Vector3.zero, Vector3.zero);

            foreach (var member in members)
            {
                if (member is not MonoBehaviour roomMemberBehaviour || roomMemberBehaviour == null)
                    continue;

                EncapsulateObject(roomMemberBehaviour.gameObject, ref bounds, ref hasAny);
            }

            return hasAny ? bounds : new Bounds(Vector3.zero, Vector3.one);
        }

        static void EncapsulateObject(GameObject rootGameObject, ref Bounds bounds, ref bool hasAny)
        {
            var renderers = rootGameObject.GetComponentsInChildren<Renderer>();
            foreach (var childRenderer in renderers)
            {
                if (!hasAny)
                {
                    bounds = childRenderer.bounds;
                    hasAny = true;
                }
                else
                {
                    bounds.Encapsulate(childRenderer.bounds);
                }
            }

            var colliders = rootGameObject.GetComponentsInChildren<Collider>();
            foreach (var physicsCollider in colliders)
            {
                if (!hasAny)
                {
                    bounds = physicsCollider.bounds;
                    hasAny = true;
                }
                else
                {
                    bounds.Encapsulate(physicsCollider.bounds);
                }
            }
        }
    }
}
