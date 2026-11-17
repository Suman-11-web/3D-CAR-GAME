using System;
using UnityEngine;
using CarGame.CameraSystem;
using CarGame.Audio;

namespace CarGame.Damage
{
    /// <summary>
    /// Handles collision impacts, physical damage durability,
    /// camera shake impulses, audio triggers, and repair actions.
    /// </summary>
    public class VehicleDamageSystem : MonoBehaviour
    {
        [Header("Durability Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private float minImpactVelocityThreshold = 3.5f;
        [SerializeField] private float damageMultiplier = 1.8f;

        [Header("Damage Visuals")]
        [SerializeField] private ParticleSystem engineSmokeParticle;
        [SerializeField] private float smokeHealthThreshold = 45f;

        [Header("Camera & Audio Feedback")]
        [SerializeField] private CameraController cameraController;
        [SerializeField] private VehicleAudioSystem audioSystem;
        [SerializeField] private float maxShakeIntensity = 0.85f;

        public event Action<float> OnHealthChanged;

        public float HealthPercent => Mathf.Clamp01(currentHealth / maxHealth);
        public bool IsWrecked => currentHealth <= 0f;

        private void Start()
        {
            if (cameraController == null) cameraController = FindObjectOfType<CameraController>();
            if (audioSystem == null) audioSystem = GetComponent<VehicleAudioSystem>();

            currentHealth = maxHealth;
            if (engineSmokeParticle != null)
            {
                var emission = engineSmokeParticle.emission;
                emission.enabled = false;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            float impactSpeed = collision.relativeVelocity.magnitude;
            if (impactSpeed < minImpactVelocityThreshold) return;

            // 1. Calculate Damage
            float damageAmount = (impactSpeed - minImpactVelocityThreshold) * damageMultiplier;
            currentHealth = Mathf.Max(currentHealth - damageAmount, 0f);
            OnHealthChanged?.Invoke(HealthPercent);

            // 2. Camera Shake Feedback
            if (cameraController != null)
            {
                float shakeRatio = Mathf.Clamp01(impactSpeed / 30f);
                float duration = Mathf.Lerp(0.15f, 0.45f, shakeRatio);
                cameraController.TriggerShake(shakeRatio * maxShakeIntensity, duration);
            }

            // 3. Audio Impact
            if (audioSystem != null)
            {
                audioSystem.PlayCrashSound(impactSpeed);
            }

            // 4. Engine Smoke Feedback
            if (engineSmokeParticle != null)
            {
                var emission = engineSmokeParticle.emission;
                emission.enabled = currentHealth <= smokeHealthThreshold;
            }
        }

        public void RepairVehicle()
        {
            currentHealth = maxHealth;
            if (engineSmokeParticle != null)
            {
                var emission = engineSmokeParticle.emission;
                emission.enabled = false;
            }
            OnHealthChanged?.Invoke(HealthPercent);
            Debug.Log("[VehicleDamageSystem] Vehicle fully repaired!");
        }
    }
}
