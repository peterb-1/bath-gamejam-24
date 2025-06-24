using System;
using Audio;
using Core;
using Core.Saving;
using Cysharp.Threading.Tasks;
using Gameplay.Core;
using Gameplay.Dash;
using Gameplay.Input;
using Hardware;
using UnityEngine;
using Utils;

namespace Gameplay.Player
{
    public class PlayerMovementBehaviour : MonoBehaviour
    {
        [Header("Forces and Multipliers")]
        [SerializeField] 
        private float jumpForce; 

        [SerializeField] 
        private float doubleJumpForce;
        
        [SerializeField] 
        private float headJumpForce;

        [SerializeField] 
        private float dashForce;
        
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
        private float wallJumpDeceleration;
        
        [SerializeField] 
        private float clingFallMultiplier;
        
        [SerializeField] 
        private float clingVelocityMultiplier;

        [Header("Durations")]
        [SerializeField] 
        private float coyoteDuration;

        [SerializeField] 
        private float jumpBufferDuration;
        
        [SerializeField] 
        private float jumpCooldown;
        
        [SerializeField] 
        private float wallJumpDecelerationDuration;
        
        [SerializeField] 
        private float wallEjectionDuration;

        [SerializeField] 
        private float doubleJumpCancellationDuration;
        
        [SerializeField] 
        private float clingDuration;

        [SerializeField] 
        private float hookCooldownDuration;

        [SerializeField] 
        private float moveToHookDuration;
        
        [SerializeField] 
        private float maxLedgeAssistDuration;
        
        [SerializeField] 
        private float springJumpDuration;

        [SerializeField] 
        private float dashDuration;

        [Header("Offsets and Distances")]
        [SerializeField] 
        private Vector3 hookOffset;
        
        [SerializeField] 
        private Vector3 dashDistortionOffset;
        
        [SerializeField] 
        private Vector2 ledgeFinishOffset;
        
        [SerializeField]
        private float groundCheckDistance;
        
        [SerializeField]
        private float wallCheckDistance;
        
        [SerializeField]
        private float wallCheckOffset;
        
        [Header("Rumble")]
        [SerializeField] 
        private RumbleConfig jumpingRumbleConfig;

        [SerializeField] 
        private RumbleConfig landingRumbleConfig;
        
        [SerializeField] 
        private RumbleConfig droneHitRumbleConfig;

        [SerializeField] 
        private ContinuousRumbleConfig ziplineRumbleConfig;
        
        [Header("Misc")]
        [SerializeField] 
        private float runAnimationSpeedThreshold;
        
        [SerializeField] 
        private float dashDistortionStrength;
        
        [SerializeField] 
        private float dashDistortionSmoothing;
        
        [SerializeField]
        private LayerMask groundLayers;

        [SerializeField] 
        private AnimationCurve moveTowardsTargetCurve;

        [Header("References")]
        [SerializeField] 
        private Rigidbody2D rigidBody;
        
        [SerializeField] 
        private BoxCollider2D boxCollider;
        
        [SerializeField] 
        private BoxCollider2D deathCollider;

        [SerializeField] 
        private Transform spriteRendererTransform;
        
        [SerializeField] 
        private SpriteRenderer dashDistortionRenderer;

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
        public bool IsDashing => dashCountdown > 0f;
        public bool IsHooked => isHooked;

        private Vector3 ziplineLocalStartOffset;
        private Vector2 lastZiplinePosition;
        private Vector2 ziplineVelocity;
        private Vector2 lastPosition;

        private float dashDirectionMultiplier;
        private float currentFallMultiplier;
        private float currentDashDistortion;
        
        private float coyoteCountdown;
        private float jumpBufferCountdown;
        private float jumpCooldownCountdown;
        private float dashCountdown;
        private float wallJumpDecelerationCountdown;
        private float wallEjectionCountdown;
        private float doubleJumpCancellationCountdown;
        private float clingCountdown;
        private float hookCountdown;
        private float moveToHookCountdown;
        
        private bool isGrounded;
        private bool isTouchingLeftWall;
        private bool isTouchingRightWall;
        private bool hasDoubleJumped;
        private bool isHooked;
        private bool isClinging;
        private bool isClimbingLedge;
        private bool hasDroppedThisFrame;
        private bool isSpringJumping;
        private bool wasEjectedLeft;
        private bool hasLandedAtStart;
        private bool hasMovedLeft;

