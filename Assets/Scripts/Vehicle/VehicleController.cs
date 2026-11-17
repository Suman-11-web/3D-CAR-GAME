using System.Collections;
using UnityEngine;
using CarGame.Data;
using CarGame.InputSystem;

namespace CarGame.Vehicle
{
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleController : MonoBehaviour
    {
        [Header("Vehicle Configuration Asset")]
        [SerializeField] private CarData carData;

        [Header("Axle Setup")]
        [SerializeField] private WheelUnit frontLeftWheel = new WheelUnit { wheelName = "FrontLeft", isSteerable = true, isMotor = true, hasBrakes = true, hasHandbrake = false };
        [SerializeField] private WheelUnit frontRightWheel = new WheelUnit { wheelName = "FrontRight", isSteerable = true, isMotor = true, hasBrakes = true, hasHandbrake = false };
        [SerializeField] private WheelUnit rearLeftWheel = new WheelUnit { wheelName = "RearLeft", isSteerable = false, isMotor = false, hasBrakes = true, hasHandbrake = true };
        [SerializeField] private WheelUnit rearRightWheel = new WheelUnit { wheelName = "RearRight", isSteerable = false, isMotor = false, hasBrakes = true, hasHandbrake = true };

        [Header("Surface Grip Modifier (Stage 3 Ready)")]
        [SerializeField] private float globalSurfaceFrictionMultiplier = 1.0f;

        // Components & State
        private Rigidbody rb;
        private VehicleInputManager inputManager;

        // Transmission State
        private GearMode currentGearMode = GearMode.D;
        private int currentGear = 1;
        private float currentRPM;
        private bool isShifting;
        private float shiftCooldownTimer;

        // Steering State
        private float smoothedSteerAngle;

        // Assist States
        public bool TractionControlActive { get; private set; }
        public bool AbsActive { get; private set; }

        // Telemetry Properties for HUD & Audio
        public float CurrentSpeedKmh => rb != null ? rb.velocity.magnitude * 3.6f : 0f;
        public float CurrentSpeedMph => CurrentSpeedKmh * 0.621371f;
        public float CurrentRPM => currentRPM;
        public float MaxRPM => carData != null ? carData.maxRPM : 7000f;
        public float IdleRPM => carData != null ? carData.idleRPM : 850f;
        public int CurrentGear => currentGear;
        public GearMode CurrentGearMode => currentGearMode;
        public bool IsShifting => isShifting;
        public Rigidbody RigidbodyComponent => rb;
        public CarData Data => carData;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            InitializeVehicle();
        }

        private void Start()
        {
            inputManager = VehicleInputManager.Instance;
            if (inputManager == null)
            {
                inputManager = FindObjectOfType<VehicleInputManager>();
            }

            ConfigureWheelColliders();
        }

        private void InitializeVehicle()
        {
            if (carData == null)
            {
                Debug.LogWarning("[VehicleController] No CarData assigned. Loading or generating default settings.");
                return;
            }

            rb.mass = carData.mass;
            rb.centerOfMass = carData.centerOfMassOffset;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            currentRPM = carData.idleRPM;
            currentGear = 1;
            currentGearMode = GearMode.D;

            // Configure drive axle based on CarData layout
            bool isAWD = carData.driveType == DriveType.AllWheelDrive;
            bool isFWD = carData.driveType == DriveType.FrontWheelDrive;
            bool isRWD = carData.driveType == DriveType.RearWheelDrive;

            frontLeftWheel.isMotor = isFWD || isAWD;
            frontRightWheel.isMotor = isFWD || isAWD;
            rearLeftWheel.isMotor = isRWD || isAWD;
            rearRightWheel.isMotor = isRWD || isAWD;
        }

