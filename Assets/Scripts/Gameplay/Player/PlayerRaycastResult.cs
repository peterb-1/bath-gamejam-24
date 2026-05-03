using UnityEngine;

namespace Gameplay.Player
{
    public readonly struct PlayerRaycastResult
    {
        public readonly RaycastHit2D LeftGround;
        public readonly RaycastHit2D LeftMid;
        public readonly RaycastHit2D LeftHead;
        public readonly RaycastHit2D RightGround;
        public readonly RaycastHit2D RightMid;
        public readonly RaycastHit2D RightHead;

        public PlayerRaycastResult(
            RaycastHit2D leftGround,
            RaycastHit2D leftMid,
            RaycastHit2D leftHead,
            RaycastHit2D rightGround,
            RaycastHit2D rightMid,
            RaycastHit2D rightHead)
        {
            LeftGround = leftGround;
            LeftMid = leftMid;
            LeftHead = leftHead;
            RightGround = rightGround;
            RightMid = rightMid;
            RightHead = rightHead;
        }
    }
}
