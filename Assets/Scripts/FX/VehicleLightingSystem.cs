using System.Collections;
using UnityEngine;
using CarGame.Vehicle;
using CarGame.InputSystem;
using CarGame.UI;

namespace CarGame.FX
{
    /// <summary>
    /// Complete vehicle lighting system:
    /// Headlights (low/high beam), Brake lights, Reverse lights, Turn signals, and Hazard lights.
    /// Controls both Unity Light components and emissive lens materials.
    /// </summary>
    public class VehicleLightingSystem : MonoBehaviour
    {
        [Header("Vehicle Reference")]
        [SerializeField] private VehicleController vehicle;

        [Header("Headlights (Front)")]
        [SerializeField] private Light[] headLights;
        [SerializeField] private Renderer[] headLightLenses;
        [SerializeField] private Material headLightOnMaterial;
        [SerializeField] private Material headLightOffMaterial;
        [SerializeField] private bool headlightsOn = true;

        [Header("Brake & Tail Lights (Rear)")]
        [SerializeField] private Light[] rearLights;
        [SerializeField] private Renderer[] rearLightLenses;
        [SerializeField] private Material tailLightMaterial;
        [SerializeField] private Material brakeLightMaterial; // Brighter red
        [SerializeField] private float tailLightIntensity = 0.8f;
        [SerializeField] private float brakeLightIntensity = 3.5f;

        [Header("Reverse Lights (Rear White)")]
        [SerializeField] private Light[] reverseLights;
        [SerializeField] private Renderer[] reverseLightLenses;

        [Header("Turn Signals & Hazards (Amber)")]
        [SerializeField] private Light[] leftTurnLights;
        [SerializeField] private Light[] rightTurnLights;
        [SerializeField] private Renderer[] leftTurnLenses;
        [SerializeField] private Renderer[] rightTurnLenses;
        [SerializeField] private float blinkInterval = 0.38f;

        // Internal State
        private bool isLeftSignalBlinking;
        private bool isRightSignalBlinking;
        private bool isHazardBlinking;
        private Coroutine blinkCoroutine;
        private bool blinkPhase;

        private void Start()
        {
            if (vehicle == null) vehicle = GetComponent<VehicleController>();
            SetHeadlights(headlightsOn);
            blinkCoroutine = StartCoroutine(SignalBlinkRoutine());
        }

        private void Update()
        {
            if (vehicle == null) return;

            // 1. Process Brake Lights
            bool isBraking = (VehicleInputManager.Instance != null && VehicleInputManager.Instance.Brake > 0.05f)
                             || vehicle.CurrentGearMode == GearMode.P;
            UpdateBrakeLights(isBraking);

            // 2. Process Reverse Lights
            bool isReversing = vehicle.CurrentGearMode == GearMode.R;
            UpdateReverseLights(isReversing);

            // 3. Sync Turn Signals / Hazards from HUD Controller if available
            if (VehicleHUDController.Instance != null)
            {
                SetHazardLights(VehicleHUDController.Instance.HazardLightsActive);
                SetLeftTurnSignal(VehicleHUDController.Instance.TurnLeftActive);
                SetRightTurnSignal(VehicleHUDController.Instance.TurnRightActive);
            }

            // Keyboard shortcut for lights in Editor
            if (UnityEngine.Input.GetKeyDown(KeyCode.L))
            {
                ToggleHeadlights();
            }
        }

        public void ToggleHeadlights()
        {
            SetHeadlights(!headlightsOn);
        }

        public void SetHeadlights(bool on)
        {
            headlightsOn = on;
            foreach (var lt in headLights)
            {
                if (lt != null) lt.enabled = on;
            }
            foreach (var r in headLightLenses)
            {
                if (r != null) r.sharedMaterial = on ? headLightOnMaterial : headLightOffMaterial;
            }
        }

        private void UpdateBrakeLights(bool isBraking)
        {
            float targetIntensity = isBraking ? brakeLightIntensity : (headlightsOn ? tailLightIntensity : 0f);
            bool enableRear = isBraking || headlightsOn;

            foreach (var lt in rearLights)
            {
                if (lt != null)
                {
                    lt.enabled = enableRear;
                    lt.intensity = targetIntensity;
                }
            }

            foreach (var r in rearLightLenses)
            {
                if (r != null)
                {
                    r.sharedMaterial = isBraking ? brakeLightMaterial : tailLightMaterial;
                }
            }
        }

        private void UpdateReverseLights(bool isReversing)
        {
            foreach (var lt in reverseLights)
            {
                if (lt != null) lt.enabled = isReversing;
            }
        }

        public void SetLeftTurnSignal(bool active)
        {
            isLeftSignalBlinking = active;
        }

        public void SetRightTurnSignal(bool active)
        {
            isRightSignalBlinking = active;
        }

        public void SetHazardLights(bool active)
        {
            isHazardBlinking = active;
        }

        private IEnumerator SignalBlinkRoutine()
        {
            WaitForSeconds wait = new WaitForSeconds(blinkInterval);
            while (true)
            {
                blinkPhase = !blinkPhase;

                // Left Signals
                bool leftOn = blinkPhase && (isLeftSignalBlinking || isHazardBlinking);
                foreach (var lt in leftTurnLights) if (lt != null) lt.enabled = leftOn;

                // Right Signals
                bool rightOn = blinkPhase && (isRightSignalBlinking || isHazardBlinking);
                foreach (var lt in rightTurnLights) if (lt != null) lt.enabled = rightOn;

                yield return wait;
            }
        }
    }
}
