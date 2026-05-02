using System;

namespace Gameplay.Player
{
    [Flags]
    public enum PlayerRaycastHit
    {
        LeftGround = 1,
        LeftMid = 2 << 0,
        LeftHead = 2 << 1,
        RightGround = 2 << 2,
        RightMid = 2 << 3,
        RightHead = 2 << 4,
    }
}
