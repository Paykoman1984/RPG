using UnityEngine;

namespace PoEClone2D.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        private void Awake()
        {
            // Simple singleton - no DontDestroyOnLoad needed
            if (Instance == null)
            {
                Instance = this;

                // Make sure we have audio sources
                if (musicSource == null)
                {
                    GameObject musicObj = new GameObject("MusicSource");
                    musicObj.transform.SetParent(transform);
                    musicSource = musicObj.AddComponent<AudioSource>();
                    musicSource.loop = true;
                }

                if (sfxSource == null)
                {
                    GameObject sfxObj = new GameObject("SFXSource");
                    sfxObj.transform.SetParent(transform);
                    sfxSource = sfxObj.AddComponent<AudioSource>();
                    sfxSource.loop = false;
                }

                Debug.Log("AudioManager initialized (Simple version)");
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlayMusic(AudioClip clip, float volume = 1f)
        {
            if (musicSource != null && clip != null)
            {
                musicSource.clip = clip;
                musicSource.volume = volume;
                musicSource.Play();
            }
        }

        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (sfxSource != null && clip != null)
            {
                sfxSource.PlayOneShot(clip, volume);
            }
        }

        public void SetMusicVolume(float volume)
        {
            if (musicSource != null) musicSource.volume = volume;
        }

        public void SetSFXVolume(float volume)
        {
            if (sfxSource != null) sfxSource.volume = volume;
        }
    }
}