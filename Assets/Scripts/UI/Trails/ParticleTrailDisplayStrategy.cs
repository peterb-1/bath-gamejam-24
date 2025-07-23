using UnityEngine;

namespace UI.Trails
{
    public class ParticleTrailDisplayStrategy : ITrailDisplayStrategy
    {
        private readonly ParticleSystem particleSystem;

        public ParticleTrailDisplayStrategy(ParticleSystem trailParticles, float scrollSpeed, float lifetime)
        {
            particleSystem = trailParticles;

            var main = particleSystem.main;
            var velocityOverLifetime = particleSystem.velocityOverLifetime;
            
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = main.startColor.color * 0.8f;
            main.startLifetime = lifetime;

            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            velocityOverLifetime.x = -scrollSpeed;

            if (particleSystem.TryGetComponent<ParticleSystemRenderer>(out var renderer))
            {
                renderer.sortingLayerName = "UI";
                renderer.sortingOrder = 1;
            }

            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particleSystem.Play();
        }

        public void Update() {}

        public void EmitTrail()
        {
            if (particleSystem == null) return;

            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particleSystem.Play();
        }

        public void StopEmitting()
        {
            if (particleSystem == null) return;

            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}