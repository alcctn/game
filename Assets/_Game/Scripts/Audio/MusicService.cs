using CleanEnergy.UI;
using UnityEngine;

namespace CleanEnergy.Audio
{
    /// <summary>
    /// Loops a background music / ambience clip. Null clip is a no-op.
    /// Volume uses <see cref="SettingsService.MusicVolume"/>; SFX mute does not affect music.
    /// </summary>
    public sealed class MusicService : MonoBehaviour
    {
        public const string VolumePrefsKey = SettingsService.MusicVolumeKey;

        [SerializeField] private AudioClip loopClip;
        [SerializeField] private bool createStubClipIfMissing;

        private AudioSource _source;
        private static MusicService _instance;

        public static MusicService Instance => _instance;

        /// <summary>True when a clip is assigned and the source is playing.</summary>
        public bool IsPlaying => _source != null && _source.isPlaying;

        public AudioClip LoopClip => loopClip;

        private void Awake()
        {
            _instance = this;
            EnsureAudio();
            if (createStubClipIfMissing && loopClip == null)
            {
                loopClip = CreateStubLoop();
            }

            ApplyVolume();
            TryStart();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public void Configure(AudioClip clip)
        {
            _instance = this;
            loopClip = clip;
            EnsureAudio();
            ApplyVolume();
            TryStart();
        }

        /// <summary>Applies persisted music volume to the active MusicService (if any).</summary>
        public static void ApplyVolumeFromPrefs()
        {
            if (_instance == null)
            {
                _instance = Object.FindAnyObjectByType<MusicService>();
            }

            if (_instance != null)
            {
                _instance.ApplyVolume();
            }
        }

        public void ApplyVolume()
        {
            EnsureAudio();
            _source.volume = SettingsService.MusicVolume;
        }

        private void TryStart()
        {
            EnsureAudio();
            if (loopClip == null)
            {
                if (_source.isPlaying)
                {
                    _source.Stop();
                }

                _source.clip = null;
                return;
            }

            if (_source.clip != loopClip)
            {
                _source.clip = loopClip;
            }

            _source.loop = true;
            if (!_source.isPlaying)
            {
                _source.Play();
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
            _source.loop = true;
            _source.spatialBlend = 0f;
        }

        private static AudioClip CreateStubLoop()
        {
            const int sampleRate = 22050;
            const float durationSeconds = 1f;
            var sampleCount = Mathf.Max(1, Mathf.CeilToInt(sampleRate * durationSeconds));
            var clip = AudioClip.Create("music_stub", sampleCount, 1, sampleRate, false);
            var data = new float[sampleCount];
            for (var i = 0; i < sampleCount; i++)
            {
                data[i] = Mathf.Sin(2f * Mathf.PI * 220f * i / sampleRate) * 0.05f;
            }

            clip.SetData(data, 0);
            return clip;
        }
    }
}
