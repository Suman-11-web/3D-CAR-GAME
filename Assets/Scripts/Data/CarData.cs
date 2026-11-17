using UnityEngine;
using CarGame.Vehicle;

namespace CarGame.Data
{
    [CreateAssetMenu(fileName = "Hatchback_CarData", menuName = "Car Game/Car Data", order = 1)]
    public class CarData : ScriptableObject
    {
        [Header("Vehicle Identity")]
        public string carName = "Urban Hatchback GT";
        public string carID = "hatchback_01";
        public int price = 15000;
        public bool isUnlockedByDefault = true;

        [Header("Physical Attributes")]
        [Tooltip("Vehicle mass in kilograms")]
        public float mass = 1380f;

        [Tooltip("Center of mass offset relative to transform origin. Lower Y prevents rolling over.")]
        public Vector3 centerOfMassOffset = new Vector3(0f, -0.35f, 0.15f);

        [Tooltip("Aerodynamic downforce factor applied at high speeds to keep vehicle planted.")]
        public float downforceFactor = 65f;

        [Header("Drivetrain Layout")]
        public DriveType driveType = DriveType.FrontWheelDrive;

        [Header("Engine & Power")]
        [Tooltip("Peak engine torque in Newton-meters (Nm)")]
        public float peakEngineTorque = 420f;

        [Tooltip("RPM at idle state")]
        public float idleRPM = 850f;

        [Tooltip("RPM shift-up threshold in automatic mode")]
        public float shiftUpRPM = 5400f;

        [Tooltip("RPM shift-down threshold in automatic mode")]
        public float shiftDownRPM = 2200f;

        [Tooltip("Maximum allowed engine RPM")]
        public float maxRPM = 6800f;

        [Tooltip("Top speed limiter in km/h")]
        public float topSpeedKmh = 210f;

        [Header("Gearbox & Transmission")]
        [Tooltip("Gear ratios for forward gears 1 through 6")]
        public float[] gearRatios = new float[] { 3.82f, 2.20f, 1.52f, 1.12f, 0.88f, 0.72f };

        [Tooltip("Reverse gear ratio")]
        public float reverseGearRatio = 3.55f;

        [Tooltip("Final drive differential ratio")]
        public float finalDriveRatio = 3.65f;

        [Tooltip("Duration in seconds of gear shift interrupt")]
        public float shiftDelay = 0.22f;

        [Header("Steering Dynamics")]
        [Tooltip("Maximum steering angle at near-zero speed (degrees)")]
        public float maxSteerAngleLowSpeed = 35f;

        [Tooltip("Maximum steering angle at high speed (degrees) to prevent rollovers")]
        public float maxSteerAngleHighSpeed = 12f;

        [Tooltip("Speed in km/h at which high-speed steering lock applies")]
        public float highSpeedThresholdKmh = 120f;

        [Tooltip("Steering responsiveness smoothing rate")]
        public float steerSmoothingRate = 9f;

        [Header("Brakes & Handbrake")]
        [Tooltip("Maximum service braking torque on front/rear axles")]
        public float maxBrakeTorque = 3600f;

        [Tooltip("Handbrake torque applied to rear wheels")]
        public float handbrakeTorque = 4800f;

        [Tooltip("Engine deceleration resistance when releasing throttle")]
        public float engineBrakingTorque = 180f;

        [Header("Suspension & Anti-Roll Bars")]
        [Tooltip("Front anti-roll bar stiffness. Counters lateral body roll.")]
        public float frontAntiRollStiffness = 6500f;

        [Tooltip("Rear anti-roll bar stiffness. Balances oversteer/understeer.")]
        public float rearAntiRollStiffness = 5800f;

        [Header("Driver Assists")]
        [Tooltip("Traction Control System (TCS) prevents drive wheel slip")]
        public bool enableTractionControl = true;

        [Tooltip("Maximum allowed forward wheel slip ratio before TCS intervenes (0.0 to 1.0)")]
        [Range(0.1f, 0.8f)]
        public float tcsSlipThreshold = 0.35f;

        [Tooltip("Anti-lock Braking System (ABS) prevents wheel lockup during braking")]
        public bool enableABS = true;

        [Tooltip("Maximum allowed slip ratio before ABS releases brake pressure")]
        [Range(0.1f, 0.8f)]
        public float absSlipThreshold = 0.40f;

        [Header("WheelCollider Defaults")]
        public float wheelRadius = 0.34f;
        public float wheelSuspensionDistance = 0.22f;
        public float suspensionSpring = 38000f;
        public float suspensionDamper = 4500f;
        public float suspensionTargetPosition = 0.5f;

        [Header("Friction Curve Presets")]
        public float forwardFrictionStiffness = 1.0f;
        public float sidewaysFrictionStiffness = 1.05f;

        /// <summary>
        /// Computes simulated engine torque given current RPM.
        /// Uses a tuned torque curve typical of a modern turbocharged 4-cylinder engine.
        /// </summary>
        public float EvaluateEngineTorqueCurve(float currentRPM)
        {
            float normRPM = Mathf.Clamp01((currentRPM - idleRPM) / (maxRPM - idleRPM));
            
            // Torque curve: builds up quickly from idle, reaches plateau between 2000-5000 RPM, then tapers slightly towards redline
            float torqueFactor;
            if (normRPM < 0.25f)
            {
                // Low RPM ramp-up
                torqueFactor = Mathf.Lerp(0.70f, 1.0f, normRPM / 0.25f);
            }
            else if (normRPM <= 0.75f)
            {
                // Plateau (peak torque band)
                torqueFactor = 1.0f;
            }
            else
            {
                // Dropoff approaching redline
                torqueFactor = Mathf.Lerp(1.0f, 0.82f, (normRPM - 0.75f) / 0.25f);
            }

            return peakEngineTorque * torqueFactor;
        }
    }
}
