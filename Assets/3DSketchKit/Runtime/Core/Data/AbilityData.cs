using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeDSketchKit.Core.Data
{
    [CreateAssetMenu(fileName = "AbilityData", menuName = "3D Sketch Kit/Ability Data", order = 0)]
    public class AbilityData : ScriptableObject
    {
        [SerializeField] string displayName = "Ability";
        [SerializeField] Sprite icon;
        [SerializeField] float cooldownSeconds;
        [SerializeField] List<AbilityParameter> parameters = new();

        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public float CooldownSeconds => cooldownSeconds;
        public IReadOnlyList<AbilityParameter> Parameters => parameters;

        public float GetFloat(string key, float defaultValue = 0f)
        {
            foreach (var parameter in parameters)
            {
                if (parameter.Key == key && parameter.Type == AbilityParameterKind.Float)
                    return parameter.FloatValue;
            }

            return defaultValue;
        }
    }

    [Serializable]
    public struct AbilityParameter
    {
        public string Key;
        public AbilityParameterKind Type;
        public float FloatValue;
        public string StringValue;
    }

    public enum AbilityParameterKind
    {
        Float,
        String
    }
}
