using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio
{
    [Serializable]
    public class AudioClipData
    {
        [field: SerializeField] 
        public AudioClipIdentifier Identifier { get; private set; }
        
        [field: SerializeField] 
        public AudioClip AudioClip { get; private set; }
        
        [field: SerializeField]
        public AudioMixerGroup MixerGroup { get; private set; }
        
        [field: SerializeField]
        public bool IsLooping { get; private set; }

        [field: SerializeField, Range(0f, 1f)] 
        public float Volume { get; private set; } = 1f;
    }
}
