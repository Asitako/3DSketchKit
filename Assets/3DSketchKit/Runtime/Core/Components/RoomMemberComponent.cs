using ThreeDSketchKit.Core.Interfaces;
using UnityEngine;

namespace ThreeDSketchKit.Core.Components
{
    public sealed class RoomMemberComponent : MonoBehaviour, IRoomMember
    {
        IRoom _owner;

        public IRoom OwnerRoom
        {
            get => _owner;
            set => _owner = value;
        }
    }
}
