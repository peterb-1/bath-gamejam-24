using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Audio
{
    [CreateAssetMenu(fileName = "AudioDatabase", menuName = "Scriptable Objects/AudioDatabase")]
    public class AudioDatabase : ScriptableObject
    {
        [SerializeField] 
        private AudioClipData[] audioClips;

        private readonly Dictionary<AudioClipIdentifier, List<AudioClipData>> audioClipDictionary = new();

        public void Initialise()
        {
            foreach (var audioClip in audioClips)
            {
                if (!audioClipDictionary.ContainsKey(audioClip.Identifier))
                {
                    audioClipDictionary.Add(audioClip.Identifier, new List<AudioClipData>());
                }

                audioClipDictionary[audioClip.Identifier].Add(audioClip);
            }
        }

        public bool TryGetClipData(AudioClipIdentifier identifier, out AudioClipData clip)
        {
            if (audioClipDictionary.TryGetValue(identifier, out var clips))
            {
                clip = clips.RandomChoice();
                return true;
            }

            clip = null;
            return false;
        }
    }
}
