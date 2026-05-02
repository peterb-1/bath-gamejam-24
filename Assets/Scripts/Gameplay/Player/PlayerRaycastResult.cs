using UnityEngine;

namespace Gameplay.Player
{
    public readonly struct PlayerRaycastResult
    {
        public readonly PlayerRaycastHit Flags;
        public readonly RaycastHit2D LeftGround;
        public readonly RaycastHit2D LeftMid;
        public readonly RaycastHit2D RightGround;
        public readonly RaycastHit2D RightMid;

        public PlayerRaycastResult(PlayerRaycastHit flags, RaycastHit2D leftGround, RaycastHit2D leftMid, RaycastHit2D rightGround, RaycastHit2D rightMid)
        {
            Flags = flags;
            LeftGround = leftGround;
            LeftMid = leftMid;
            RightGround = rightGround;
            RightMid = rightMid;
        }
    }
}