        private HingeJoint2D hook;
        
        private static readonly int IsGrounded = Animator.StringToHash("isGrounded");
        private static readonly int IsRunning = Animator.StringToHash("isRunning");
        private static readonly int IsHookedHash = Animator.StringToHash("isHooked");
        private static readonly int DoubleJump = Animator.StringToHash("doubleJump");
        private static readonly int CancelDoubleJump = Animator.StringToHash("cancelDoubleJump");
        private static readonly int Strength = Shader.PropertyToID("_Strength");

        public event Action OnLanded;
        public event Action OnWallJump;
        public event Action OnPlayerHooked;
        public event Action OnPlayerUnhooked;
        public event Action OnPlayerDashedIntoLaser;
        public event Action OnMissionCompleteWithoutMovingLeft;

        private void Awake()
        {
            InputManager.OnJumpPerformed += HandleJumpPerformed;
            InputManager.OnDropPerformed += HandleDropPerformed;
            SceneLoader.OnSceneLoadStart += HandleSceneLoadStart;
            DashTrackerService.OnDashUsed += HandleDashUsed;

            playerDeathBehaviour.OnDeathSequenceStart += HandleDeathSequenceStart;
            playerVictoryBehaviour.OnVictorySequenceStart += HandleVictorySequenceStart;

            isGrounded = true;
            hasDoubleJumped = true;
            currentFallMultiplier = fallMultiplier;
        }

        private void Update()
        {
            hasDroppedThisFrame = false;
            
            if (!isGrounded) coyoteCountdown -= Time.deltaTime;
            if (jumpBufferCountdown > 0f) jumpBufferCountdown -= Time.deltaTime;
            if (jumpCooldownCountdown > 0f) jumpCooldownCountdown -= Time.deltaTime;
            if (dashCountdown > 0f) dashCountdown -= Time.deltaTime;
            if (wallJumpDecelerationCountdown > 0f) wallJumpDecelerationCountdown -= Time.deltaTime;
            if (wallEjectionCountdown > 0f) wallEjectionCountdown -= Time.deltaTime;
            if (doubleJumpCancellationCountdown > 0f) doubleJumpCancellationCountdown -= Time.deltaTime;
            if (clingCountdown > 0f) clingCountdown -= Time.deltaTime;
            if (hookCountdown > 0f) hookCountdown -= Time.deltaTime;

            if (isClimbingLedge) return;

            var wasGrounded = isGrounded;
            var leftGroundPosition = leftGroundCheck.position;
            var rightGroundPosition = rightGroundCheck.position;

            var doesRaycastUpHit = Physics2D.Raycast(leftGroundPosition, Vector2.up, groundCheckDistance, groundLayers) || 
                                   Physics2D.Raycast(rightGroundPosition, Vector2.up, groundCheckDistance, groundLayers);
            
            var downLeftHit = Physics2D.Raycast(leftGroundPosition, Vector2.down, groundCheckDistance, groundLayers);
            var downRightHit = Physics2D.Raycast(rightGroundPosition, Vector2.down, groundCheckDistance, groundLayers);
            var leftGroundHit = Physics2D.Raycast(leftGroundPosition, Vector2.left, groundCheckDistance, groundLayers);
            var leftMidHit = Physics2D.Raycast(leftMidCheck.position, Vector2.left, groundCheckDistance, groundLayers);
            var leftHeadHit = Physics2D.Raycast(leftHeadCheck.position, Vector2.left, groundCheckDistance, groundLayers);
            var rightGroundHit = Physics2D.Raycast(rightGroundPosition, Vector2.right, groundCheckDistance, groundLayers);
            var rightMidHit = Physics2D.Raycast(rightMidCheck.position, Vector2.right, groundCheckDistance, groundLayers);
            var rightHeadHit = Physics2D.Raycast(rightHeadCheck.position, Vector2.right, groundCheckDistance, groundLayers);
            
            var doesRaycastDownHit = downLeftHit || downRightHit;
            var doesRaycastLeftHit = leftGroundHit || leftMidHit || leftHeadHit;
            var doesRaycastRightHit = rightGroundHit || rightMidHit || rightHeadHit;

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
                hasLandedAtStart = true;
            }

            if (isGrounded && !wasGrounded)
            {
                AudioManager.Instance.Play(AudioClipIdentifier.Land);
                RumbleManager.Instance.Rumble(landingRumbleConfig);
                
                OnLanded?.Invoke();
            }

