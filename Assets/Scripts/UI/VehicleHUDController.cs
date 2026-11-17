using UnityEngine;
using UnityEngine.UI;
using CarGame.Vehicle;
using CarGame.InputSystem;
using CarGame.CameraSystem;

namespace CarGame.UI
{
    /// <summary>
    /// Master HUD Controller. Coordinates all on-screen touch driving widgets,
    /// speedometer telemetry, steering modes, and auxiliary controls.
    /// Multi-touch compatible.
    /// </summary>
    public class VehicleHUDController : MonoBehaviour
    {
        public static VehicleHUDController Instance { get; private set; }

        [Header("Vehicle & Input Connections")]
        [SerializeField] private VehicleController vehicle;
        [SerializeField] private VehicleInputManager inputManager;
        [SerializeField] private CameraController cameraController;

        [Header("Primary Controls")]
        [SerializeField] private TouchSteeringWheel steeringWheel;
        [SerializeField] private GameObject arrowButtonsRoot;
        [SerializeField] private TouchButtonControl btnLeftArrow;
        [SerializeField] private TouchButtonControl btnRightArrow;
        [SerializeField] private TouchPedal acceleratorPedal;
        [SerializeField] private TouchPedal brakePedal;
        [SerializeField] private VerticalGearSelectorUI gearSelector;

        [Header("Auxiliary Controls")]
        [SerializeField] private TouchButtonControl btnHandbrake;
        [SerializeField] private TouchButtonControl btnCameraSwitch;
        [SerializeField] private TouchButtonControl btnHazard;
        [SerializeField] private TouchButtonControl btnTurnLeft;
        [SerializeField] private TouchButtonControl btnTurnRight;
        [SerializeField] private TouchButtonControl btnRepairGarage;

        [Header("Speedometer & Gauges")]
        [SerializeField] private SpeedometerTachometerUI speedometerUI;

        [Header("Minimap & Navigation")]
        [SerializeField] private RectTransform minimapFrame;

        public bool HazardLightsActive { get; private set; }
        public bool TurnLeftActive { get; private set; }
        public bool TurnRightActive { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (vehicle == null) vehicle = FindObjectOfType<VehicleController>();
            if (inputManager == null) inputManager = VehicleInputManager.Instance;
            if (cameraController == null) cameraController = FindObjectOfType<CameraController>();

            SetupButtonListeners();
            ApplySteeringMode(inputManager != null ? inputManager.controlType : SteeringControlType.SteeringWheel);
        }

        private void SetupButtonListeners()
        {
            if (btnCameraSwitch != null)
            {
                btnCameraSwitch.onStateChanged.AddListener((active) =>
                {
                    if (active && cameraController != null)
                    {
                        cameraController.CycleCameraView();
                    }
                });
            }

            if (btnHandbrake != null)
            {
                btnHandbrake.onStateChanged.AddListener((pressed) =>
                {
                    if (inputManager != null) inputManager.SetVirtualHandbrake(pressed);
                });
            }

            if (btnHazard != null)
            {
                btnHazard.onStateChanged.AddListener((active) =>
                {
                    HazardLightsActive = active;
                    if (active)
                    {
                        TurnLeftActive = false;
                        TurnRightActive = false;
                    }
                });
            }

            if (btnTurnLeft != null)
            {
                btnTurnLeft.onStateChanged.AddListener((active) =>
                {
                    TurnLeftActive = active;
                    if (active)
                    {
                        TurnRightActive = false;
                        HazardLightsActive = false;
                    }
                });
            }

            if (btnTurnRight != null)
            {
                btnTurnRight.onStateChanged.AddListener((active) =>
                {
                    TurnRightActive = active;
                    if (active)
                    {
                        TurnLeftActive = false;
                        HazardLightsActive = false;
                    }
                });
            }

            // Arrow button steering fallback
            if (btnLeftArrow != null)
            {
                btnLeftArrow.onStateChanged.AddListener((pressed) =>
                {
                    if (inputManager != null && inputManager.controlType == SteeringControlType.TouchButtons)
                    {
                        inputManager.SetVirtualSteering(pressed ? -1f : (btnRightArrow != null && btnRightArrow.IsActive ? 1f : 0f));
                    }
                });
            }

            if (btnRightArrow != null)
            {
                btnRightArrow.onStateChanged.AddListener((pressed) =>
                {
                    if (inputManager != null && inputManager.controlType == SteeringControlType.TouchButtons)
                    {
                        inputManager.SetVirtualSteering(pressed ? 1f : (btnLeftArrow != null && btnLeftArrow.IsActive ? -1f : 0f));
                    }
                });
            }
        }

        public void ApplySteeringMode(SteeringControlType mode)
        {
            if (inputManager != null) inputManager.controlType = mode;

            bool isWheel = mode == SteeringControlType.SteeringWheel;
            bool isButtons = mode == SteeringControlType.TouchButtons;

            if (steeringWheel != null) steeringWheel.gameObject.SetActive(isWheel);
            if (arrowButtonsRoot != null) arrowButtonsRoot.SetActive(isButtons);
        }
    }
}
