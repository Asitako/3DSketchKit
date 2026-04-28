using ThreeDSketchKit.Core.Attributes;
using UnityEngine;

namespace ThreeDSketchKit.Core.Components
{
    public abstract class CharacterSystemHost : MonoBehaviour
    {
        [SerializeField] bool activateOnInitialize = true;

        protected CharacterEntity Entity { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsActive { get; private set; }

        public abstract CharacterSystemKind SystemKind { get; }

        public void InitializeHost(CharacterEntity entity)
        {
            if (IsInitialized)
                return;

            Entity = entity;
            OnInitialize(entity);
            IsInitialized = true;

            if (activateOnInitialize)
                Activate();
        }

        public void Activate()
        {
            if (IsActive)
                return;
            IsActive = true;
            OnActivate();
        }

        public void Deactivate()
        {
            if (!IsActive)
                return;
            IsActive = false;
            OnDeactivate();
        }

        public void ShutdownHost()
        {
            if (!IsInitialized)
                return;

            if (IsActive)
                Deactivate();

            OnShutdown();
            Entity = null;
            IsInitialized = false;
        }

        public abstract void ValidateHost(CharacterModuleValidationReport report);

        protected virtual void OnInitialize(CharacterEntity entity) {}
        protected virtual void OnActivate() {}
        protected virtual void OnDeactivate() {}
        protected virtual void OnShutdown() {}
    }
}
