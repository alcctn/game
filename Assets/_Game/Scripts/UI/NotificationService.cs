using System.Collections.Generic;

namespace CleanEnergy.UI
{
    public sealed class GameNotification
    {
        public string Message { get; }
        public float ExpiresAt { get; }

        public GameNotification(string message, float expiresAt)
        {
            Message = message ?? string.Empty;
            ExpiresAt = expiresAt;
        }
    }

    /// <summary>
    /// Short-lived notification ring buffer for the bottom HUD feed.
    /// </summary>
    public sealed class NotificationService
    {
        public const int MaxCount = 6;
        public const float DefaultLifetimeSeconds = 5f;
        public const float BatteryFullCooldownSeconds = 3f;

        private readonly List<GameNotification> _items = new List<GameNotification>();
        private float _batteryFullReadyAt;

        public IReadOnlyList<GameNotification> Active => _items;

        public void Push(string message, float now, float lifetimeSeconds = DefaultLifetimeSeconds)
        {
            Prune(now);
            _items.Add(new GameNotification(message, now + lifetimeSeconds));
            while (_items.Count > MaxCount)
            {
                _items.RemoveAt(0);
            }
        }

        public bool TryPushBatteryFull(float now)
        {
            if (now < _batteryFullReadyAt)
            {
                return false;
            }

            Push("Battery full", now);
            _batteryFullReadyAt = now + BatteryFullCooldownSeconds;
            return true;
        }

        public void Prune(float now)
        {
            for (var i = _items.Count - 1; i >= 0; i--)
            {
                if (_items[i].ExpiresAt <= now)
                {
                    _items.RemoveAt(i);
                }
            }
        }
    }
}
