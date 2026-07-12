using System;
using CleanEnergy.Map;
using UnityEngine;

namespace CleanEnergy.Simulation
{
    public enum SimulationSpeed
    {
        Paused = 0,
        One = 1,
        Two = 2,
        Four = 4
    }

    /// <summary>
    /// Fixed-step simulation clock with pause, speed multipliers and day cycle.
    /// </summary>
    public sealed class SimulationClock : MonoBehaviour
    {
        [SerializeField] private float baseTickSeconds = 0.5f;
        [SerializeField] private SimulationSpeed speed = SimulationSpeed.One;
        [SerializeField] private int ticksPerDay = DayCycleService.DefaultTicksPerDay;
        [SerializeField] private MapGenerator mapGenerator;

        private float _accumulator;
        private DayCycleService _dayCycle;

        public float BaseTickSeconds => baseTickSeconds;
        public SimulationSpeed Speed => speed;
        public int TickIndex { get; private set; }
        public DayCycleService DayCycle => _dayCycle ??= new DayCycleService(ticksPerDay);
        public event Action<SimulationContext> Ticked;

        private void Awake()
        {
            _dayCycle = new DayCycleService(ticksPerDay);
        }

        private void OnEnable()
        {
            SubscribeMap();
        }

        private void OnDisable()
        {
            UnsubscribeMap();
        }

        public void BindMapGenerator(MapGenerator generator)
        {
            UnsubscribeMap();
            mapGenerator = generator;
            SubscribeMap();
        }

        private void Update()
        {
            if (speed == SimulationSpeed.Paused || baseTickSeconds <= 0f)
            {
                return;
            }

            _accumulator += Time.deltaTime * (int)speed;
            while (_accumulator >= baseTickSeconds)
            {
                _accumulator -= baseTickSeconds;
                TickIndex++;
                DayCycle.SyncFromTickIndex(TickIndex);
                var context = new SimulationContext(
                    TickIndex,
                    baseTickSeconds,
                    speed,
                    DayCycle.DayNormalized,
                    DayCycle.Phase);
                Ticked?.Invoke(context);
            }
        }

        public void SetSpeed(SimulationSpeed newSpeed)
        {
            speed = newSpeed;
        }

        public void ResetClock()
        {
            TickIndex = 0;
            _accumulator = 0f;
            DayCycle.Reset();
        }

        public void RestoreTick(int tickIndex)
        {
            TickIndex = Mathf.Max(0, tickIndex);
            _accumulator = 0f;
            DayCycle.SyncFromTickIndex(TickIndex);
        }

        private void OnMapGenerated(Core.MapGeneratedEvent _)
        {
            ResetClock();
        }

        private void SubscribeMap()
        {
            if (mapGenerator != null)
            {
                mapGenerator.Events.MapGenerated += OnMapGenerated;
            }
        }

        private void UnsubscribeMap()
        {
            if (mapGenerator != null)
            {
                mapGenerator.Events.MapGenerated -= OnMapGenerated;
            }
        }
    }
}
