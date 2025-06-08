using System;
using Gameplay.Colour;
using UnityEngine;

namespace Gameplay.Ghosts
{
    [Serializable]
    public class GhostFrame 
    {
        public float time;
        public Vector2 position;
        public float zRotation;
        public ColourId colourId;
        public int animationStateHash;
        public bool isFacingRight;
    }
}