            if (isHooked)
            {
                HookUpdate();
            }

            DashUpdate();

            WallUpdate();

            if (jumpBufferCountdown <= 0f)
            {
                var isOnLeftLedge = leftGroundHit && !leftMidHit;
                var isOnRightLedge = rightGroundHit && !rightMidHit;

                // make sure we're on exactly ONE ledge - if it's both, we're in the middle of a block and will be pushed out as normal
                if (isOnLeftLedge != isOnRightLedge)
                {
                    if (isOnLeftLedge)
                    {
                        TryClimbLedgeAsync(true, leftGroundHit, leftMidHit).Forget();
                    }
                    else
                    {
                        TryClimbLedgeAsync(false,  rightGroundHit, rightMidHit).Forget();
                    }
                }
            }
            
            var position = transform.position.xy();
            SaveManager.Instance.SaveData.StatsData.AddToStat(StatType.DistanceCovered, (position - lastPosition).magnitude);
            lastPosition = position;
        }

        private void WallUpdate()
        {
            if (isTouchingLeftWall || isTouchingRightWall)
            {
                if (!isClinging)
                {
                    if (doubleJumpCancellationCountdown > 0f && wallEjectionCountdown > 0f)
                    {
                        ReplaceDoubleJumpWithWallJump(isMovingLeft: isTouchingRightWall);
                    }
                    else
                    {
                        isClinging = true;
                        currentFallMultiplier = clingFallMultiplier;
                        clingCountdown = clingDuration;
                        dashCountdown = 0f;
                    
                        if (rigidBody.linearVelocityY < 0f)
                        {
                            rigidBody.linearVelocityY *= clingVelocityMultiplier;
                        }
                    }
                }
                else if (clingCountdown <= 0f)
                {
                    currentFallMultiplier = fallMultiplier;
                }
            }
            else
            {
                currentFallMultiplier = fallMultiplier;
                isClinging = false;
            }
        }
        
        private void DashUpdate()
        {
            var targetDistortionStrength = dashCountdown > 0f ? dashDistortionStrength * dashDirectionMultiplier : 0f;
            var lerpDistortion = Mathf.Lerp(currentDashDistortion, targetDistortionStrength, dashDistortionSmoothing);

            dashDistortionRenderer.color = Color.Lerp(new Color(1f, 1f, 1f, 0f), Color.white, Mathf.Abs(lerpDistortion) / dashDistortionStrength);
            dashDistortionRenderer.material.SetFloat(Strength, lerpDistortion);
            currentDashDistortion = lerpDistortion;
        }

        private void HookUpdate()
        {
            var currentPosition = hook.transform.position.xy();

            ziplineVelocity = currentPosition == lastZiplinePosition 
                ? Vector2.zero 
                : (currentPosition - lastZiplinePosition) / Time.deltaTime;
                
            lastZiplinePosition = currentPosition;

            if (moveToHookCountdown > 0f)
            {
                moveToHookCountdown -= Time.deltaTime;
                    
                var lerp = (moveToHookDuration - moveToHookCountdown) / moveToHookDuration;
                    
                transform.localPosition = Vector3.Lerp(ziplineLocalStartOffset, hookOffset,  Mathf.Clamp(lerp, 0f, 1f));
            }
            
            SaveManager.Instance.SaveData.StatsData.AddToStat(StatType.ZiplineTime, Time.deltaTime);
        }
        
