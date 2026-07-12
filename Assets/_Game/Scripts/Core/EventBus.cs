using System;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using UnityEngine;

namespace CleanEnergy.Core
{
    /// <summary>
    /// Raised when a map generation pass completes.
    /// </summary>
    public sealed class MapGeneratedEvent
    {
        public string Seed { get; }
        public int Width { get; }
        public int Height { get; }

        public MapGeneratedEvent(string seed, int width, int height)
        {
            Seed = seed;
            Width = width;
            Height = height;
        }
    }

    /// <summary>
    /// Minimal typed event hub for prototype systems.
    /// </summary>
    public sealed class EventBus
    {
        public event Action<MapGeneratedEvent> MapGenerated;

        public void Publish(MapGeneratedEvent evt) => MapGenerated?.Invoke(evt);
    }
}
