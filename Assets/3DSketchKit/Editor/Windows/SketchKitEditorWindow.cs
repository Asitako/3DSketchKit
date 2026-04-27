using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ThreeDSketchKit.Core.Attributes;
using ThreeDSketchKit.Core.Components;
using ThreeDSketchKit.Core.Data;
using ThreeDSketchKit.Editor.Rooms;
using ThreeDSketchKit.Utility;
using UnityEditor;
using UnityEngine;

namespace ThreeDSketchKit.Editor.Windows
{
    public sealed class SketchKitEditorWindow : EditorWindow
    {
        static readonly GUIContent[] TabContents =
        {
            new("Rooms", "Group selected objects under a Room, or unpack an existing room."),
            new("Abilities", "Browse ability types and append slots to an AbilityManager."),
            new("Zones", "Create ScriptableObject assets for zones, abilities, and rooms."),
        };

        static readonly GUIContent CreateRoomFromSelectionContent = new(
            "Create Room From Selection",
            "Need two or more roots, same room nesting level, and at most one room owner. " +
            "Cannot use every member of a room, cannot merge room roots on different levels, and depth is limited (see 3D Sketch Kit docs). " +
            "If two or more members share a room, the new group is parented there; if only one is registered, the new group is at scene level.");

        static readonly GUIContent UnpackRoomContent = new(
            "Unpack room",
            "Former members move under the room’s owner in the hierarchy and become members of that room again, or to the scene root with no room membership if there is no owner.");

        static readonly GUIContent RefreshAbilityListContent = new(
            "Refresh ability list",
            "Rescan assemblies for IAbility types with a parameterless constructor and update the dropdown.");

        static readonly GUIContent RebuildAbilityCatalogContent = new(
            "Rebuild ability type catalog",
            "Runs AbilityTypeCatalog.RefreshDiscoveredAbilities() so ids and types match the current codebase.");

        static readonly GUIContent AbilityTypeLabelContent = new(
            "Type",
            "Ability implementation to add. Types with [SketchKitAbilityId] get a stable id in the new slot.");

        static readonly GUIContent AppendAbilitySlotContent = new(
            "Append to selected AbilityManager",
            "Adds one slot to the AbilityManager on the active GameObject, using the type chosen above. Select an object with AbilityManager first.");

        static readonly GUIContent CreateZoneEffectDataContent = new(
            "Create ZoneEffectData",
            "Starts the create-asset flow for a new ZoneEffectData asset (zone buff / effect configuration).");

        static readonly GUIContent CreateAbilityDataContent = new(
            "Create AbilityData",
            "Starts the create-asset flow for a new AbilityData asset.");

        static readonly GUIContent CreateRoomDataContent = new(
            "Create RoomData",
            "Starts the create-asset flow for a new RoomData asset (e.g. destroy rules).");

        enum Tab
        {
            Rooms,
            Abilities,
            Zones
        }

        Tab _tab = Tab.Rooms;
        Vector2 _scroll;
        int _abilityTypeIndex;
        Type[] _abilityTypes = Array.Empty<Type>();

        [MenuItem("Window/3D Sketch Kit")]
        public static void Open()
        {
            var sketchKitWindow = GetWindow<SketchKitEditorWindow>();
            sketchKitWindow.titleContent = new GUIContent("3D Sketch Kit");
            sketchKitWindow.minSize = new Vector2(360, 220);
        }

        void OnEnable() => RefreshAbilityTypes();

