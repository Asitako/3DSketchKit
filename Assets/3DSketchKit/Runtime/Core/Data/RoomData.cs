using UnityEngine;

namespace ThreeDSketchKit.Core.Data
{
    public enum RoomDestroyRule
    {
        DestroyGameObjects,
        DeactivateOnly
    }

    [CreateAssetMenu(fileName = "RoomData", menuName = "3D Sketch Kit/Room Data", order = 2)]
    public class RoomData : ScriptableObject
    {
        [SerializeField] string roomId = "room";
        [SerializeField] RoomDestroyRule destroyRule = RoomDestroyRule.DestroyGameObjects;

        public string RoomId => roomId;
        public RoomDestroyRule DestroyRule => destroyRule;
    }
}