        /// <summary>
        /// Tunes WheelCollider suspension and friction curves based on CarData.
        /// </summary>
        public void ConfigureWheelColliders()
        {
            if (carData == null) return;

            WheelUnit[] wheels = { frontLeftWheel, frontRightWheel, rearLeftWheel, rearRightWheel };
            foreach (var w in wheels)
            {
                if (w.wheelCollider == null) continue;

                w.wheelCollider.radius = carData.wheelRadius;
                w.wheelCollider.suspensionDistance = carData.wheelSuspensionDistance;

                JointSpring spring = w.wheelCollider.suspensionSpring;
                spring.spring = carData.suspensionSpring;
                spring.damper = carData.suspensionDamper;
                spring.targetPosition = carData.suspensionTargetPosition;
                w.wheelCollider.suspensionSpring = spring;

                WheelFrictionCurve forwardFriction = w.wheelCollider.forwardFriction;
                forwardFriction.extremumSlip = 0.2f;
                forwardFriction.extremumValue = 1.0f;
                forwardFriction.asymptoteSlip = 0.5f;
                forwardFriction.asymptoteValue = 0.75f;
                forwardFriction.stiffness = carData.forwardFrictionStiffness * globalSurfaceFrictionMultiplier;
                w.wheelCollider.forwardFriction = forwardFriction;

                WheelFrictionCurve sidewaysFriction = w.wheelCollider.sidewaysFriction;
                sidewaysFriction.extremumSlip = 0.2f;
                sidewaysFriction.extremumValue = 1.0f;
                sidewaysFriction.asymptoteSlip = 0.5f;
                sidewaysFriction.asymptoteValue = 0.75f;
                sidewaysFriction.stiffness = carData.sidewaysFrictionStiffness * globalSurfaceFrictionMultiplier;
                w.wheelCollider.sidewaysFriction = sidewaysFriction;
            }
        }

        private void Update()
        {
            // Update visual wheel rotation and position each frame
            frontLeftWheel.UpdateVisualPose();
            frontRightWheel.UpdateVisualPose();
            rearLeftWheel.UpdateVisualPose();
            rearRightWheel.UpdateVisualPose();

            // Check gear mode requests from input
            if (inputManager != null && inputManager.RequestedGearMode != currentGearMode)
            {
                SetGearMode(inputManager.RequestedGearMode);
            }
        }

        private void FixedUpdate()
        {
            if (carData == null) return;

            // 1. Gather Telemetry
            frontLeftWheel.UpdateTelemetry();
            frontRightWheel.UpdateTelemetry();
            rearLeftWheel.UpdateTelemetry();
            rearRightWheel.UpdateTelemetry();

            // 2. Fetch Inputs
            float steerInput = inputManager != null ? inputManager.Steering : 0f;
            float throttleInput = inputManager != null ? inputManager.Throttle : 0f;
            float brakeInput = inputManager != null ? inputManager.Brake : 0f;
            bool handbrakeInput = inputManager != null && inputManager.Handbrake;

            // 3. Process Dynamics
            ApplySteering(steerInput);
            UpdateEngineAndTransmission(throttleInput);
            ApplyDrivetrainTorque(throttleInput);
            ApplyBraking(brakeInput, handbrakeInput);
            ApplyAntiRollBars();
            ApplyAerodynamicDownforce();
        }

        #region Steering

        private void ApplySteering(float steerInput)
        {
            float speedKmh = CurrentSpeedKmh;
            float speedFactor = Mathf.Clamp01(speedKmh / carData.highSpeedThresholdKmh);
            float targetMaxAngle = Mathf.Lerp(carData.maxSteerAngleLowSpeed, carData.maxSteerAngleHighSpeed, speedFactor);
            float targetAngle = steerInput * targetMaxAngle;

            smoothedSteerAngle = Mathf.Lerp(smoothedSteerAngle, targetAngle, Time.fixedDeltaTime * carData.steerSmoothingRate);

            frontLeftWheel.ApplySteerAngle(smoothedSteerAngle);
            frontRightWheel.ApplySteerAngle(smoothedSteerAngle);
        }

        #endregion

        #region Engine & Transmission Simulation

        private void UpdateEngineAndTransmission(float throttleInput)
        {
            if (shiftCooldownTimer > 0f)
            {
                shiftCooldownTimer -= Time.fixedDeltaTime;
                if (shiftCooldownTimer <= 0f) isShifting = false;
            }

            // Average wheel RPM of driving wheels
            float avgWheelRpm = GetAverageDrivenWheelRPM();

            switch (currentGearMode)
            {
                case GearMode.P:
                case GearMode.N:
                    // Engine revs freely in Park or Neutral
                    float targetFreeRPM = Mathf.Lerp(carData.idleRPM, carData.maxRPM, throttleInput);
                    currentRPM = Mathf.MoveTowards(currentRPM, targetFreeRPM, Time.fixedDeltaTime * 4500f);
                    break;

                case GearMode.D:
                    float currentGearRatio = carData.gearRatios[Mathf.Clamp(currentGear - 1, 0, carData.gearRatios.Length - 1)];
                    float calculatedRPM = Mathf.Abs(avgWheelRpm) * currentGearRatio * carData.finalDriveRatio;
                    float baseRPM = Mathf.Max(calculatedRPM, carData.idleRPM);

                    // Add slight throttle rev feel
                    float throttleBoost = throttleInput * 350f;
                    currentRPM = Mathf.Clamp(baseRPM + throttleBoost, carData.idleRPM, carData.maxRPM);

                    // Automatic Shift Up / Down
                    if (!isShifting && shiftCooldownTimer <= 0f)
                    {
                        if (currentRPM >= carData.shiftUpRPM && currentGear < carData.gearRatios.Length)
                        {
                            StartCoroutine(ShiftGearRoutine(currentGear + 1));
                        }
                        else if (currentRPM <= carData.shiftDownRPM && currentGear > 1)
                        {
                            StartCoroutine(ShiftGearRoutine(currentGear - 1));
                        }
                    }
                    break;

                case GearMode.R:
                    float revRPM = Mathf.Abs(avgWheelRpm) * carData.reverseGearRatio * carData.finalDriveRatio;
                    currentRPM = Mathf.Clamp(Mathf.Max(revRPM, carData.idleRPM) + (throttleInput * 300f), carData.idleRPM, carData.maxRPM);
                    break;
            }
        }

