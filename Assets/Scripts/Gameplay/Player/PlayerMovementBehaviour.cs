using Audio;
using Cysharp.Threading.Tasks;
using Gameplay.Input;
using UnityEngine;
using Utils;

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

        [SerializeField] 
        private AnimationCurve moveTowardsTargetCurve;

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

        [SerializeField]
        private PlayerDeathBehaviour playerDeathBehaviour;

        [SerializeField]
        private PlayerVictoryBehaviour playerVictoryBehaviour;
        
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

            playerDeathBehaviour.OnDeathSequenceStart += HandleDeathSequenceStart;

            playerVictoryBehaviour.OnVictorySequenceStart += HandleVictorySequenceStart;
        }

        private void Update()
        {
            if (!isGrounded) coyoteCountdown -= Time.deltaTime;
            if (jumpBufferCountdown > 0f) jumpBufferCountdown -= Time.deltaTime;

            var trans = transform;
            var up = trans.up;
            var down = -up;
            var right = trans.right;
            var left = -right;
            var leftGroundPosition = leftGroundCheck.position;
            var rightGroundPosition = rightGroundCheck.position;

            var doesRaycastUpHit = Physics2D.Raycast(leftGroundPosition, up, groundCheckDistance, groundLayers) ||
                                   Physics2D.Raycast(rightGroundPosition, up, groundCheckDistance, groundLayers);
            
            var doesRaycastDownHit = Physics2D.Raycast(leftGroundPosition, down, groundCheckDistance, groundLayers) ||
                         Physics2D.Raycast(rightGroundPosition, down, groundCheckDistance, groundLayers);

            isGrounded = doesRaycastDownHit && !doesRaycastUpHit;
            
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
            
            rigidBody.linearVelocity = moveAmount == 0f
                ? new Vector2(Mathf.Lerp(rigidBody.linearVelocityX, 0f, deceleration * Time.fixedDeltaTime), rigidBody.linearVelocityY)
                : new Vector2(Mathf.Lerp(rigidBody.linearVelocityX, desiredVelocity, acceleration * Time.fixedDeltaTime), rigidBody.linearVelocityY);
            
            if (rigidBody.linearVelocityY < 0f)
            {
                rigidBody.linearVelocity += Vector2.up * (Physics2D.gravity.y * (fallMultiplier - 1f) * Time.deltaTime);
            }

            var isMoving = Mathf.Abs(rigidBody.linearVelocityX) > 1e-3f;

            if (isGrounded)
            {
                playerAnimator.SetBool(IsRunning, isMoving);
            }

            if (isMoving)
            {
                var spriteScale = new Vector3(rigidBody.linearVelocityX > 0f ? 1f : -1f, 1f, 1f);
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
            if (jumpBufferCountdown <= 0f) return;
            
            if (isGrounded || coyoteCountdown > 0f)
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
            AudioManager.Instance.Play(AudioClipIdentifier.Jump);
            rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocityX, force);
            jumpBufferCountdown = 0f;
        }

        private void PerformWallJump(Vector2 force)
        {
            AudioManager.Instance.Play(AudioClipIdentifier.Jump);
            rigidBody.linearVelocity = force;
            hasDoubleJumped = true;
            jumpBufferCountdown = 0f;
        }
        
        private void HandleDeathSequenceStart()
        {
            rigidBody.linearVelocity = Vector2.zero;
            rigidBody.gravityScale = 0f;
        }

        private void HandleVictorySequenceStart(Vector2 position, float duration)
        {
            MoveToTargetAsync(position, duration).Forget();
        }

        private async UniTask MoveToTargetAsync(Vector2 targetPosition, float duration)
        {
            rigidBody.gravityScale = 0f;
            
            var timeElapsed = 0f;
            var trans = transform;
            var startPosition = trans.position.xy();
            var currentVelocity = rigidBody.linearVelocity;
            
            while (timeElapsed < duration)
            {
                var lerp = moveTowardsTargetCurve.Evaluate(timeElapsed / duration);

                var positionFromVelocity = startPosition + currentVelocity * (timeElapsed * (1 - lerp));
                var targetContribution = Vector2.Lerp(startPosition, targetPosition, lerp);

                trans.position = Vector2.Lerp(positionFromVelocity, targetContribution, lerp);
                trans.localScale = (1.0f - lerp) * Vector3.one;

                await UniTask.Yield();

                timeElapsed += Time.deltaTime;
            }

            trans.position = targetPosition;
            
            rigidBody.linearVelocity = Vector2.zero;
        }

        private void OnDestroy()
        {
            InputManager.OnJumpPerformed -= HandleJumpPerformed;
            
            playerDeathBehaviour.OnDeathSequenceStart -= HandleDeathSequenceStart;
        }
    }
}
