using System;
using UnityEngine;
using CarGame.Vehicle;
using CarGame.UI;

namespace CarGame.Gameplay
{
    /// <summary>
    /// Vehicle fuel consumption simulation and gas station refueling.
    /// Drains fuel based on RPM load and vehicle distance.
    /// </summary>
    public class FuelSystem : MonoBehaviour
    {
        [Header("Fuel Capacity")]
        [SerializeField] private float maxFuelLiters = 50f;
        [SerializeField] private float currentFuelLiters = 47.5f; // 95% starting fuel matching screenshot
        [SerializeField] private float idleBurnRatePerSecond = 0.0015f;
        [SerializeField] private float driveBurnFactor = 0.00008f;

        [Header("References")]
        [SerializeField] private VehicleController vehicle;

        public event Action<float> OnFuelLevelChanged;

        public float FuelRatio => Mathf.Clamp01(currentFuelLiters / maxFuelLiters);
        public bool IsEmpty => currentFuelLiters <= 0.01f;

        private void Start()
        {
            if (vehicle == null) vehicle = GetComponent<VehicleController>();
            UpdateHUD();
        }

        private void Update()
        {
            if (vehicle == null || IsEmpty) return;

            // Compute consumption
            float rpmFactor = (vehicle.CurrentRPM / vehicle.MaxRPM);
            float speedKmh = vehicle.CurrentSpeedKmh;

            float fuelBurn = (idleBurnRatePerSecond + (speedKmh * driveBurnFactor * rpmFactor)) * Time.deltaTime;
            currentFuelLiters = Mathf.Max(currentFuelLiters - fuelBurn, 0f);

            UpdateHUD();
            OnFuelLevelChanged?.Invoke(FuelRatio);
        }

        public void Refuel(float liters)
        {
            currentFuelLiters = Mathf.Clamp(currentFuelLiters + liters, 0f, maxFuelLiters);
            UpdateHUD();
            OnFuelLevelChanged?.Invoke(FuelRatio);
        }

        public void RefuelFull()
        {
            Refuel(maxFuelLiters);
        }

        private void UpdateHUD()
        {
            SpeedometerTachometerUI speedo = FindObjectOfType<SpeedometerTachometerUI>();
            if (speedo != null)
            {
                speedo.SetFuelLevel(FuelRatio);
            }
        }
    }
}
