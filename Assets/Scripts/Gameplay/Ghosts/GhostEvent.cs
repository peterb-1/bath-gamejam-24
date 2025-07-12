using System;

namespace Gameplay.Ghosts
{
    [Serializable]
    public class GhostEvent
    {
        public GhostEventType type;
        public float time;
        public ushort data;
    }

    public enum GhostEventType
    {
        Jump,
        Land,
        Dash,
        DashCollection,
        DroneKill,
        ZiplineHook,
        ZiplineUnhook,
        CollectibleFound
    }
}