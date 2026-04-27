using System.Collections;
using System.Collections.Generic;
using ThreeDSketchKit.Core.Data;
using ThreeDSketchKit.Core.Interfaces;
using UnityEngine;

namespace ThreeDSketchKit.Core.Components
{
    public sealed class Room : MonoBehaviour, IRoom
    {
        [SerializeField] RoomData roomData;
        [SerializeField] List<RoomMemberComponent> roomMembers = new();

        RoomMemberReadOnlyAdapter _adapter;

        public RoomData RoomData => roomData;
        public IReadOnlyList<IRoomMember> Members => _adapter ??= new RoomMemberReadOnlyAdapter(roomMembers);

        public void AddMember(IRoomMember member)
        {
            if (member is not RoomMemberComponent roomMemberComponent || roomMemberComponent == null)
                return;
            if (roomMembers.Contains(roomMemberComponent))
                return;
            roomMembers.Add(roomMemberComponent);
            member.OwnerRoom = this;
        }

        public void RemoveMember(IRoomMember member)
        {
            if (member is not RoomMemberComponent roomMemberComponent)
                return;
            if (roomMembers.Remove(roomMemberComponent))
                member.OwnerRoom = null;
        }

        public void ActivateRoom()
        {
            foreach (var roomMember in roomMembers)
            {
                if (roomMember != null)
                    roomMember.gameObject.SetActive(true);
            }
        }

        public void DeactivateRoom()
        {
            foreach (var roomMember in roomMembers)
            {
                if (roomMember != null)
                    roomMember.gameObject.SetActive(false);
            }
        }

        public void DestroyRoom()
        {
            var destroyRule = roomData != null ? roomData.DestroyRule : RoomDestroyRule.DestroyGameObjects;

            for (var memberIndex = roomMembers.Count - 1; memberIndex >= 0; memberIndex--)
            {
                var currentRoomMember = roomMembers[memberIndex];
                if (currentRoomMember == null)
                    continue;

                if (destroyRule == RoomDestroyRule.DestroyGameObjects)
                    Destroy(currentRoomMember.gameObject);
                else
                    currentRoomMember.gameObject.SetActive(false);
            }

            roomMembers.Clear();
            if (destroyRule == RoomDestroyRule.DestroyGameObjects)
                Destroy(gameObject);
            else
                gameObject.SetActive(false);
        }

        sealed class RoomMemberReadOnlyAdapter : IReadOnlyList<IRoomMember>
        {
            readonly List<RoomMemberComponent> _backingRoomMembers;

            public RoomMemberReadOnlyAdapter(List<RoomMemberComponent> backingRoomMembers) => _backingRoomMembers = backingRoomMembers;

            public IEnumerator<IRoomMember> GetEnumerator()
            {
                foreach (var roomMember in _backingRoomMembers)
                    yield return roomMember;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public int Count => _backingRoomMembers.Count;

            public IRoomMember this[int index] => _backingRoomMembers[index];
        }
    }
}
