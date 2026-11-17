using System;
using UnityEngine;
using CarGame.Vehicle;
using CarGame.Economy;
using CarGame.AI;

namespace CarGame.Gameplay
{
    /// <summary>
    /// Enforces traffic regulations:
    /// Fines player for red-light jumping, speeding violations, and collisions.
    /// </summary>
    public class TrafficRuleScoringSystem : MonoBehaviour
    {
        public static TrafficRuleScoringSystem Instance { get; private set; }

        [Header("Speed Limits")]
        [SerializeField] private float citySpeedLimitKmh = 60f;
        [SerializeField] private float speedCheckTolerance = 8f;
        [SerializeField] private int speedingFine = 30;

        [Header("Red Light Infractions")]
        [SerializeField] private int redLightFine = 75;

        [Header("Collision Penalties")]
        [SerializeField] private int collisionFine = 40;

        [Header("References")]
        [SerializeField] private VehicleController playerVehicle;

        private float speedViolationTimer;
        private float violationCooldownTimer;

        public event Action<string, int> OnTrafficViolation;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (playerVehicle == null) playerVehicle = FindObjectOfType<VehicleController>();
        }

        private void Update()
        {
            if (playerVehicle == null) return;

            if (violationCooldownTimer > 0f)
            {
                violationCooldownTimer -= Time.deltaTime;
            }

            // Check Speeding
            if (playerVehicle.CurrentSpeedKmh > (citySpeedLimitKmh + speedCheckTolerance))
            {
                speedViolationTimer += Time.deltaTime;
                if (speedViolationTimer >= 3.0f && violationCooldownTimer <= 0f)
                {
                    TriggerViolation("Speeding Violation!", speedingFine);
                    speedViolationTimer = 0f;
                }
            }
            else
            {
                speedViolationTimer = Mathf.Max(speedViolationTimer - Time.deltaTime, 0f);
            }
        }

        public void ReportRedLightJumping()
        {
            if (violationCooldownTimer <= 0f)
            {
                TriggerViolation("Red Light Violation!", redLightFine);
            }
        }

        public void ReportCollision()
        {
            if (violationCooldownTimer <= 0f)
            {
                TriggerViolation("Traffic Collision!", collisionFine);
            }
        }

        private void TriggerViolation(string description, int fine)
        {
            violationCooldownTimer = 4.0f; // Prevent spamming fines
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.DeductFine(fine);
            }

            OnTrafficViolation?.Invoke(description, fine);
            Debug.LogWarning($"[TrafficScoring] {description} -${fine}");
        }
    }
}
