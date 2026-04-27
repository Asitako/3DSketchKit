using System.Collections.Generic;

namespace ThreeDSketchKit.Core.Interfaces
{
    public interface IRoom
    {
        IReadOnlyList<IRoomMember> Members { get; }
        void AddMember(IRoomMember member);
        void RemoveMember(IRoomMember member);
        void ActivateRoom();
        void DeactivateRoom();
        void DestroyRoom();
    }
}
