using System;
using Audio;
using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Core;
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
        private float headJumpForce;
        
        [SerializeField]
        private Vector2 wallJumpForce;

        [SerializeField] 
        private Vector2 ziplineForceMultiplier;

        [SerializeField] 
        private float fallMultiplier;
        
        [SerializeField] 
        private float moveSpeed;
        
        [SerializeField] 
        private float acceleration;

        [SerializeField] 
        private float deceleration;

        [SerializeField] 
        private float runAnimationSpeedThreshold;

        [SerializeField] 
        private float coyoteDuration;

        [SerializeField] 
        private float jumpBufferDuration;
        
        [SerializeField] 
        private float jumpCooldown;

        [SerializeField] 
        private float hookCooldownDuration;

        [SerializeField] 
        private float moveToHookDuration;
        
        [SerializeField] 
        private float springJumpDuration;

        [SerializeField] 
        private Vector3 hookOffset;
        
        [SerializeField]
        private float groundCheckDistance;
        
        [SerializeField]
        private float wallCheckDistance;
        
        [SerializeField]
        private float wallCheckOffset;
        
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
        private Transform leftMidCheck;
        
        [SerializeField]
        private Transform rightMidCheck;
        
        [SerializeField]
        private Transform leftHeadCheck;

        [SerializeField]
        private Transform rightHeadCheck;

        [SerializeField]
        private PlayerDeathBehaviour playerDeathBehaviour;

        [SerializeField]
        private PlayerVictoryBehaviour playerVictoryBehaviour;

        public Vector2 Velocity => isHooked ? ziplineVelocity : rigidBody.linearVelocity;

        private Vector3 ziplineLocalStartOffset;
        private Vector2 lastZiplinePosition;
        private Vector2 ziplineVelocity;

        public bool IsHooked => isHooked;
        
        private float coyoteCountdown;
        private float jumpBufferCountdown;
        private float jumpCooldownCountdown;
        private float hookCountdown;
        private float moveToHookCountdown;
        
        private bool isGrounded;
        private bool isTouchingLeftWall;
        private bool isTouchingRightWall;
        private bool hasDoubleJumped;
        private bool isHooked;
        private bool hasDroppedThisFrame;
        private bool isSpringJumping;

        private HingeJoint2D hook;
        
        private static readonly int IsGrounded = Animator.StringToHash("isGrounded");
        private static readonly int IsRunning = Animator.StringToHash("isRunning");
        private static readonly int IsHookedHash = Animator.StringToHash("isHooked");
        private static readonly int DoubleJump = Animator.StringToHash("doubleJump");

        public event Action OnPlayerHooked;
        public event Action OnPlayerUnhooked;

        private void Awake()
        {
            InputManager.OnJumpPerformed += HandleJumpPerformed;
            InputManager.OnDropPerformed += HandleDropPerformed;
            
            SceneLoader.OnSceneLoadStart += HandleSceneLoadStart;

            playerDeathBehaviour.OnDeathSequenceStart += HandleDeathSequenceStart;
            playerVictoryBehaviour.OnVictorySequenceStart += HandleVictorySequenceStart;

            hasDoubleJumped = true;
        }

        private void Update()
        {
            hasDroppedThisFrame = false;
            
            if (!isGrounded) coyoteCountdown -= Time.deltaTime;
            if (jumpBufferCountdown > 0f) jumpBufferCountdown -= Time.deltaTime;
            if (jumpCooldownCountdown > 0f) jumpCooldownCountdown -= Time.deltaTime;
            if (hookCountdown > 0f) hookCountdown -= Time.deltaTime;

            var wasGrounded = isGrounded;
            var leftGroundPosition = leftGroundCheck.position;
            var rightGroundPosition = rightGroundCheck.position;

            var doesRaycastUpHit = Physics2D.Raycast(leftGroundPosition, Vector2.up, groundCheckDistance, groundLayers) || 
                                   Physics2D.Raycast(rightGroundPosition, Vector2.up, groundCheckDistance, groundLayers);
            
            var doesRaycastDownHit = Physics2D.Raycast(leftGroundPosition, Vector2.down, groundCheckDistance, groundLayers) ||
                                     Physics2D.Raycast(rightGroundPosition, Vector2.down, groundCheckDistance, groundLayers);
            
            var doesRaycastLeftHit = Physics2D.Raycast(leftGroundPosition, Vector2.left, groundCheckDistance, groundLayers) || 
                                     Physics2D.Raycast(leftMidCheck.position, Vector2.left, groundCheckDistance, groundLayers) ||
                                     Physics2D.Raycast(leftHeadCheck.position, Vector2.left, groundCheckDistance, groundLayers);
            
            var doesRaycastRightHit = Physics2D.Raycast(rightGroundPosition, Vector2.right, groundCheckDistance, groundLayers) ||
                                      Physics2D.Raycast(rightMidCheck.position, Vector2.right, groundCheckDistance, groundLayers) ||
                                      Physics2D.Raycast(rightHeadCheck.position, Vector2.right, groundCheckDistance, groundLayers);

            var leftWallProbe = Physics2D.OverlapCircle(leftGroundPosition + Vector3.right * wallCheckOffset, wallCheckDistance, groundLayers);
            var rightWallProbe = Physics2D.OverlapCircle(rightGroundPosition + Vector3.left * wallCheckOffset, wallCheckDistance, groundLayers);
            
            isGrounded = doesRaycastDownHit && !doesRaycastUpHit;
            isTouchingLeftWall = doesRaycastLeftHit && !leftWallProbe;
            isTouchingRightWall = doesRaycastRightHit && !rightWallProbe;
            
            playerAnimator.SetBool(IsGrounded, isGrounded);

            if (isGrounded || isHooked)
            {
                coyoteCountdown = coyoteDuration;
                hasDoubleJumped = false;
            }

            if (isGrounded && !wasGrounded)
            {
                AudioManager.Instance.Play(AudioClipIdentifier.Land);
            }

            if (isHooked)
            {
                var currentPosition = hook.transform.position.xy();

                ziplineVelocity = currentPosition == lastZiplinePosition 
                    ? Vector2.zero 
                    : (currentPosition - lastZiplinePosition) / Time.deltaTime;
                
                lastZiplinePosition = currentPosition;

                if (moveToHookCountdown > 0)
                {
                    moveToHookCountdown -= Time.deltaTime;
                    
                    var lerp = (moveToHookDuration - moveToHookCountdown) / moveToHookDuration;
                    
                    transform.localPosition = Vector3.Lerp(ziplineLocalStartOffset, hookOffset,  Mathf.Clamp(lerp, 0f, 1f));
                }
            }
        }

        private void FixedUpdate()
        {
            var moveAmount = InputManager.MoveAmount;
            var desiredVelocity = moveAmount * moveSpeed;

            if (!isHooked)
            {
                rigidBody.linearVelocity = moveAmount == 0f
                    ? new Vector2(Mathf.Lerp(rigidBody.linearVelocityX, 0f, deceleration * Time.fixedDeltaTime), rigidBody.linearVelocityY)
                    : new Vector2(Mathf.Lerp(rigidBody.linearVelocityX, desiredVelocity, acceleration * Time.fixedDeltaTime), rigidBody.linearVelocityY);
            }

            if (rigidBody.linearVelocityY < 0f)
            {
                rigidBody.linearVelocity += Vector2.up * (Physics2D.gravity.y * (fallMultiplier - 1f) * Time.deltaTime);
            }

            var isMoving = Mathf.Abs(rigidBody.linearVelocityX) > runAnimationSpeedThreshold;

            if (isGrounded)
            {
                playerAnimator.SetBool(IsRunning, isMoving);
            }

            if (isMoving || isHooked)
            {
                var isMovingRight = (isMoving && rigidBody.linearVelocityX > 0f) ||
                                    (isHooked && ziplineVelocity.x > 0f);
                
                var spriteScale = new Vector3(isMovingRight ? 1f : -1f, 1f, 1f);
                spriteRendererTransform.localScale = spriteScale;
            }

            if (jumpBufferCountdown > 0f && !isSpringJumping)
            {
                TryJump();
            }
        }
        
        private void HandleJumpPerformed()
        {
            if (hasDroppedThisFrame) return;
            
            jumpBufferCountdown = jumpBufferDuration;
        }
        
        private void HandleDropPerformed()
        {
            hasDroppedThisFrame = TryUnhookPlayer();
        }
        
        private void TryJump()
        {
            if (jumpCooldownCountdown <= 0f && (isGrounded || isHooked || coyoteCountdown > 0f))
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
                PerformJump(Mathf.Max(doubleJumpForce, rigidBody.linearVelocityY));
                hasDoubleJumped = true;
                playerAnimator.SetTrigger(DoubleJump);
            }
        }
        
        private void PerformJump(float force)
        {
            TryUnhookPlayer();
            
            AudioManager.Instance.Play(AudioClipIdentifier.Jump);
            rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocityX, force);
            
            jumpBufferCountdown = 0f;
            coyoteCountdown = 0f;
            jumpCooldownCountdown = jumpCooldown;
        }

        private void PerformWallJump(Vector2 force)
        {
            AudioManager.Instance.Play(AudioClipIdentifier.Jump);
            rigidBody.linearVelocity = force;
            
            jumpBufferCountdown = 0f;
            coyoteCountdown = 0f;
        }

        public void PerformHeadJump()
        {
            AudioManager.Instance.Play(AudioClipIdentifier.Jump);
            rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocityX, headJumpForce);
        }
        
        public void PerformSpringJump(float radians, Vector2 minBounce, float verticalDamping)
        {
            if (isHooked) return;
            
            var parallelComponent = Velocity.x * Mathf.Cos(radians) + Velocity.y * Mathf.Sin(radians);
            var perpendicularComponent = -Velocity.x * Mathf.Sin(radians) + Velocity.y * Mathf.Cos(radians);
            
            var horizontalComponent = parallelComponent * Mathf.Cos(radians) - perpendicularComponent * Mathf.Sin(radians);
            var verticalComponent = verticalDamping * (parallelComponent * Mathf.Sin(radians) - perpendicularComponent * Mathf.Cos(radians));

            if (Mathf.Abs(minBounce.x) > 1e-3f)
            {
                horizontalComponent = minBounce.x > 0
                    ? Mathf.Max(horizontalComponent, minBounce.x)
                    : Mathf.Min(-horizontalComponent, minBounce.x);
            }

            if (Mathf.Abs(minBounce.y) > 1e-3f)
            {
                verticalComponent = minBounce.y > 0
                    ? Mathf.Max(verticalComponent, minBounce.y)
                    : Mathf.Min(-verticalComponent, minBounce.y);
            }
            
            var targetVelocity = Vector2.right * horizontalComponent + Vector2.up * verticalComponent;
            
            AudioManager.Instance.Play(AudioClipIdentifier.SpringJump);
            
            SmoothSpringJumpAsync(targetVelocity).Forget();
        }

        private async UniTask SmoothSpringJumpAsync(Vector2 targetVelocity)
        {
            isSpringJumping = true;
            
            var initialTime = TimeManager.Instance.UnpausedRealtimeSinceStartup;
            var startVelocity = rigidBody.linearVelocity;
            var timeElapsed = 0f;

            // run independent of timescale, since this happens during the slowdown
            while (timeElapsed < springJumpDuration)
            {
                var lerp = timeElapsed / springJumpDuration;

                rigidBody.linearVelocity = Vector2.Lerp(startVelocity, targetVelocity, lerp);
                
                await UniTask.Yield();
                
                timeElapsed = TimeManager.Instance.UnpausedRealtimeSinceStartup - initialTime;
            }
            
            rigidBody.linearVelocity = targetVelocity;
            hasDoubleJumped = false;
            isSpringJumping = false;
        }

        public bool TryHookPlayer(HingeJoint2D newHook)
        {
            if (isHooked || (hookCountdown > 0f && hook == newHook) || !playerDeathBehaviour.IsAlive)
            {
                return false;
            }
            
            AudioManager.Instance.Play(AudioClipIdentifier.ZiplineAttach);
            
            hook = newHook;
            isHooked = true;
            hookCountdown = 0f;
            moveToHookCountdown = moveToHookDuration;

            var trans = transform;
            var hookTransform = hook.transform;
            var oldWorldPosition = trans.position;
            var hookPosition = hookTransform.position;

            hook.connectedBody = rigidBody;
            rigidBody.linearVelocity = Vector2.zero;
            rigidBody.gravityScale = 0f;
            
            lastZiplinePosition = hookPosition.xy();
            ziplineLocalStartOffset = oldWorldPosition - hookPosition;
            
            trans.parent = hookTransform;
            trans.localPosition = ziplineLocalStartOffset;
            
            playerAnimator.SetBool(IsHookedHash, true);
            
            OnPlayerHooked?.Invoke();

            return true;
        }

        public bool TryUnhookPlayer(Vector2? unhookVelocity = null)
        {
            if (!isHooked) return false;
            
            OnPlayerUnhooked?.Invoke();
            
            AudioManager.Instance.Stop(AudioClipIdentifier.ZiplineAttach);
            AudioManager.Instance.Play(AudioClipIdentifier.ZiplineDetach);
            
            isHooked = false;
            hookCountdown = hookCooldownDuration;
            hook.transform.Rotate(Vector3.forward, -transform.eulerAngles.z);
            hook.connectedBody = null;
            transform.parent = null;

            if (unhookVelocity != null) ziplineVelocity = unhookVelocity.Value;

            rigidBody.linearVelocity = ziplineVelocity * ziplineForceMultiplier;
            rigidBody.gravityScale = 1f;
            
            playerAnimator.SetBool(IsHookedHash, false);

            return true;
        }
        
        private void HandleSceneLoadStart()
        {
            rigidBody.linearVelocity = Vector2.zero;
            rigidBody.gravityScale = 0f;
        }
        
        private void HandleDeathSequenceStart()
        {
            TryUnhookPlayer();
            
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
            InputManager.OnDropPerformed -= HandleDropPerformed;
            
            SceneLoader.OnSceneLoadStart -= HandleSceneLoadStart;
            
            playerDeathBehaviour.OnDeathSequenceStart -= HandleDeathSequenceStart;
            playerVictoryBehaviour.OnVictorySequenceStart -= HandleVictorySequenceStart;
        }
    }
}
