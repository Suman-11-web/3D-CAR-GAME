using UnityEngine;
using CarGame.Vehicle;

namespace CarGame.InputSystem
{
    public enum SteeringControlType
    {
        SteeringWheel, // On-screen rotatable steering wheel
        TouchButtons,  // Left/Right arrow touch buttons
        TiltGyro       // Mobile device gyroscope/accelerometer tilt
    }

    /// <summary>
    /// Central input manager for the vehicle.
    /// Supports desktop keyboard/gamepad for development/testing,
    /// and provides API for mobile touch HUD controls.
    /// </summary>
    public class VehicleInputManager : MonoBehaviour
    {
        public static VehicleInputManager Instance { get; private set; }

        [Header("Control Settings")]
        public SteeringControlType controlType = SteeringControlType.SteeringWheel;
        [Range(0.2f, 2.0f)] public float tiltSensitivity = 1.0f;
        [Range(0.2f, 2.0f)] public float touchSensitivity = 1.0f;

        [Header("Virtual / Touch Inputs (Written by Stage 2 Touch HUD)")]
        [SerializeField] private float virtualSteer;
        [SerializeField] private float virtualThrottle;
        [SerializeField] private float virtualBrake;
        [SerializeField] private bool virtualHandbrake;
        [SerializeField] private bool isVirtualInputActive;

        // Effective Output Values
        public float Steering { get; private set; }
        public float Throttle { get; private set; }
        public float Brake { get; private set; }
        public bool Handbrake { get; private set; }
        public GearMode RequestedGearMode { get; private set; } = GearMode.D;
        public bool CameraCycleTriggered { get; private set; }
        public bool HornActive { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            CameraCycleTriggered = false;

            // 1. Process Virtual HUD Inputs if present, else fallback to Keyboard / Gamepad
            if (isVirtualInputActive)
            {
                Steering = Mathf.Clamp(virtualSteer, -1f, 1f);
                Throttle = Mathf.Clamp01(virtualThrottle);
                Brake = Mathf.Clamp01(virtualBrake);
                Handbrake = virtualHandbrake;
            }
            else
            {
                ProcessKeyboardInput();
            }

            // Gyroscope tilt support if selected
            if (controlType == SteeringControlType.TiltGyro && SystemInfo.supportsAccelerometer)
            {
                float tilt = UnityEngine.Input.acceleration.x * tiltSensitivity * 2.0f;
                Steering = Mathf.Clamp(tilt, -1f, 1f);
            }
        }

        private void ProcessKeyboardInput()
        {
            // Steering: A/D or Left/Right
            float rawSteer = UnityEngine.Input.GetAxis("Horizontal");
            Steering = Mathf.Clamp(rawSteer * touchSensitivity, -1f, 1f);

            // Throttle & Brake: W/S or Up/Down
            float rawVertical = UnityEngine.Input.GetAxis("Vertical");
            if (rawVertical > 0f)
            {
                Throttle = rawVertical;
                Brake = 0f;
            }
            else if (rawVertical < 0f)
            {
                Throttle = 0f;
                Brake = Mathf.Abs(rawVertical);
            }
            else
            {
                Throttle = 0f;
                Brake = 0f;
            }

            // Handbrake: Spacebar
            Handbrake = UnityEngine.Input.GetKey(KeyCode.Space);

            // Quick Gear Switch keys for testing in Editor:
            if (UnityEngine.Input.GetKeyDown(KeyCode.P)) RequestedGearMode = GearMode.P;
            if (UnityEngine.Input.GetKeyDown(KeyCode.R)) RequestedGearMode = GearMode.R;
            if (UnityEngine.Input.GetKeyDown(KeyCode.N)) RequestedGearMode = GearMode.N;
            if (UnityEngine.Input.GetKeyDown(KeyCode.D)) RequestedGearMode = GearMode.D;

            // Camera toggle: C or V
            if (UnityEngine.Input.GetKeyDown(KeyCode.C) || UnityEngine.Input.GetKeyDown(KeyCode.V))
            {
                CameraCycleTriggered = true;
            }

            // Horn: H
            HornActive = UnityEngine.Input.GetKey(KeyCode.H);
        }

        #region Public API for Touch HUD (Stage 2)

        public void SetVirtualSteering(float steerValue)
        {
            virtualSteer = steerValue;
            isVirtualInputActive = true;
        }

        public void SetVirtualThrottle(float throttleValue)
        {
            virtualThrottle = throttleValue;
            isVirtualInputActive = true;
        }

        public void SetVirtualBrake(float brakeValue)
        {
            virtualBrake = brakeValue;
            isVirtualInputActive = true;
        }

        public void SetVirtualHandbrake(bool pressed)
        {
            virtualHandbrake = pressed;
            isVirtualInputActive = true;
        }

        public void SetGearMode(GearMode mode)
        {
            RequestedGearMode = mode;
        }

        public void RequestCameraCycle()
        {
            CameraCycleTriggered = true;
        }

        public void SetHorn(bool active)
        {
            HornActive = active;
        }

        public void ReleaseVirtualInputs()
        {
            virtualSteer = 0f;
            virtualThrottle = 0f;
            virtualBrake = 0f;
            virtualHandbrake = false;
            isVirtualInputActive = false;
        }

        #endregion
    }
}
