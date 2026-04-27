using System;
using System.Collections.Generic;
using System.Reflection;
using ThreeDSketchKit.Core.Data;
using ThreeDSketchKit.Core.Interfaces;
using ThreeDSketchKit.Utility;
using UnityEngine;

namespace ThreeDSketchKit.Core.Components
{
    /// <summary>
    /// Bridges Unity lifecycle to plain <see cref="IAbility"/> modules.
    /// </summary>
    public sealed class AbilityManager : MonoBehaviour, IAbilityHost
    {
        [SerializeField] List<AbilitySlot> abilitySlots = new();
        [SerializeField] Vector2 movementInput;

        readonly List<IAbility> _abilities = new();

        public GameObject Owner => gameObject;
        public Vector2 MovementInput => movementInput;

        public IReadOnlyList<IAbility> Abilities => _abilities;

        void Awake() => RebuildAbilities();

        void Update()
        {
            var deltaTime = Time.deltaTime;
            foreach (var ability in _abilities)
            {
                if (!ability.IsActive)
                    continue;
                if (ability is ITickableAbility tickableAbility)
                    tickableAbility.Tick(deltaTime);
            }
        }

        /// <summary>Call from your input reader before abilities tick.</summary>
        public void SetMovementInput(Vector2 input) => movementInput = input;

        public T GetDependency<T>() where T : class
        {
            var currentComponent = GetComponent<T>();
            if (currentComponent != null)
                return currentComponent;
            return GetComponentInChildren<T>();
        }

        public void RebuildAbilities()
        {
            _abilities.Clear();
            foreach (var slot in abilitySlots)
            {
                var resolutionKey = !string.IsNullOrWhiteSpace(slot.AbilityId)
                    ? slot.AbilityId.Trim()
                    : slot.AssemblyQualifiedTypeName;
                if (string.IsNullOrWhiteSpace(resolutionKey))
                    continue;
                if (!AbilityTypeCatalog.TryResolveType(resolutionKey, out var abilityType))
                {
                    SketchKitRuntimeLog.InvalidAbilityType(this, resolutionKey);
                    continue;
                }

                IAbility abilityInstance;
                try
                {
                    abilityInstance = (IAbility)Activator.CreateInstance(
                        abilityType,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        binder: null,
                        args: null,
                        culture: null);
                }
                catch (Exception exception)
                {
                    SketchKitRuntimeLog.AbilityCreationFailed(this, abilityType.Name, exception.Message);
                    continue;
                }

                if (abilityInstance is IAbilityLifecycle abilityLifecycle)
                    abilityLifecycle.OnAttached(this, slot.Config);

                abilityInstance.IsActive = slot.StartActive;
                _abilities.Add(abilityInstance);
            }
        }

        public void PerformAllActions()
        {
            foreach (var ability in _abilities)
            {
                if (ability.IsActive)
                    ability.PerformAction();
            }
        }

        public void PerformByName(string abilityName)
        {
            foreach (var ability in _abilities)
            {
                if (!ability.IsActive)
                    continue;
                if (string.Equals(ability.AbilityName, abilityName, StringComparison.Ordinal))
                    ability.PerformAction();
            }
        }
    }

    [Serializable]
    public sealed class AbilitySlot
    {
        [Tooltip("Stable id from AbilityTypeCatalog (see SketchKitAbilityIdAttribute). If set, takes precedence over assembly-qualified name.")]
        public string abilityId = "";
        [Tooltip("Assembly-qualified type name implementing IAbility. Used when abilityId is empty or as a fallback key.")]
        public string assemblyQualifiedTypeName;
        public AbilityData config;
        public bool startActive = true;

        public string AbilityId
        {
            get => abilityId;
            set => abilityId = value;
        }

        public string AssemblyQualifiedTypeName
        {
            get => assemblyQualifiedTypeName;
            set => assemblyQualifiedTypeName = value;
        }

        public AbilityData Config
        {
            get => config;
            set => config = value;
        }

        public bool StartActive
        {
            get => startActive;
            set => startActive = value;
        }
    }
}
