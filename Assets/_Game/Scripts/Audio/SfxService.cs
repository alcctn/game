using CleanEnergy.Placement;
using CleanEnergy.UI;
using UnityEngine;

namespace CleanEnergy.Audio
{
    /// <summary>
    /// Plays short notification SFX. Null clips are no-ops; mute uses PlayerPrefs.
    /// </summary>
    public sealed class SfxService : MonoBehaviour
    {
        public const string MutePrefsKey = SettingsService.SfxMuteKey;

        [SerializeField] private PlacementController placementController;
        [SerializeField] private NotificationController notificationController;
        [SerializeField] private AudioClip placeClip;
        [SerializeField] private AudioClip demolishClip;
        [SerializeField] private AudioClip shortageClip;
        [SerializeField] private AudioClip researchUnlockClip;
        [SerializeField] private AudioClip batteryFullClip;
        [SerializeField] private bool createStubClipsIfMissing = true;

        private AudioSource _source;
        private bool _suppressPlayback;
        private int _playCount;

        /// <summary>Successful Play calls that were not muted / suppressed / null-clip.</summary>
        public int PlayCount => _playCount;

        /// <summary>True when the last Play call was blocked by mute or suppress.</summary>
        public bool LastPlayBlocked { get; private set; }

        public bool SuppressPlayback
        {
            get => _suppressPlayback;
            set => _suppressPlayback = value;
        }

        public static bool IsMuted
        {
            get => SettingsService.SfxMute;
            set => SettingsService.SetSfxMute(value);
        }

        public void Configure(PlacementController placement, NotificationController notifications)
        {
            Unsubscribe();
            placementController = placement;
            notificationController = notifications;
            EnsureAudio();
            EnsureStubClips();
            Subscribe();
        }

        private void Awake()
        {
            EnsureAudio();
            EnsureStubClips();
        }

        private void OnEnable() => Subscribe();

        private void OnDisable() => Unsubscribe();

        /// <summary>
        /// Plays the clip for <paramref name="id"/>. Muted / suppressed / null clip = no-op.
        /// </summary>
        public void Play(SfxId id)
        {
            LastPlayBlocked = false;
            if (_suppressPlayback || IsMuted)
            {
                LastPlayBlocked = true;
                return;
            }

                EnsureAudio();
                EnsureStubClips();
                var clip = ResolveClip(id);
            if (clip == null)
            {
                return;
            }

            _source.PlayOneShot(clip);
            _playCount++;
        }

        public void ResetPlayCount()
        {
            _playCount = 0;
            LastPlayBlocked = false;
        }

        private AudioClip ResolveClip(SfxId id)
        {
            switch (id)
            {
                case SfxId.Place: return placeClip;
                case SfxId.Demolish: return demolishClip;
                case SfxId.Shortage: return shortageClip;
                case SfxId.ResearchUnlock: return researchUnlockClip;
                case SfxId.BatteryFull: return batteryFullClip;
                default: return null;
            }
        }

        private void EnsureAudio()
        {
            if (_source != null)
            {
                return;
            }

            _source = GetComponent<AudioSource>();
            if (_source == null)
            {
                _source = gameObject.AddComponent<AudioSource>();
            }

            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
        }

        private void EnsureStubClips()
        {
            if (!createStubClipsIfMissing)
            {
                return;
            }

            if (placeClip == null) placeClip = CreateSineStub("sfx_place", 660f);
            if (demolishClip == null) demolishClip = CreateSineStub("sfx_demolish", 220f);
            if (shortageClip == null) shortageClip = CreateSineStub("sfx_shortage", 180f, 0.12f);
            if (researchUnlockClip == null) researchUnlockClip = CreateSineStub("sfx_research", 880f);
            if (batteryFullClip == null) batteryFullClip = CreateSineStub("sfx_battery", 520f);
        }

        private static AudioClip CreateSineStub(string name, float frequency, float durationSeconds = 0.08f)
        {
            const int sampleRate = 22050;
            var sampleCount = Mathf.Max(1, Mathf.CeilToInt(sampleRate * durationSeconds));
            var clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            var data = new float[sampleCount];
            for (var i = 0; i < sampleCount; i++)
            {
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / sampleRate) * 0.2f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        private void Subscribe()
        {
            if (placementController != null)
            {
                placementController.BuildingPlaced += OnBuildingPlaced;
                placementController.BuildingRemoved += OnBuildingRemoved;
            }

            if (notificationController != null)
            {
                notificationController.ShortageWarned += OnShortage;
                notificationController.ResearchUnlockNotified += OnResearchUnlock;
                notificationController.BatteryFullNotified += OnBatteryFull;
            }
        }

        private void Unsubscribe()
        {
            if (placementController != null)
            {
                placementController.BuildingPlaced -= OnBuildingPlaced;
                placementController.BuildingRemoved -= OnBuildingRemoved;
            }

            if (notificationController != null)
            {
                notificationController.ShortageWarned -= OnShortage;
                notificationController.ResearchUnlockNotified -= OnResearchUnlock;
                notificationController.BatteryFullNotified -= OnBatteryFull;
            }
        }

        private void OnBuildingPlaced(BuildingPlacedEvent _) => Play(SfxId.Place);

        private void OnBuildingRemoved(BuildingPlacedEvent _) => Play(SfxId.Demolish);

        private void OnShortage() => Play(SfxId.Shortage);

        private void OnResearchUnlock() => Play(SfxId.ResearchUnlock);

        private void OnBatteryFull() => Play(SfxId.BatteryFull);
    }
}
