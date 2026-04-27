using ThreeDSketchKit.Core.Interfaces;
using UnityEngine;

namespace ThreeDSketchKit.Core.Components
{
    /// <summary>
    /// Invokes a <see cref="Room"/> operation when a collider enters this trigger.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class RoomZoneAction : MonoBehaviour
    {
        public enum RoomActionKind
        {
            Activate,
            Deactivate,
            Destroy
        }

        [SerializeField] Room targetRoom;
        [SerializeField] RoomActionKind action = RoomActionKind.Deactivate;
        [SerializeField] string requiredTag = "";

        void Reset()
        {
            var attachedCollider = GetComponent<Collider>();
            attachedCollider.isTrigger = true;
        }

        void OnTriggerEnter(Collider otherCollider)
        {
            if (targetRoom == null)
                return;
            if (!string.IsNullOrEmpty(requiredTag) && !otherCollider.CompareTag(requiredTag))
                return;

            var roomInterface = (IRoom)targetRoom;
            switch (action)
            {
                case RoomActionKind.Activate:
                    roomInterface.ActivateRoom();
                    break;
                case RoomActionKind.Deactivate:
                    roomInterface.DeactivateRoom();
                    break;
                case RoomActionKind.Destroy:
                    roomInterface.DestroyRoom();
                    break;
            }
        }
    }
}
