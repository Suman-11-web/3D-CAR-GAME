using UnityEngine;
using CarGame.Vehicle;

namespace CarGame.FX
{
    /// <summary>
    /// Generates skid marks and emits wet-road tire spray / smoke particles
    /// when tires exceed lateral or longitudinal slip thresholds.
    /// </summary>
    public class SkidmarkAndSpraySystem : MonoBehaviour
    {
        [Header("Vehicle Reference")]
        [SerializeField] private VehicleController vehicle;

        [Header("Slip Thresholds")]
        [SerializeField] private float slipThreshold = 0.32f;

        [Header("Wheel Trails / Skidmarks")]
        [SerializeField] private TrailRenderer[] wheelSkidTrails;

        [Header("Particle Effects")]
        [SerializeField] private ParticleSystem[] wheelSprayParticles;
        [SerializeField] private bool isWetRoad = true;

        private void Start()
        {
            if (vehicle == null) vehicle = GetComponent<VehicleController>();

            // Setup skid trails
            foreach (var trail in wheelSkidTrails)
            {
                if (trail != null)
                {
                    trail.emitting = false;
                }
            }
        }

        private void FixedUpdate()
        {
            if (vehicle == null) return;

            // Check if vehicle is experiencing high slip or hard braking/handbraking
            bool shouldSkid = vehicle.TractionControlActive || vehicle.AbsActive;

            // In Stage 1 & 2, VehicleController handles slip telemetry
            for (int i = 0; i < wheelSkidTrails.Length; i++)
            {
                if (wheelSkidTrails[i] != null)
                {
                    wheelSkidTrails[i].emitting = shouldSkid;
                }
            }

            // Particle Spray
            for (int i = 0; i < wheelSprayParticles.Length; i++)
            {
                if (wheelSprayParticles[i] != null)
                {
                    var emission = wheelSprayParticles[i].emission;
                    emission.enabled = shouldSkid && vehicle.CurrentSpeedKmh > 5f;
                }
            }
        }

        public void SetWetRoadMode(bool wet)
        {
            isWetRoad = wet;
        }
    }
}
