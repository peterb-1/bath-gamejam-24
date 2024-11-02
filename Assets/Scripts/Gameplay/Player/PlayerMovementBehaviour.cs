using Gameplay.Colour;
using Gameplay.Input;
using UnityEngine;

namespace Gameplay.Player
{
    public class PlayerMovementBehaviour : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] 
        private float jumpForce;

        [SerializeField] 
        private float doubleJumpForce;
        
        [SerializeField]
        private Vector2 wallJumpForce;

        [SerializeField] 
        private float fallMultiplier;
        
        [SerializeField] 
        private float moveSpeed;
        
        [SerializeField] 
        private float acceleration;

        [SerializeField] 
        private float deceleration;

        [SerializeField] 
        private float coyoteDuration;

        [SerializeField] 
        private float jumpBufferDuration;

        [SerializeField]
        private float groundCheckDistance;
        
        [SerializeField]
        private LayerMask groundLayers;

        [Header("References")]
        [SerializeField] 
        private Rigidbody2D rigidBody;

        [SerializeField]
        private Transform leftGroundCheck;

        [SerializeField]
        private Transform rightGroundCheck;
        
        [SerializeField]
        private Transform leftHeadCheck;

        [SerializeField]
        private Transform rightHeadCheck;

        private Vector2 preFreezeVelocity;
        
        private float coyoteCountdown;
        private float jumpBufferCountdown;
        
        private bool isGrounded;
        private bool isTouchingLeftWall;
        private bool isTouchingRightWall;
        private bool hasDoubleJumped;
        private bool isFrozen;

        private void Awake()
        {
            InputManager.OnJumpPerformed += HandleJumpPerformed;
            ColourManager.OnColourChangeStarted += HandleColourChangeStarted;
            ColourManager.OnColourChangeEnded += HandleColourChangeEnded;
        }

        private void HandleColourChangeStarted(ColourId _, float duration)
        {
            isFrozen = true;
            preFreezeVelocity = rigidBody.linearVelocity;
            rigidBody.linearVelocity = Vector2.zero;
            rigidBody.gravityScale = 0f;
        }
        
        private void HandleColourChangeEnded()
        {
            isFrozen = false;
            rigidBody.linearVelocity = preFreezeVelocity;
            rigidBody.gravityScale = 1f;
        }

        private void Update()
        {
            if (!isGrounded) coyoteCountdown -= Time.deltaTime;
            if (jumpBufferCountdown > 0f)
            {
                jumpBufferCountdown -= Time.deltaTime;
            }

            var trans = transform;
            var down = -trans.up;
            var right = trans.right;
            var left = -right;
            var leftGroundPosition = leftGroundCheck.position;
            var rightGroundPosition = rightGroundCheck.position;

            isGrounded = Physics2D.Raycast(leftGroundPosition, down, groundCheckDistance, groundLayers) ||
                         Physics2D.Raycast(rightGroundPosition, down, groundCheckDistance, groundLayers);
            
            isTouchingLeftWall = Physics2D.Raycast(leftGroundPosition, left, groundCheckDistance, groundLayers) ||
                                 Physics2D.Raycast(leftHeadCheck.position, left, groundCheckDistance, groundLayers);
            
            isTouchingRightWall = Physics2D.Raycast(rightGroundPosition, right, groundCheckDistance, groundLayers) ||
                                 Physics2D.Raycast(rightHeadCheck.position, right, groundCheckDistance, groundLayers);

            if (isGrounded)
            {
                coyoteCountdown = coyoteDuration;
                hasDoubleJumped = false;
            }
        }

        private void FixedUpdate()
        {
            if (isFrozen) return;
            
            var moveAmount = InputManager.MoveAmount;
            var desiredVelocity = moveAmount * moveSpeed;
            
            rigidBody.linearVelocity = moveAmount == 0
                ? new Vector2(Mathf.Lerp(rigidBody.linearVelocityX, 0, deceleration * Time.fixedDeltaTime), rigidBody.linearVelocityY)
                : new Vector2(Mathf.Lerp(rigidBody.linearVelocityX, desiredVelocity, acceleration * Time.fixedDeltaTime), rigidBody.linearVelocityY);
            
            if (rigidBody.linearVelocityY < 0)
            {
                rigidBody.linearVelocity += Vector2.up * (Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime);
            }

            TryJump();
        }
        
        private void HandleJumpPerformed()
        {
            jumpBufferCountdown = jumpBufferDuration;
        }
        
        private void TryJump()
        {
            if (jumpBufferCountdown <= 0) return;
            
            if (isGrounded || coyoteCountdown > 0)
            {
                PerformJump(jumpForce);
            }
            else if (isTouchingLeftWall)
            {
                PerformWallJump(new Vector2(wallJumpForce.x, wallJumpForce.y));
            }
            else if (isTouchingRightWall)
            {
                PerformWallJump(new Vector2(-wallJumpForce.x, wallJumpForce.y));
            }
            else if (!hasDoubleJumped)
            {
                PerformJump(doubleJumpForce);
                hasDoubleJumped = true;
            }
        }
        
        private void PerformJump(float force)
        {
            rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocityX, force);
            jumpBufferCountdown = 0;
        }

        private void PerformWallJump(Vector2 force)
        {
            rigidBody.linearVelocity = force;
            hasDoubleJumped = true;
            jumpBufferCountdown = 0;
        }

        private void OnDestroy()
        {
            InputManager.OnJumpPerformed -= HandleJumpPerformed;
            ColourManager.OnColourChangeStarted -= HandleColourChangeStarted;
            ColourManager.OnColourChangeEnded -= HandleColourChangeEnded;
        }
    }
}
