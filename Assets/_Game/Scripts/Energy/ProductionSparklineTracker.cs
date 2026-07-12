using System.Collections.Generic;
using UnityEngine;

namespace CleanEnergy.Energy
{
    /// <summary>
    /// Ring buffers of recent production samples per building instance (not saved).
    /// </summary>
    public sealed class ProductionSparklineTracker
    {
        public const int SampleCapacity = 20;
        public const int MaxTrackedBuildings = 32;

        private readonly Dictionary<string, Ring> _rings = new Dictionary<string, Ring>();
        private readonly Queue<string> _order = new Queue<string>();

        public int TrackedCount => _rings.Count;

        public void Clear()
        {
            _rings.Clear();
            _order.Clear();
        }

        public void Record(string instanceId, float production)
        {
            if (string.IsNullOrEmpty(instanceId))
            {
                return;
            }

            if (!_rings.TryGetValue(instanceId, out var ring))
            {
                while (_rings.Count >= MaxTrackedBuildings && _order.Count > 0)
                {
                    var oldest = _order.Dequeue();
                    _rings.Remove(oldest);
                }

                ring = new Ring(SampleCapacity);
                _rings[instanceId] = ring;
                _order.Enqueue(instanceId);
            }

            ring.Push(production);
        }

        public bool TryGetSamples(string instanceId, List<float> into)
        {
            if (into == null || !_rings.TryGetValue(instanceId, out var ring))
            {
                return false;
            }

            ring.CopyTo(into);
            return into.Count > 0;
        }

        /// <summary>Pure ring push used by tests.</summary>
        public static void PushSample(float[] buffer, ref int nextIndex, ref int count, float value)
        {
            if (buffer == null || buffer.Length == 0)
            {
                return;
            }

            buffer[nextIndex] = value;
            nextIndex = (nextIndex + 1) % buffer.Length;
            if (count < buffer.Length)
            {
                count++;
            }
        }

        /// <summary>Copies chronological samples from a ring into a list.</summary>
        public static void CopyChronological(
            float[] buffer,
            int nextIndex,
            int count,
            List<float> into)
        {
            into?.Clear();
            if (buffer == null || into == null || count <= 0)
            {
                return;
            }

            var start = (nextIndex - count + buffer.Length * 2) % buffer.Length;
            for (var i = 0; i < count; i++)
            {
                into.Add(buffer[(start + i) % buffer.Length]);
            }
        }

        private sealed class Ring
        {
            private readonly float[] _buffer;
            private int _next;
            private int _count;

            public Ring(int capacity)
            {
                _buffer = new float[Mathf.Max(1, capacity)];
            }

            public void Push(float value)
            {
                PushSample(_buffer, ref _next, ref _count, value);
            }

            public void CopyTo(List<float> into)
            {
                CopyChronological(_buffer, _next, _count, into);
            }
        }
    }
}
