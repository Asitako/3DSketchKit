using System;
using System.Linq;
using ThreeDSketchKit.Core.Components;
using ThreeDSketchKit.Core.Data;
using ThreeDSketchKit.Utility;
using UnityEditor;
using UnityEngine;

namespace ThreeDSketchKit.Editor.Windows
{
    public sealed class SketchKitEditorWindow : EditorWindow
    {
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
            _tab = (Tab)GUILayout.Toolbar((int)_tab, new[] { "Rooms", "Abilities", "Zones" });
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
            EditorGUILayout.LabelField("Room from selection", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Select GameObjects in the Hierarchy, then create a parent with Room and register each root as a RoomMember.",
                MessageType.Info);

            using (new EditorGUI.DisabledScope(Selection.gameObjects.Length == 0))
            {
                if (GUILayout.Button("Create Room From Selection"))
                    CreateRoomFromSelection();
            }
        }

        void DrawAbilities()
        {
            EditorGUILayout.LabelField("Ability types", EditorStyles.boldLabel);
            if (GUILayout.Button("Refresh ability list"))
                RefreshAbilityTypes();

            if (_abilityTypes.Length == 0)
            {
                EditorGUILayout.HelpBox("No IAbility implementations with a parameterless constructor were found.", MessageType.Warning);
                return;
            }

            _abilityTypeIndex = EditorGUILayout.Popup("Type", _abilityTypeIndex, _abilityTypes.Select(abilityType => abilityType.FullName).ToArray());

            var abilityManager = Selection.activeGameObject != null ? Selection.activeGameObject.GetComponent<AbilityManager>() : null;
            using (new EditorGUI.DisabledScope(abilityManager == null))
            {
                if (GUILayout.Button("Append to selected AbilityManager") && abilityManager != null)
                {
                    Undo.RecordObject(abilityManager, "Add Ability Slot");
                    var newAbilitySlot = new AbilitySlot
                    {
                        assemblyQualifiedTypeName = _abilityTypes[_abilityTypeIndex].AssemblyQualifiedName,
                        config = null,
                        startActive = true
                    };
                    var serializedAbilityManager = new SerializedObject(abilityManager);
                    var abilitySlotsProperty = serializedAbilityManager.FindProperty("abilitySlots");
                    abilitySlotsProperty.arraySize++;
                    var newSlotProperty = abilitySlotsProperty.GetArrayElementAtIndex(abilitySlotsProperty.arraySize - 1);
                    newSlotProperty.FindPropertyRelative("assemblyQualifiedTypeName").stringValue = newAbilitySlot.AssemblyQualifiedTypeName;
                    newSlotProperty.FindPropertyRelative("config").objectReferenceValue = null;
                    newSlotProperty.FindPropertyRelative("startActive").boolValue = newAbilitySlot.StartActive;
                    serializedAbilityManager.ApplyModifiedProperties();
                    EditorUtility.SetDirty(abilityManager);
                }
            }

            if (abilityManager == null)
                EditorGUILayout.HelpBox("Select a GameObject with AbilityManager to append a slot.", MessageType.None);
        }

        void DrawZones()
        {
            EditorGUILayout.LabelField("Zone data assets", EditorStyles.boldLabel);
            if (GUILayout.Button("Create ZoneEffectData"))
            {
                var zoneEffectDataAsset = CreateInstance<ZoneEffectData>();
                ProjectWindowUtil.CreateAsset(zoneEffectDataAsset, "NewZoneEffectData.asset");
            }

            if (GUILayout.Button("Create AbilityData"))
            {
                var abilityDataAsset = CreateInstance<AbilityData>();
                ProjectWindowUtil.CreateAsset(abilityDataAsset, "NewAbilityData.asset");
            }

            if (GUILayout.Button("Create RoomData"))
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

            var roomRootObject = new GameObject("Room");
            Undo.RegisterCreatedObjectUndo(roomRootObject, "Create Room");
            var room = Undo.AddComponent<Room>(roomRootObject);
            Undo.RecordObject(room, "Create Room Members");

            foreach (var selectedRootGameObject in selected)
            {
                Undo.SetTransformParent(selectedRootGameObject.transform, roomRootObject.transform, "Parent To Room");
                var roomMember = selectedRootGameObject.GetComponent<RoomMemberComponent>();
                if (roomMember == null)
                    roomMember = Undo.AddComponent<RoomMemberComponent>(selectedRootGameObject);
                room.AddMember(roomMember);
            }

            EditorUtility.SetDirty(room);
            Selection.activeGameObject = roomRootObject;
        }

        void RefreshAbilityTypes() => _abilityTypes = AbilityTypeDiscovery.FindAbilityTypes().ToArray();
    }
}
