using Cysharp.Threading.Tasks;
using Gameplay.Core;
using Gameplay.Player;
using UnityEngine;
using Utils;

namespace Gameplay.Trails
{
    public class GameplayParticleTrailBehaviour : AbstractGameplayTrailBehaviour
    {
        [field: SerializeField] 
        public ParticleSystem TrailParticles { get; private set; }
        
        [Header("Emission")]
        [SerializeField] 
        private float emissionVelocityThreshold;
        
        [SerializeField] 
        private float emissionSpeedMultiplier;

        [SerializeField] 
        private float emissionOffset;

        private PlayerMovementBehaviour playerMovementBehaviour;
        private ParticleSystem.MainModule mainModule;
        private ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime;
        
        private bool isReady;
        
        private async void Awake()
        {
            mainModule = TrailParticles.main;
            mainModule.simulationSpace = ParticleSystemSimulationSpace.World;
            
            velocityOverLifetime = TrailParticles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            
            await UniTask.WaitUntil(PlayerAccessService.IsReady);

            playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;
            isReady = true;
        }
        
        private void Update()
        {
            if (!isReady || (PauseManager.Instance != null && PauseManager.Instance.IsPaused)) return;

            var velocity = playerMovementBehaviour.Velocity;
            
            if (velocity.magnitude < emissionVelocityThreshold)
            {
                if (TrailParticles.isEmitting)
                {
                    TrailParticles.Stop();
                }
                
                return;
            }

            if (!TrailParticles.isEmitting)
            { 
                TrailParticles.Play();
            }

            var velocityDir = velocity.normalized;
            var offset = -velocityDir * emissionOffset;
            
            transform.localPosition = offset.WithZ(0f);
            
            velocityOverLifetime.x = -velocityDir.x * emissionSpeedMultiplier;
            velocityOverLifetime.y = -velocityDir.y * emissionSpeedMultiplier;
        }

        public override void SetColour(Color colour)
        {
            mainModule.startColor = colour;

            var particles = new ParticleSystem.Particle[TrailParticles.particleCount];
            var count = TrailParticles.GetParticles(particles);

            for (var i = 0; i < count; i++)
            {
                particles[i].startColor = colour;
            }

            TrailParticles.SetParticles(particles, count);
        }
    }
}