        void OnGUI()
        {
            _tab = (Tab)GUILayout.Toolbar((int)_tab, TabContents);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            switch (_tab)
            {
                case Tab.Rooms:
                    DrawRooms();
                    break;
                case Tab.Abilities:
                    DrawAbilities();
                    break;
                case Tab.Zones:
                    DrawZones();
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        void DrawRooms()
        {
            using (new EditorGUI.DisabledScope(Selection.gameObjects.Length == 0))
            {
                if (GUILayout.Button(CreateRoomFromSelectionContent))
                    CreateRoomFromSelection();
            }

            EditorGUILayout.Space(6);

            using (new EditorGUI.DisabledScope(!HasRoomInSelection()))
            {
                if (GUILayout.Button(UnpackRoomContent))
                    DismantleRoomFromSelection();
            }
        }

        void DrawAbilities()
        {
            EditorGUILayout.LabelField("Ability types", EditorStyles.boldLabel);
            if (GUILayout.Button(RefreshAbilityListContent))
                RefreshAbilityTypes();
            if (GUILayout.Button(RebuildAbilityCatalogContent))
                AbilityTypeCatalog.RefreshDiscoveredAbilities();

            if (_abilityTypes.Length == 0)
            {
                EditorGUILayout.HelpBox("No IAbility implementations with a parameterless constructor were found.", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(AbilityTypeLabelContent);
            _abilityTypeIndex = EditorGUILayout.Popup(_abilityTypeIndex, _abilityTypes.Select(abilityType => abilityType.FullName).ToArray());
            EditorGUILayout.EndHorizontal();

            var abilityManager = Selection.activeGameObject != null ? Selection.activeGameObject.GetComponent<AbilityManager>() : null;
            using (new EditorGUI.DisabledScope(abilityManager == null))
            {
                if (GUILayout.Button(AppendAbilitySlotContent) && abilityManager != null)
                {
                    Undo.RecordObject(abilityManager, "Add Ability Slot");
                    var selectedAbilityType = _abilityTypes[_abilityTypeIndex];
                    var stableIdAttribute = selectedAbilityType.GetCustomAttribute<SketchKitAbilityIdAttribute>(inherit: false);
                    var newAbilitySlot = new AbilitySlot
                    {
                        abilityId = stableIdAttribute != null ? stableIdAttribute.AbilityId : "",
                        assemblyQualifiedTypeName = selectedAbilityType.AssemblyQualifiedName,
                        config = null,
                        startActive = true
                    };
                    var serializedAbilityManager = new SerializedObject(abilityManager);
                    var abilitySlotsProperty = serializedAbilityManager.FindProperty("abilitySlots");
                    abilitySlotsProperty.arraySize++;
                    var newSlotProperty = abilitySlotsProperty.GetArrayElementAtIndex(abilitySlotsProperty.arraySize - 1);
                    newSlotProperty.FindPropertyRelative("abilityId").stringValue = newAbilitySlot.AbilityId;
                    newSlotProperty.FindPropertyRelative("assemblyQualifiedTypeName").stringValue = newAbilitySlot.AssemblyQualifiedTypeName;
                    newSlotProperty.FindPropertyRelative("config").objectReferenceValue = null;
                    newSlotProperty.FindPropertyRelative("startActive").boolValue = newAbilitySlot.StartActive;
                    serializedAbilityManager.ApplyModifiedProperties();
                    EditorUtility.SetDirty(abilityManager);
                }
            }
        }

        void DrawZones()
        {
            EditorGUILayout.LabelField("Zone data assets", EditorStyles.boldLabel);
            if (GUILayout.Button(CreateZoneEffectDataContent))
            {
                var zoneEffectDataAsset = CreateInstance<ZoneEffectData>();
                ProjectWindowUtil.CreateAsset(zoneEffectDataAsset, "NewZoneEffectData.asset");
            }

            if (GUILayout.Button(CreateAbilityDataContent))
            {
                var abilityDataAsset = CreateInstance<AbilityData>();
                ProjectWindowUtil.CreateAsset(abilityDataAsset, "NewAbilityData.asset");
            }

            if (GUILayout.Button(CreateRoomDataContent))
            {
                var roomDataAsset = CreateInstance<RoomData>();
                ProjectWindowUtil.CreateAsset(roomDataAsset, "NewRoomData.asset");
            }
        }

        void CreateRoomFromSelection()
        {
            var selected = Selection.gameObjects
                .Where(selectedObject => selectedObject.transform.parent == null || !Selection.Contains(selectedObject.transform.parent.gameObject))
                .ToArray();
            if (selected.Length == 0)
            {
                EditorUtility.DisplayDialog("3D Sketch Kit", "Select at least one GameObject.", "OK");
                return;
            }

            if (!RoomCommandValidation.TryValidateCreateFromSelection(selected, out var plan, out var errorMessage))
            {
                EditorUtility.DisplayDialog("3D Sketch Kit", errorMessage, "OK");
                return;
            }

            var roomRootObject = new GameObject("Room");
            Undo.RegisterCreatedObjectUndo(roomRootObject, "Create Room");
            var room = Undo.AddComponent<Room>(roomRootObject);
            Undo.RecordObject(room, "Create Room Members");

            if (plan!.ParentForNewRoom != null)
            {
                Undo.SetTransformParent(roomRootObject.transform, plan.ParentForNewRoom.transform, "Create Room");
                Undo.RecordObject(plan.ParentForNewRoom, "Create Room");
                // Inner Room root must be a registered member of the parent Room so Unpack can find the owner
                // (hierarchy parent alone is not used by Room data until here).
                var selfAsMember = roomRootObject.GetComponent<RoomMemberComponent>();
                if (selfAsMember == null)
                {
                    selfAsMember = Undo.AddComponent<RoomMemberComponent>(roomRootObject);
                    Undo.RecordObject(selfAsMember, "Create Room");
                }
                else
                    Undo.RecordObject(selfAsMember, "Create Room");
                plan.ParentForNewRoom.AddMember(selfAsMember);
            }

            foreach (var g in selected)
            {
                foreach (var member in g.GetComponentsInChildren<RoomMemberComponent>(true))
                {
                    if (member == null)
                        continue;
                    var ownerR = member.OwnerRoom as Room;
                    if (ownerR != null)
                        ownerR.RemoveMember(member);
                }

                Undo.SetTransformParent(g.transform, roomRootObject.transform, "Parent To Room");

                foreach (var c in g.GetComponentsInChildren<RoomMemberComponent>(true))
                {
                    if (c == null || c.gameObject == g)
                        continue;
                    Undo.DestroyObjectImmediate(c);
                }

                var m = g.GetComponent<RoomMemberComponent>();
                if (m == null)
                    m = Undo.AddComponent<RoomMemberComponent>(g);
                room.AddMember(m);
            }

            EditorUtility.SetDirty(room);
            if (plan.ParentForNewRoom != null)
                EditorUtility.SetDirty(plan.ParentForNewRoom);
            Selection.activeGameObject = roomRootObject;
        }

        static bool HasRoomInSelection()
        {
            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
                return false;
            foreach (var selectedGameObject in Selection.gameObjects)
            {
                if (selectedGameObject == null)
                    continue;
                if (selectedGameObject.GetComponent<Room>() != null || selectedGameObject.GetComponentInParent<Room>() != null)
                    return true;
            }

            return false;
        }

        static IEnumerable<Room> CollectDistinctRoomsFromSelection()
        {
            var rooms = new HashSet<Room>();
            foreach (var selectedGameObject in Selection.gameObjects)
            {
                if (selectedGameObject == null)
                    continue;
                var room = selectedGameObject.GetComponent<Room>() ?? selectedGameObject.GetComponentInParent<Room>();
                if (room != null)
                    rooms.Add(room);
            }

            return rooms;
        }

        void DismantleRoomFromSelection()
        {
            var roomsToUnpack = CollectDistinctRoomsFromSelection().ToList();
            if (roomsToUnpack.Count == 0)
            {
                EditorUtility.DisplayDialog("3D Sketch Kit", "No Room found in the selection (select a Room root or a registered member).", "OK");
                return;
            }

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Dismantle Room");
            var undoGroup = Undo.GetCurrentGroup();

            roomsToUnpack.Sort(CompareRoomUnpackOrder);

            var selectionAfter = new List<UnityEngine.Object>();
            foreach (var room in roomsToUnpack)
                selectionAfter.AddRange(DismantleSingleRoom(room));

            Undo.CollapseUndoOperations(undoGroup);

            if (selectionAfter.Count > 0)
                Selection.objects = selectionAfter.ToArray();
        }

        /// <summary>Deeper rooms first so nested selections unpack safely.</summary>
        static int CompareRoomUnpackOrder(Room a, Room b)
        {
            if (a == null || b == null)
                return 0;
            var depthA = GetTransformHierarchyDepth(a.transform);
            var depthB = GetTransformHierarchyDepth(b.transform);
            return depthB.CompareTo(depthA);
        }

        static int GetTransformHierarchyDepth(Transform transform)
        {
            var depth = 0;
            while (transform.parent != null)
            {
                depth++;
                transform = transform.parent;
            }

            return depth;
        }

        /// <summary>Room that owns the unpacked room object (if any), for hierarchy and membership of former members.</summary>
        static Room GetUnpackParentOwnerRoom(Room room)
        {
            if (room == null)
                return null;
            var self = room.GetComponent<RoomMemberComponent>();
            if (self != null)
            {
                var w = self.OwnerRoom as Room;
                if (w != null && w != room)
                    return w;
            }

            // Scenes from before the inner room was auto-registered: child of a Room in hierarchy, but no member on the Room root.
            var p = room.transform.parent;
            if (p == null)
                return null;
            var parentRoom = p.GetComponent<Room>();
            if (parentRoom == null || parentRoom == room)
                return null;
            return parentRoom;
        }

        /// <summary>
        /// Unparents each member, removes the <see cref="Room"/> root: members register on the same owner, or become unowned at scene root.
        /// </summary>
        static IEnumerable<UnityEngine.Object> DismantleSingleRoom(Room room)
        {
            if (room == null)
                yield break;

            var roomMembers = room.Members
                .OfType<RoomMemberComponent>()
                .Where(roomMember => roomMember != null)
                .ToList();

            var parentOwner = GetUnpackParentOwnerRoom(room);
            var selfMember = room.GetComponent<RoomMemberComponent>();
            if (parentOwner != null && selfMember != null)
            {
                Undo.RecordObject(parentOwner, "Unpack Room");
                parentOwner.RemoveMember(selfMember);
            }

            Undo.RecordObject(room, "Unpack Room");

            foreach (var roomMember in roomMembers)
            {
                var mTransform = roomMember.transform;
                var mGo = roomMember.gameObject;
                Undo.RecordObject(mTransform, "Unpack Room");
                Undo.RecordObject(roomMember, "Unpack Room");
                room.RemoveMember(roomMember);
                Undo.SetTransformParent(mTransform, parentOwner != null ? parentOwner.transform : null, "Unpack Room");
                if (parentOwner != null)
                {
                    Undo.RecordObject(parentOwner, "Unpack Room");
                    parentOwner.AddMember(roomMember);
                }
                else
                    Undo.DestroyObjectImmediate(roomMember);

                yield return mGo;
            }

            if (parentOwner != null)
                EditorUtility.SetDirty(parentOwner);

            Undo.DestroyObjectImmediate(room.gameObject);
        }

        void RefreshAbilityTypes() => _abilityTypes = AbilityTypeDiscovery.FindAbilityTypes().ToArray();
    }
}
