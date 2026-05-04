using System.Collections.Generic;
using ChromaCube.Data;
using ChromaCube.Level;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ChromaCube.Audio
{
    public class AudioManager : MonoBehaviour
    {
        private const string CaptureClipName = "score-point";
        private const string AudioAssetFolder = "Assets/_Game/backgroundmusic";

        [SerializeField] private AudioClip captureClip;
        [SerializeField] private List<AudioClip> backgroundClips = new List<AudioClip>();
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.35f;
        [SerializeField, Range(0f, 1f)] private float effectsVolume = 0.75f;

        private AudioSource musicSource;
        private AudioSource effectsSource;
        private bool initialized;

        public void Initialize(LevelManager manager)
        {
            EnsureSources();
            AutoDiscoverClipsInEditor();

            if (initialized)
            {
                return;
            }

            manager.OnLevelLoaded += HandleLevelLoaded;
            manager.OnTileCaptured += HandleTileCaptured;
            initialized = true;
        }

        private void EnsureSources()
        {
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
                musicSource.volume = musicVolume;
            }

            if (effectsSource == null)
            {
                effectsSource = gameObject.AddComponent<AudioSource>();
                effectsSource.loop = false;
                effectsSource.playOnAwake = false;
                effectsSource.volume = effectsVolume;
            }
        }

        private void HandleLevelLoaded(LevelData level, int captured, int required)
        {
            if (backgroundClips.Count == 0)
            {
                return;
            }

            var clipIndex = Mathf.Abs(level.index - 1) % backgroundClips.Count;
            var selectedClip = backgroundClips[clipIndex];
            if (musicSource.clip == selectedClip && musicSource.isPlaying)
            {
                return;
            }

            musicSource.clip = selectedClip;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }

        private void HandleTileCaptured(TileData tile)
        {
            if (captureClip == null)
            {
                return;
            }

            effectsSource.PlayOneShot(captureClip, effectsVolume);
        }

        private void AutoDiscoverClipsInEditor()
        {
#if UNITY_EDITOR
            var guids = AssetDatabase.FindAssets("t:AudioClip", new[] { AudioAssetFolder });
            var discoveredBackgroundClips = new List<AudioClip>();
            AudioClip discoveredCaptureClip = null;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip == null)
                {
                    continue;
                }

                if (clip.name.ToLowerInvariant() == CaptureClipName)
                {
                    discoveredCaptureClip = clip;
                }
                else
                {
                    discoveredBackgroundClips.Add(clip);
                }
            }

            discoveredBackgroundClips.Sort((left, right) => string.CompareOrdinal(left.name, right.name));
            captureClip = discoveredCaptureClip;
            backgroundClips = discoveredBackgroundClips;
#endif
        }
    }
}
