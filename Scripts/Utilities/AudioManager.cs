using PoEClone2D.Combat;
using UnityEngine;

namespace PoEClone2D.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource uiSource;

        [Header("Settings")]
        [SerializeField][Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField][Range(0f, 1f)] private float musicVolume = 0.7f;
        [SerializeField][Range(0f, 1f)] private float sfxVolume = 1f;
        [SerializeField][Range(0f, 1f)] private float uiVolume = 1f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializeAudioSources();
        }

        private void InitializeAudioSources()
        {
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFXSource");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }

            if (uiSource == null)
            {
                GameObject uiObj = new GameObject("UISource");
                uiObj.transform.SetParent(transform);
                uiSource = uiObj.AddComponent<AudioSource>();
                uiSource.playOnAwake = false;
            }

            UpdateVolumes();
        }

        public void PlayAttackSound(AttackData attackData, Vector3 position)
        {
            if (attackData == null || attackData.AttackSound == null) return;

            PlaySoundAtPosition(attackData.AttackSound, position,
                attackData.AttackVolume,
                attackData.RandomizePitch ? Random.Range(attackData.MinPitch, attackData.MaxPitch) : 1f);
        }

        public void PlayHitSound(AttackData attackData, Vector3 position)
        {
            if (attackData == null || attackData.HitSound == null) return;

            PlaySoundAtPosition(attackData.HitSound, position,
                attackData.HitVolume,
                attackData.RandomizePitch ? Random.Range(attackData.MinPitch, attackData.MaxPitch) : 1f);
        }

        public void PlayMissSound(AttackData attackData, Vector3 position)
        {
            if (attackData == null || attackData.MissSound == null) return;

            PlaySoundAtPosition(attackData.MissSound, position,
                attackData.HitVolume,
                attackData.RandomizePitch ? Random.Range(attackData.MinPitch, attackData.MaxPitch) : 1f);
        }

        private void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volume, float pitch)
        {
            // Create temporary GameObject for 3D sound
            GameObject tempAudioObj = new GameObject("TempAudio");
            tempAudioObj.transform.position = position;

            AudioSource tempSource = tempAudioObj.AddComponent<AudioSource>();
            tempSource.clip = clip;
            tempSource.volume = volume * sfxVolume * masterVolume;
            tempSource.pitch = pitch;
            tempSource.spatialBlend = 1f; // Full 3D sound
            tempSource.minDistance = 5f;
            tempSource.maxDistance = 50f;
            tempSource.rolloffMode = AudioRolloffMode.Logarithmic;

            tempSource.Play();

            // Destroy after clip finishes
            Destroy(tempAudioObj, clip.length + 0.1f);
        }

        public void PlaySound2D(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return;

            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(clip, volume * sfxVolume * masterVolume);
        }

        public void UpdateVolumes()
        {
            if (musicSource != null)
                musicSource.volume = musicVolume * masterVolume;

            if (sfxSource != null)
                sfxSource.volume = sfxVolume * masterVolume;

            if (uiSource != null)
                uiSource.volume = uiVolume * masterVolume;
        }

        // Volume setters
        public void SetMasterVolume(float volume) { masterVolume = volume; UpdateVolumes(); }
        public void SetMusicVolume(float volume) { musicVolume = volume; UpdateVolumes(); }
        public void SetSFXVolume(float volume) { sfxVolume = volume; UpdateVolumes(); }
        public void SetUIVolume(float volume) { uiVolume = volume; UpdateVolumes(); }

        // Getters
        public float GetMasterVolume() => masterVolume;
        public float GetMusicVolume() => musicVolume;
        public float GetSFXVolume() => sfxVolume;
        public float GetUIVolume() => uiVolume;
    }
}