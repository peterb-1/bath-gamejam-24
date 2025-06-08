using System;
using System.Collections.Generic;

namespace Gameplay.Ghosts
{
    [Serializable]
    public class GhostRun 
    {
        public List<GhostFrame> frames;
        public float victoryTime;
    }
}