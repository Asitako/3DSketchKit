using ThreeDSketchKit.Core.Components;
using UnityEngine;

namespace ThreeDSketchKit.Editor.Rooms
{
    /// <summary>Result of <see cref="RoomCommandValidation.TryValidateCreateFromSelection"/>.</summary>
    public sealed class RoomCreatePlan
    {
        public RoomCreatePlan(Room parentForNewRoom)
        {
            ParentForNewRoom = parentForNewRoom;
        }

        /// <summary>Parent <see cref="Room"/> the new group root will be a child of; <c>null</c> = under scene (no room ancestor).</summary>
        public Room ParentForNewRoom { get; }
    }
}
