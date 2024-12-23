using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio
{
    [Serializable]
    public class MusicData
    {
        [field: SerializeField] 
        public MusicIdentifier Identifier { get; private set; }
        
        [field: SerializeField] 
        public AudioClip[] ParallelAudioClips { get; private set; }
        
        [field: SerializeField] 
        public AudioMixerGroup MixerGroup { get; private set; }

        [field: SerializeField, Range(0f, 1f)] 
        public float Volume { get; private set; } = 1f;
    }
}