        private void FixedUpdate()
        {
            if (isClimbingLedge) return;
            
            var moveAmount = InputManager.MoveAmount;
            var desiredVelocity = moveAmount * moveSpeed;

            if (moveAmount < 0f)
            {
                hasMovedLeft = true;
            }

            if (dashCountdown > 0f)
            {
                rigidBody.linearVelocity = new Vector2(dashForce * dashDirectionMultiplier, 0f);
            }
            else if (!isHooked)
            {
                var targetDeceleration = wallJumpDecelerationCountdown > 0f ? wallJumpDeceleration : deceleration;

                rigidBody.linearVelocity = moveAmount != 0f
                    ? new Vector2(Mathf.Lerp(rigidBody.linearVelocityX, desiredVelocity, acceleration * Time.fixedDeltaTime), rigidBody.linearVelocityY)
                    : new Vector2(Mathf.Lerp(rigidBody.linearVelocityX, 0f, targetDeceleration * Time.fixedDeltaTime), rigidBody.linearVelocityY);
            }

            if (rigidBody.linearVelocityY < 0f)
            {
                rigidBody.linearVelocity += Vector2.up * (Physics2D.gravity.y * (currentFallMultiplier - 1f) * Time.deltaTime);
            }

            var isMoving = Mathf.Abs(rigidBody.linearVelocityX) > runAnimationSpeedThreshold;

            if (isGrounded)
            {
                playerAnimator.SetBool(IsRunning, isMoving);
            }

            if (isMoving || isHooked)
            {
                var isMovingRight = (isMoving && rigidBody.linearVelocityX > 0f) || (isHooked && ziplineVelocity.x > 0f);
                var spriteScale = new Vector3(isMovingRight ? 1f : -1f, 1f, 1f);
                
                spriteRendererTransform.localScale = spriteScale;
            }

            if (jumpBufferCountdown > 0f && !isSpringJumping && hasLandedAtStart)
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
            else if (!wasEjectedLeft && wallEjectionCountdown > 0f)
            {
                PerformWallJump(new Vector2(wallJumpForce.x, wallJumpForce.y));
            }
            else if (wasEjectedLeft && wallEjectionCountdown > 0f)
            {
                PerformWallJump(new Vector2(-wallJumpForce.x, wallJumpForce.y));
            }
            else if (!hasDoubleJumped)
            {
                PerformJump(Mathf.Max(doubleJumpForce, rigidBody.linearVelocityY));
                hasDoubleJumped = true;
                doubleJumpCancellationCountdown = doubleJumpCancellationDuration;
                playerAnimator.ResetTrigger(CancelDoubleJump);
                playerAnimator.SetTrigger(DoubleJump);
            }
        }
        
        private void PerformJump(float force)
        {
            TryUnhookPlayer();
            
            AudioManager.Instance.Play(AudioClipIdentifier.Jump);
            RumbleManager.Instance.Rumble(jumpingRumbleConfig);
            SaveManager.Instance.SaveData.StatsData.AddToStat(StatType.JumpsMade, 1);
            
            rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocityX, force);
            
