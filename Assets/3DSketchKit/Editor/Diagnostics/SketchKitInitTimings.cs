using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ThreeDSketchKit.Editor.Diagnostics
{
    /// <summary>Small helper to measure time spent in 3D Sketch Kit editor init hooks.</summary>
    public static class SketchKitInitTimings
    {
        static readonly Dictionary<string, long> StartTicksByLabel = new(StringComparer.Ordinal);
        static readonly Dictionary<string, long> DurationTicksByLabel = new(StringComparer.Ordinal);

        static long? _firstStartTicks;
        static long? _lastEndTicks;

        public static void Begin(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
                label = "<unnamed>";

            var nowTicks = Stopwatch.GetTimestamp();
            _firstStartTicks ??= nowTicks;
            StartTicksByLabel[label] = nowTicks;
        }

        public static void End(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
                label = "<unnamed>";

            var nowTicks = Stopwatch.GetTimestamp();
            _lastEndTicks = nowTicks;

            if (!StartTicksByLabel.TryGetValue(label, out var startTicks))
                return;

            var deltaTicks = nowTicks - startTicks;
            DurationTicksByLabel.TryGetValue(label, out var existing);
            DurationTicksByLabel[label] = existing + Math.Max(0, deltaTicks);
        }

        public static double GetTotalSeconds()
        {
            if (_firstStartTicks == null || _lastEndTicks == null)
                return 0d;
            return (_lastEndTicks.Value - _firstStartTicks.Value) / (double)Stopwatch.Frequency;
        }

        public static double GetSummedModuleSeconds()
        {
            var totalTicks = 0L;
            foreach (var kv in DurationTicksByLabel)
                totalTicks += kv.Value;
            return totalTicks / (double)Stopwatch.Frequency;
        }

        public static string BuildReport()
        {
            var lines = new List<string>();
            lines.Add($"Total init window: {GetTotalSeconds():0.000}s");
            lines.Add($"Summed module time: {GetSummedModuleSeconds():0.000}s");

            foreach (var kv in DurationTicksByLabel.OrderByDescending(kv => kv.Value))
            {
                var seconds = kv.Value / (double)Stopwatch.Frequency;
                lines.Add($"- {kv.Key}: {seconds:0.000}s");
            }

            return string.Join("\n", lines);
        }
    }
}