        private IEnumerator ShiftGearRoutine(int targetGear)
        {
            isShifting = true;
            shiftCooldownTimer = carData.shiftDelay + 0.35f;
            yield return new WaitForSeconds(carData.shiftDelay);
            currentGear = targetGear;
            isShifting = false;
        }

        private float GetAverageDrivenWheelRPM()
        {
            int count = 0;
            float sum = 0f;

            WheelUnit[] wheels = { frontLeftWheel, frontRightWheel, rearLeftWheel, rearRightWheel };
            foreach (var w in wheels)
            {
                if (w.isMotor && w.wheelCollider != null)
                {
                    sum += w.wheelCollider.rpm;
                    count++;
                }
            }

            return count > 0 ? (sum / count) : 0f;
        }

        #endregion

        #region Drivetrain Torque & Traction Control (TCS)

        private void ApplyDrivetrainTorque(float throttleInput)
        {
            if (currentGearMode == GearMode.P || currentGearMode == GearMode.N || isShifting)
            {
                DistributeMotorTorque(0f);
                return;
            }

            // Top speed limiter
            if (CurrentSpeedKmh >= carData.topSpeedKmh && currentGearMode == GearMode.D)
            {
                DistributeMotorTorque(0f);
                return;
            }

            // Reverse speed limiter (e.g., max 45 km/h)
            if (currentGearMode == GearMode.R && CurrentSpeedKmh >= 45f)
            {
                DistributeMotorTorque(0f);
                return;
            }

            float engineTorque = carData.EvaluateEngineTorqueCurve(currentRPM);
            float activeRatio = (currentGearMode == GearMode.R)
                ? -carData.reverseGearRatio
                : carData.gearRatios[Mathf.Clamp(currentGear - 1, 0, carData.gearRatios.Length - 1)];

            float driveTorque = engineTorque * activeRatio * carData.finalDriveRatio * throttleInput;

            // Traction Control System (TCS)
            TractionControlActive = false;
            if (carData.enableTractionControl && throttleInput > 0.05f)
            {
                float maxSlip = Mathf.Max(frontLeftWheel.forwardSlip, frontRightWheel.forwardSlip, rearLeftWheel.forwardSlip, rearRightWheel.forwardSlip);
                if (maxSlip > carData.tcsSlipThreshold)
                {
                    TractionControlActive = true;
                    // Cut torque proportional to excessive slip
                    float cutFactor = Mathf.Clamp01(1.0f - (maxSlip - carData.tcsSlipThreshold) * 2.0f);
                    driveTorque *= cutFactor;
                }
            }

            DistributeMotorTorque(driveTorque);
        }

        private void DistributeMotorTorque(float totalTorque)
        {
            int motorWheelCount = 0;
            if (frontLeftWheel.isMotor) motorWheelCount++;
            if (frontRightWheel.isMotor) motorWheelCount++;
            if (rearLeftWheel.isMotor) motorWheelCount++;
            if (rearRightWheel.isMotor) motorWheelCount++;

            if (motorWheelCount == 0) return;
            float torquePerWheel = totalTorque / motorWheelCount;

            frontLeftWheel.ApplyMotorTorque(torquePerWheel);
            frontRightWheel.ApplyMotorTorque(torquePerWheel);
            rearLeftWheel.ApplyMotorTorque(torquePerWheel);
            rearRightWheel.ApplyMotorTorque(torquePerWheel);
        }

        #endregion

        #region Brakes, Handbrake, ABS & Engine Braking