            jumpCooldownCountdown = jumpCooldown;
            jumpBufferCountdown = 0f;
            coyoteCountdown = 0f;
            wallEjectionCountdown = 0f;
            dashCountdown = 0f;
        }

        private void PerformWallJump(Vector2 force, bool shouldTriggerEffects = true)
        {
            if (shouldTriggerEffects)
            {
                AudioManager.Instance.Play(AudioClipIdentifier.Jump);
                RumbleManager.Instance.Rumble(jumpingRumbleConfig);
                SaveManager.Instance.SaveData.StatsData.AddToStat(StatType.JumpsMade, 1);
            }
            
            rigidBody.linearVelocity = force;

            wallJumpDecelerationCountdown = wallJumpDecelerationDuration;
            jumpBufferCountdown = 0f;
            coyoteCountdown = 0f;
            wallEjectionCountdown = 0f;
            dashCountdown = 0f;
            
            OnWallJump?.Invoke();
        }

        public void PerformHeadJump()
        {
            AudioManager.Instance.Play(AudioClipIdentifier.Jump);
            RumbleManager.Instance.Rumble(droneHitRumbleConfig);
            
            rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocityX, headJumpForce);
            dashCountdown = 0f;
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

        private void ReplaceDoubleJumpWithWallJump(bool isMovingLeft)
        {
            if (isMovingLeft)
            {
                PerformWallJump(new Vector2(-wallJumpForce.x, wallJumpForce.y), shouldTriggerEffects: false);
            }
            else
            {
                PerformWallJump(new Vector2(wallJumpForce.x, wallJumpForce.y), shouldTriggerEffects: false);
            }
            
            hasDoubleJumped = false;
            playerAnimator.SetTrigger(CancelDoubleJump);
            doubleJumpCancellationCountdown = 0f;
        }
        
        private void HandleDashUsed(int _)
        {
            dashDirectionMultiplier = Mathf.Sign(spriteRendererTransform.localScale.x);
            dashCountdown = dashDuration;

            dashDistortionRenderer.transform.localPosition = dashDistortionOffset * dashDirectionMultiplier;
        }

        public bool TryHookPlayer(HingeJoint2D newHook)
        {
            if (isHooked || (hookCountdown > 0f && hook == newHook) || !playerDeathBehaviour.IsAlive)
            {
                return false;
            }
            
            AudioManager.Instance.Play(AudioClipIdentifier.ZiplineAttach);
            
            RumbleManager.Instance.Rumble(ziplineRumbleConfig);
            
            dashCountdown = 0f;
            
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
            
            RumbleManager.Instance.StopRumble();
            
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
        
        public void NotifyEjectedFromBuilding(Bounds buildingBounds)
        {
            var playerBounds = boxCollider.bounds;

            var min = buildingBounds.min;
            var max = buildingBounds.max;
            var playerMin = playerBounds.min;
            var playerMax = playerBounds.max;

            var leftDistance = playerMax.x - min.x;
            var rightDistance = max.x - playerMin.x;
            var downDistance = playerMax.y - min.y;
            var upDistance = max.y - playerMin.y;

            wasEjectedLeft = leftDistance < rightDistance;

            // we only care about sideways ejections
            if (Mathf.Min(leftDistance, rightDistance) < Mathf.Min(downDistance, upDistance))
            {
                wallEjectionCountdown = wallEjectionDuration;

                if (doubleJumpCancellationCountdown > 0f && (!isTouchingLeftWall || !isTouchingRightWall))
                {
                    ReplaceDoubleJumpWithWallJump(wasEjectedLeft);
                }
            }
        }
        
        private async UniTask TryClimbLedgeAsync(bool isLeft, RaycastHit2D groundHit, RaycastHit2D midHit)
        {
            var ledgeCollider = groundHit.collider;
            
            if (ledgeCollider == null)
            {
                ledgeCollider = midHit.collider;
            }

            if (spriteRendererTransform.localScale.x > 0f == isLeft) return;
            
            isClimbingLedge = true;
            
            var ledgeBounds = ledgeCollider.bounds;
            var ledgeCornerPosition = isLeft ? ledgeBounds.max.xy() : new Vector2(ledgeBounds.min.x, ledgeBounds.max.y);
            var finishPosition = ledgeCornerPosition + (isLeft ? new Vector2(-ledgeFinishOffset.x, ledgeFinishOffset.y) : ledgeFinishOffset);
            
            rigidBody.gravityScale = 0f;
            boxCollider.enabled = false;

            var timeElapsed = 0f;
            var trans = transform;
            var startPosition = trans.position.xy();
            var targetDuration = (finishPosition - startPosition).magnitude / rigidBody.linearVelocity.magnitude;
            var duration = Mathf.Min(maxLedgeAssistDuration, targetDuration);
            
            while (timeElapsed < duration)
            {
                var lerp = moveTowardsTargetCurve.Evaluate(timeElapsed / duration);

                trans.position = Vector2.Lerp(startPosition, finishPosition, lerp);

                await UniTask.Yield();

                timeElapsed += Time.deltaTime;
            }

            trans.position = finishPosition;
            
            rigidBody.gravityScale = 1f;
            boxCollider.enabled = true;
            isClimbingLedge = false;
        }

        private void HandleSceneLoadStart()
        {
            rigidBody.linearVelocity = Vector2.zero;
            rigidBody.gravityScale = 0f;
        }
        
        private void HandleDeathSequenceStart(PlayerDeathSource source)
        {
            if (dashCountdown > 0f && source is PlayerDeathSource.Laser)
            {
                OnPlayerDashedIntoLaser?.Invoke();
            }
            
            dashCountdown = 0f;
            
            TryUnhookPlayer();
            
            rigidBody.linearVelocity = Vector2.zero;
            rigidBody.gravityScale = 0f;
        }

        private void HandleVictorySequenceStart(Vector2 position, float duration)
        {
            if (!hasMovedLeft)
            {
                OnMissionCompleteWithoutMovingLeft?.Invoke();
            }
            
            dashCountdown = 0f;

            deathCollider.enabled = false;
            
            ShrinkInBlackHoleAsync(position, duration).Forget();
        }

        private async UniTask ShrinkInBlackHoleAsync(Vector2 targetPosition, float duration)
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
            DashTrackerService.OnDashUsed -= HandleDashUsed;
            
            playerDeathBehaviour.OnDeathSequenceStart -= HandleDeathSequenceStart;
            playerVictoryBehaviour.OnVictorySequenceStart -= HandleVictorySequenceStart;
        }
    }
}
