using System;
using System.Collections.Generic;
using ThreeDSketchKit.Core.Data.Characters;
using UnityEngine;

namespace ThreeDSketchKit.Core.Components
{
    [DisallowMultipleComponent]
    public sealed class CharacterEntity : MonoBehaviour
    {
        [SerializeField] string entityId;
        [SerializeField] string displayName = "Character";
        [SerializeField] CharacterEntityRole role = CharacterEntityRole.Neutral;
        [SerializeField] Animator animator;
        [SerializeField] Transform modelRoot;
        [SerializeField] CharacterPreset preset;
        [SerializeField] List<CharacterSocketReference> sockets = new();

        readonly Dictionary<Type, CharacterSystemHost> _systemsByType = new();
        readonly Dictionary<string, Transform> _socketsById = new(StringComparer.Ordinal);

        public string EntityId => entityId;
        public string DisplayName => displayName;
        public CharacterEntityRole Role => role;
        public Animator Animator => animator;
        public Transform ModelRoot => modelRoot;
        public CharacterPreset Preset => preset;

        public AbilityManager AbilityManager { get; private set; }
        public MovementComponent Movement { get; private set; }
        public HealthComponent Health { get; private set; }

        void Awake()
        {
            ResolveCoreReferences();
            RebuildSocketLookup();
            RegisterSystems();
            InitializeSystems();
        }

        void OnDestroy()
        {
            foreach (var system in _systemsByType.Values)
                system.ShutdownHost();
        }

        public void Configure(
            string newEntityId,
            string newDisplayName,
            CharacterEntityRole newRole,
            Animator newAnimator,
            Transform newModelRoot,
            CharacterPreset newPreset)
        {
            entityId = newEntityId;
            displayName = newDisplayName;
            role = newRole;
            animator = newAnimator;
            modelRoot = newModelRoot;
            preset = newPreset;
        }

        public void SetSockets(IEnumerable<CharacterSocketReference> socketReferences)
        {
            sockets.Clear();
            if (socketReferences != null)
                sockets.AddRange(socketReferences);
            RebuildSocketLookup();
        }

        public bool TryGetSystem<TSystem>(out TSystem system) where TSystem : CharacterSystemHost
        {
            if (_systemsByType.TryGetValue(typeof(TSystem), out var host) && host is TSystem typedHost)
            {
                system = typedHost;
                return true;
            }

            system = null;
            return false;
        }

        public TSystem GetSystem<TSystem>() where TSystem : CharacterSystemHost
        {
            if (TryGetSystem<TSystem>(out var system))
                return system;
            throw new InvalidOperationException($"Character system {typeof(TSystem).Name} is not registered on {name}.");
        }

        public bool TryGetSocket(string socketId, out Transform socket)
        {
            socket = null;
            return !string.IsNullOrWhiteSpace(socketId) &&
                   _socketsById.TryGetValue(socketId.Trim(), out socket) &&
                   socket != null;
        }

        public Transform GetSocket(string socketId)
        {
            if (TryGetSocket(socketId, out var socket))
                return socket;
            throw new InvalidOperationException($"Socket '{socketId}' is not registered on {name}.");
        }

        public T GetDependency<T>() where T : Component
        {
            var dependency = GetComponent<T>();
            if (dependency != null)
                return dependency;
            return GetComponentInChildren<T>(true);
        }

        public CharacterModuleValidationReport ValidateSystems()
        {
            ResolveCoreReferences();
            RebuildSocketLookup();
            RegisterSystems();
            var report = new CharacterModuleValidationReport();
            foreach (var system in _systemsByType.Values)
                system.ValidateHost(report);
            return report;
        }

        void ResolveCoreReferences()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>(true);
            if (modelRoot == null && animator != null)
                modelRoot = animator.transform;

            AbilityManager = GetComponent<AbilityManager>() ?? GetComponentInChildren<AbilityManager>(true);
            Movement = GetComponent<MovementComponent>() ?? GetComponentInChildren<MovementComponent>(true);
            Health = GetComponent<HealthComponent>() ?? GetComponentInChildren<HealthComponent>(true);
        }

        void RegisterSystems()
        {
            _systemsByType.Clear();
            var systems = GetComponentsInChildren<CharacterSystemHost>(true);
            foreach (var system in systems)
            {
                if (system == null)
                    continue;
                _systemsByType[system.GetType()] = system;
            }
        }

        void InitializeSystems()
        {
            foreach (var system in _systemsByType.Values)
                system.InitializeHost(this);
        }

        void RebuildSocketLookup()
        {
            _socketsById.Clear();
            foreach (var socket in sockets)
            {
                if (string.IsNullOrWhiteSpace(socket.SocketId) || socket.Transform == null)
                    continue;
                _socketsById[socket.SocketId.Trim()] = socket.Transform;
            }
        }
    }

    public enum CharacterEntityRole
    {
        Neutral,
        Player,
        Mob,
        NPC
    }

    [Serializable]
    public struct CharacterSocketReference
    {
        [SerializeField] string socketId;
        [SerializeField] Transform transform;

        public CharacterSocketReference(string socketId, Transform transform)
        {
            this.socketId = socketId;
            this.transform = transform;
        }

        public string SocketId => socketId;
        public Transform Transform => transform;
    }
}
