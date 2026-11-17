using System;
using UnityEngine;
using CarGame.Vehicle;
using CarGame.InputSystem;

namespace CarGame.CameraSystem
{
    [Serializable]
    public class CameraPreset
    {
        public string name;
        public CameraViewMode mode;
        public Vector3 offset;
        public Vector3 lookAtOffset;
        public float positionSmoothSpeed = 12f;
        public float rotationSmoothSpeed = 10f;
    }

    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        [Header("Target Target / Car")]
        [SerializeField] private VehicleController targetVehicle;

        [Header("Active View Mode")]
        [SerializeField] private CameraViewMode currentMode = CameraViewMode.Chase;

        [Header("Presets Configuration")]
        [SerializeField]
        private CameraPreset chasePreset = new CameraPreset
        {
            name = "Chase View",
            mode = CameraViewMode.Chase,
            offset = new Vector3(0f, 1.85f, -4.85f),
            lookAtOffset = new Vector3(0f, 1.05f, 1.5f),
            positionSmoothSpeed = 14f,
            rotationSmoothSpeed = 12f
        };

        [SerializeField]
        private CameraPreset farChasePreset = new CameraPreset
        {
            name = "Far Chase View",
            mode = CameraViewMode.FarChase,
            offset = new Vector3(0f, 2.85f, -7.2f),
            lookAtOffset = new Vector3(0f, 1.2f, 2.0f),
            positionSmoothSpeed = 10f,
            rotationSmoothSpeed = 8f
        };

        [SerializeField]
        private CameraPreset cockpitPreset = new CameraPreset
        {
            name = "Cockpit View",
            mode = CameraViewMode.Cockpit,
            offset = new Vector3(-0.35f, 1.18f, 0.12f), // Left-hand drive hatchback cockpit
            lookAtOffset = new Vector3(-0.35f, 1.18f, 6.0f),
            positionSmoothSpeed = 28f,
            rotationSmoothSpeed = 28f
        };

        [SerializeField]
        private CameraPreset hoodPreset = new CameraPreset
        {
            name = "Hood View",
            mode = CameraViewMode.Hood,
            offset = new Vector3(0f, 0.95f, 1.45f), // Front bonnet/hood
            lookAtOffset = new Vector3(0f, 0.95f, 7.0f),
            positionSmoothSpeed = 28f,
            rotationSmoothSpeed = 28f
        };

        [Header("Dynamic FOV")]
        [SerializeField] private bool enableDynamicFOV = true;
        [SerializeField] private float baseFOV = 60f;
        [SerializeField] private float maxSpeedFOV = 74f;
        [SerializeField] private float fovDampingRate = 4.0f;

        [Header("Camera Shake (Stage 3 Ready)")]
        private float shakeIntensity;
        private float shakeTimeRemaining;

        // Internal References
        private Camera cam;
        private Vector3 currentVelocity;

        public CameraViewMode CurrentMode => currentMode;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        private void Start()
        {
            if (targetVehicle == null)
            {
                targetVehicle = FindObjectOfType<VehicleController>();
            }

            if (targetVehicle != null)
            {
                // Snap camera immediately to target initial position without lagging at origin
                SnapToTarget();
            }
        }

        public void SetTargetVehicle(VehicleController vehicle)
        {
            targetVehicle = vehicle;
            if (targetVehicle != null)
            {
                SnapToTarget();
            }
        }

        private void Update()
        {
            // Listen to camera switch input
            if (VehicleInputManager.Instance != null && VehicleInputManager.Instance.CameraCycleTriggered)
            {
                CycleCameraView();
            }

            // Update Dynamic FOV
            if (enableDynamicFOV && targetVehicle != null && cam != null)
            {
                float speedKmh = targetVehicle.CurrentSpeedKmh;
                float topSpeed = targetVehicle.Data != null ? targetVehicle.Data.topSpeedKmh : 200f;
                float speedRatio = Mathf.Clamp01(speedKmh / topSpeed);
                float targetFOV = Mathf.Lerp(baseFOV, maxSpeedFOV, speedRatio);
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovDampingRate);
            }

            // Camera Shake Decay
            if (shakeTimeRemaining > 0f)
            {
                shakeTimeRemaining -= Time.deltaTime;
                if (shakeTimeRemaining <= 0f) shakeIntensity = 0f;
            }
        }

        private void LateUpdate()
        {
            if (targetVehicle == null) return;

            CameraPreset activePreset = GetPresetForMode(currentMode);
            Transform targetTr = targetVehicle.transform;

            // 1. Calculate Target Position in World Space
            Vector3 worldTargetPos = targetTr.TransformPoint(activePreset.offset);

            // 2. Smooth Position
            transform.position = Vector3.Lerp(transform.position, worldTargetPos, Time.deltaTime * activePreset.positionSmoothSpeed);

            // 3. Calculate Target Look-At Point
            Vector3 worldLookAt = targetTr.TransformPoint(activePreset.lookAtOffset);
            Vector3 lookDirection = (worldLookAt - transform.position).normalized;

            if (lookDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDirection, targetTr.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * activePreset.rotationSmoothSpeed);
            }

            // 4. Apply Shake Offset if active
            if (shakeIntensity > 0f)
            {
                Vector3 shakeOffset = UnityEngine.Random.insideUnitSphere * shakeIntensity;
                transform.position += shakeOffset;
            }
        }

        /// <summary>
        /// Snaps camera immediately behind the vehicle without smoothing lag.
        /// </summary>
        public void SnapToTarget()
        {
            if (targetVehicle == null) return;
            CameraPreset activePreset = GetPresetForMode(currentMode);
            transform.position = targetVehicle.transform.TransformPoint(activePreset.offset);
            Vector3 worldLookAt = targetVehicle.transform.TransformPoint(activePreset.lookAtOffset);
            transform.rotation = Quaternion.LookRotation((worldLookAt - transform.position).normalized, targetVehicle.transform.up);
        }

        /// <summary>
        /// Cycles through camera views (Chase -> FarChase -> Cockpit -> Hood).
        /// </summary>
        public void CycleCameraView()
        {
            switch (currentMode)
            {
                case CameraViewMode.Chase:
                    currentMode = CameraViewMode.FarChase;
                    break;
                case CameraViewMode.FarChase:
                    currentMode = CameraViewMode.Cockpit;
                    break;
                case CameraViewMode.Cockpit:
                    currentMode = CameraViewMode.Hood;
                    break;
                case CameraViewMode.Hood:
                    currentMode = CameraViewMode.Chase;
                    break;
            }
        }

        public void SetCameraMode(CameraViewMode mode)
        {
            currentMode = mode;
        }

        public void TriggerShake(float intensity, float duration)
        {
            shakeIntensity = intensity;
            shakeTimeRemaining = duration;
        }

        private CameraPreset GetPresetForMode(CameraViewMode mode)
        {
            switch (mode)
            {
                case CameraViewMode.FarChase: return farChasePreset;
                case CameraViewMode.Cockpit:  return cockpitPreset;
                case CameraViewMode.Hood:     return hoodPreset;
                default:                      return chasePreset;
            }
        }
    }
}
