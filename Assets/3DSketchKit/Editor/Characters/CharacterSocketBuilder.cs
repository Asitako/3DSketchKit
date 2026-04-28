using System.Collections.Generic;
using ThreeDSketchKit.Core.Components;
using UnityEngine;

namespace ThreeDSketchKit.Editor.Characters
{
    public static class CharacterSocketBuilder
    {
        static readonly string[] SocketIds =
        {
            "RightHand",
            "LeftHand",
            "WeaponSocket",
            "BackSocket",
            "HeadSocket",
            "VfxRoot",
            "CameraTarget",
            "InteractionPoint"
        };

        public static List<CharacterSocketReference> BuildSockets(GameObject characterRoot, Transform modelRoot)
        {
            var socketsRoot = new GameObject("Sockets").transform;
            socketsRoot.SetParent(characterRoot.transform, false);

            var sockets = new List<CharacterSocketReference>();
            foreach (var socketId in SocketIds)
            {
                var target = FindTransformByName(modelRoot, socketId);
                var socketTransform = new GameObject(socketId).transform;
                socketTransform.SetParent(socketsRoot, false);

                if (target != null)
                {
                    socketTransform.position = target.position;
                    socketTransform.rotation = target.rotation;
                }

                sockets.Add(new CharacterSocketReference(socketId, socketTransform));
            }

            return sockets;
        }

        static Transform FindTransformByName(Transform root, string expectedName)
        {
            if (root == null)
                return null;

            var transforms = root.GetComponentsInChildren<Transform>(true);
            foreach (var transform in transforms)
            {
                if (transform.name.IndexOf(expectedName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return transform;
            }

            return null;
        }
    }
}
