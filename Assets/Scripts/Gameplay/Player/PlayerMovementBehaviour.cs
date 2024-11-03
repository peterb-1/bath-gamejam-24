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
        private Transform spriteRendererTransform;

        [SerializeField] 
        private Animator playerAnimator;

        [SerializeField]
        private Transform leftGroundCheck;

        [SerializeField]
        private Transform rightGroundCheck;
        
        [SerializeField]
        private Transform leftHeadCheck;

        [SerializeField]
        private Transform rightHeadCheck;
        
        private float coyoteCountdown;
        private float jumpBufferCountdown;
        
        private bool isGrounded;
        private bool isTouchingLeftWall;
        private bool isTouchingRightWall;
        private bool hasDoubleJumped;
        
        private static readonly int IsGrounded = Animator.StringToHash("isGrounded");
        private static readonly int IsRunning = Animator.StringToHash("isRunning");
        private static readonly int DoubleJump = Animator.StringToHash("doubleJump");

        private void Awake()
        {
            InputManager.OnJumpPerformed += HandleJumpPerformed;
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
            
            playerAnimator.SetBool(IsGrounded, isGrounded);

            if (isGrounded)
            {
                coyoteCountdown = coyoteDuration;
                hasDoubleJumped = false;
            }
        }

        private void FixedUpdate()
        {
            var moveAmount = InputManager.MoveAmount;
            var desiredVelocity = moveAmount * moveSpeed;
            
            rigidBody.linearVelocity = moveAmount == 0
                ? new Vector2(Mathf.Lerp(rigidBody.linearVelocityX, 0, deceleration * Time.fixedDeltaTime), rigidBody.linearVelocityY)
                : new Vector2(Mathf.Lerp(rigidBody.linearVelocityX, desiredVelocity, acceleration * Time.fixedDeltaTime), rigidBody.linearVelocityY);
            
            if (rigidBody.linearVelocityY < 0)
            {
                rigidBody.linearVelocity += Vector2.up * (Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime);
            }

            var isMoving = Mathf.Abs(rigidBody.linearVelocityX) > 1e-3f;

            if (isGrounded)
            {
                playerAnimator.SetBool(IsRunning, isMoving);
            }

            if (isMoving)
            {
                var spriteScale = new Vector3(rigidBody.linearVelocityX > 0 ? 1f : -1f, 1f, 1f);
                spriteRendererTransform.localScale = spriteScale;
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
                playerAnimator.SetTrigger(DoubleJump);
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
        }
    }
}
