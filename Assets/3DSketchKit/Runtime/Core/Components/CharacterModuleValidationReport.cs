using System.Collections.Generic;

namespace ThreeDSketchKit.Core.Components
{
    public sealed class CharacterModuleValidationReport
    {
        readonly List<string> _errors = new();
        readonly List<string> _warnings = new();

        public IReadOnlyList<string> Errors => _errors;
        public IReadOnlyList<string> Warnings => _warnings;
        public bool HasErrors => _errors.Count > 0;

        public void Error(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                _errors.Add(message);
        }

        public void Warning(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                _warnings.Add(message);
        }
    }
}