        private void ApplyBraking(float brakeInput, bool handbrakePressed)
        {
            AbsActive = false;

            // 1. Park Mode: Absolute lock
            if (currentGearMode == GearMode.P)
            {
                ApplyAllBrakes(carData.maxBrakeTorque * 1.5f);
                return;
            }

            // 2. Service Brakes (Pedal)
            float targetBrakeTorque = brakeInput * carData.maxBrakeTorque;

            // ABS Logic
            if (carData.enableABS && brakeInput > 0.15f)
            {
                float maxSlip = Mathf.Max(frontLeftWheel.forwardSlip, frontRightWheel.forwardSlip, rearLeftWheel.forwardSlip, rearRightWheel.forwardSlip);
                if (maxSlip > carData.absSlipThreshold)
                {
                    AbsActive = true;
                    // Pulse/relieve brake pressure to recover grip
                    targetBrakeTorque *= 0.45f;
                }
            }

            // Engine Braking: When off-throttle and moving forward
            if (brakeInput <= 0.01f && currentGearMode == GearMode.D && CurrentSpeedKmh > 5f)
            {
                targetBrakeTorque = carData.engineBrakingTorque;
            }

            frontLeftWheel.ApplyBrakeTorque(targetBrakeTorque);
            frontRightWheel.ApplyBrakeTorque(targetBrakeTorque);

            // Handbrake overrides rear axle
            float rearBrake = handbrakePressed ? carData.handbrakeTorque : targetBrakeTorque;
            rearLeftWheel.ApplyBrakeTorque(rearBrake);
            rearRightWheel.ApplyBrakeTorque(rearBrake);
        }

        private void ApplyAllBrakes(float torque)
        {
            frontLeftWheel.ApplyBrakeTorque(torque);
            frontRightWheel.ApplyBrakeTorque(torque);
            rearLeftWheel.ApplyBrakeTorque(torque);
            rearRightWheel.ApplyBrakeTorque(torque);
        }

        #endregion

        #region Anti-Roll Bars & Downforce

        private void ApplyAntiRollBars()
        {
            ApplyAntiRollPair(frontLeftWheel, frontRightWheel, carData.frontAntiRollStiffness);
            ApplyAntiRollPair(rearLeftWheel, rearRightWheel, carData.rearAntiRollStiffness);
        }

        private void ApplyAntiRollPair(WheelUnit wheelL, WheelUnit wheelR, float stiffness)
        {
            if (wheelL.wheelCollider == null || wheelR.wheelCollider == null) return;

            float travelL = 1.0f;
            float travelR = 1.0f;

            if (wheelL.isGrounded)
            {
                travelL = (-wheelL.wheelCollider.transform.InverseTransformPoint(wheelL.lastHit.point).y - wheelL.wheelCollider.radius)
                          / wheelL.wheelCollider.suspensionDistance;
            }

            if (wheelR.isGrounded)
            {
                travelR = (-wheelR.wheelCollider.transform.InverseTransformPoint(wheelR.lastHit.point).y - wheelR.wheelCollider.radius)
                          / wheelR.wheelCollider.suspensionDistance;
            }

            float antiRollForce = (travelL - travelR) * stiffness;

            if (wheelL.isGrounded)
            {
                rb.AddForceAtPosition(wheelL.wheelCollider.transform.up * -antiRollForce, wheelL.wheelCollider.transform.position);
            }

            if (wheelR.isGrounded)
            {
                rb.AddForceAtPosition(wheelR.wheelCollider.transform.up * antiRollForce, wheelR.wheelCollider.transform.position);
            }
        }

        private void ApplyAerodynamicDownforce()
        {
            float speedMs = rb.velocity.magnitude;
            // Downforce is proportional to speed squared
            float downforce = carData.downforceFactor * speedMs * speedMs;
            rb.AddForce(-transform.up * downforce);
        }

        #endregion

        #region Public Controls & External API

        public void SetGearMode(GearMode mode)
        {
            // Disallow shifting into Reverse if traveling forward at noticeable speed
            if (mode == GearMode.R && CurrentSpeedKmh > 10f && Vector3.Dot(rb.velocity, transform.forward) > 0.5f)
            {
                Debug.LogWarning("[VehicleController] Cannot engage Reverse while moving forward at speed.");
                return;
            }

            currentGearMode = mode;
            if (mode == GearMode.D) currentGear = 1;
        }

        public void ToggleTractionControl()
        {
            if (carData != null) carData.enableTractionControl = !carData.enableTractionControl;
        }

        public void ToggleABS()
        {
            if (carData != null) carData.enableABS = !carData.enableABS;
        }

        public void SetSurfaceFrictionMultiplier(float multiplier)
        {
            globalSurfaceFrictionMultiplier = Mathf.Clamp(multiplier, 0.2f, 1.5f);
            ConfigureWheelColliders();
        }

        #endregion
    }
}
