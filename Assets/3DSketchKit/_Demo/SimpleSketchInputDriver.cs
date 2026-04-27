using ThreeDSketchKit.Core.Components;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThreeDSketchKit.Demo
{
    /// <summary>
    /// Minimal legacy Input wiring for the MVP demo scene (Horizontal/Vertical/Jump/Fire1).
    /// </summary>
    public sealed class SimpleSketchInputDriver : MonoBehaviour
    {
        [FormerlySerializedAs("abilities")]
        [SerializeField]
        AbilityManager abilityManager;

        void Reset()
        {
            abilityManager = GetComponent<AbilityManager>();
        }

        void Update()
        {
            if (abilityManager == null)
                return;

            abilityManager.SetMovementInput(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")));

            if (Input.GetButtonDown("Jump"))
                abilityManager.PerformByName("Jump");

            if (Input.GetButtonDown("Fire1"))
                abilityManager.PerformByName("MeleeAttack");
        }
    }
}
