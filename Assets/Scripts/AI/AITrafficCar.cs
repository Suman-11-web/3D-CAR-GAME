using UnityEngine;

namespace CarGame.AI
{
    /// <summary>
    /// Autonomous traffic vehicle.
    /// Follows waypoint routes, uses multi-raycast sensors to detect
    /// obstacles (player, other traffic, pedestrians, red lights),
    /// and decelerates smoothly to maintain safe following distances.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class AITrafficCar : MonoBehaviour
    {
        [Header("Route Following")]
        [SerializeField] private TrafficWaypoint currentWaypoint;
        [SerializeField] private float waypointReachDistance = 4.0f;
        [SerializeField] private float maxSpeedKmh = 50f;
        [SerializeField] private float accelerationRate = 4.5f;
        [SerializeField] private float brakeRate = 8.0f;
        [SerializeField] private float steeringTurnSpeed = 3.5f;

        [Header("Obstacle Detection Sensors")]
        [SerializeField] private float sensorForwardDistance = 16f;
        [SerializeField] private float sensorSideOffset = 0.85f;
        [SerializeField] private float stopDistanceThreshold = 4.5f;
        [SerializeField] private LayerMask obstacleLayers;

        [Header("Brake Light Visuals")]
        [SerializeField] private Renderer[] brakeLightRenderers;
        [SerializeField] private Material brakeLightOnMat;
        [SerializeField] private Material brakeLightOffMat;

        // Internal State
        private Rigidbody rb;
        private float currentSpeed;
        private bool isBrakingForObstacle;

        public float CurrentSpeedKmh => currentSpeed * 3.6f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public void InitializeRoute(TrafficWaypoint startWaypoint, float initialSpeedKmh)
        {
            currentWaypoint = startWaypoint;
            currentSpeed = initialSpeedKmh / 3.6f;
            if (currentWaypoint != null)
            {
                transform.LookAt(new Vector3(currentWaypoint.transform.position.x, transform.position.y, currentWaypoint.transform.position.z));
            }
        }

        private void FixedUpdate()
        {
            if (currentWaypoint == null) return;

            // 1. Raycast Obstacle Detection
            bool obstacleAhead = EvaluateSensors(out float obstacleDistance);

            // 2. Compute Desired Speed
            float targetSpeedMs = (maxSpeedKmh / 3.6f);
            if (obstacleAhead)
            {
                if (obstacleDistance <= stopDistanceThreshold)
                {
                    targetSpeedMs = 0f;
                }
                else
                {
                    // Linear deceleration buffer
                    float distFactor = (obstacleDistance - stopDistanceThreshold) / (sensorForwardDistance - stopDistanceThreshold);
                    targetSpeedMs = Mathf.Lerp(0f, targetSpeedMs, distFactor);
                }
            }

            // 3. Accelerate or Decelerate
            if (currentSpeed < targetSpeedMs)
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeedMs, accelerationRate * Time.fixedDeltaTime);
                isBrakingForObstacle = false;
            }
            else
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeedMs, brakeRate * Time.fixedDeltaTime);
                isBrakingForObstacle = true;
            }

            // 4. Steer Towards Current Waypoint
            Vector3 targetPos = currentWaypoint.transform.position;
            targetPos.y = transform.position.y;
            Vector3 targetDir = (targetPos - transform.position).normalized;

            if (targetDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(targetDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * steeringTurnSpeed);
            }

            // 5. Apply Movement
            rb.velocity = transform.forward * currentSpeed;

            // 6. Check Waypoint Reached
            float distToWaypoint = Vector3.Distance(transform.position, targetPos);
            if (distToWaypoint <= waypointReachDistance)
            {
                AdvanceToNextWaypoint();
            }

            // 7. Update Brake Lights
            UpdateBrakeLights(isBrakingForObstacle);
        }

        private bool EvaluateSensors(out float closestHitDistance)
        {
            closestHitDistance = sensorForwardDistance;
            bool hitDetected = false;

            Vector3 originCenter = transform.position + (transform.forward * 1.5f) + (transform.up * 0.4f);
            Vector3 originLeft = originCenter - (transform.right * sensorSideOffset);
            Vector3 originRight = originCenter + (transform.right * sensorSideOffset);

            RaycastHit hit;
            // Center Ray
            if (Physics.Raycast(originCenter, transform.forward, out hit, sensorForwardDistance, obstacleLayers))
            {
                if (hit.collider.transform != transform)
                {
                    closestHitDistance = Mathf.Min(closestHitDistance, hit.distance);
                    hitDetected = true;
                }
            }

            // Left Ray
            if (Physics.Raycast(originLeft, transform.forward, out hit, sensorForwardDistance, obstacleLayers))
            {
                if (hit.collider.transform != transform)
                {
                    closestHitDistance = Mathf.Min(closestHitDistance, hit.distance);
                    hitDetected = true;
                }
            }

            // Right Ray
            if (Physics.Raycast(originRight, transform.forward, out hit, sensorForwardDistance, obstacleLayers))
            {
                if (hit.collider.transform != transform)
                {
                    closestHitDistance = Mathf.Min(closestHitDistance, hit.distance);
                    hitDetected = true;
                }
            }

            return hitDetected;
        }

        private void AdvanceToNextWaypoint()
        {
            if (currentWaypoint.nextWaypoints != null && currentWaypoint.nextWaypoints.Count > 0)
            {
                int nextIndex = Random.Range(0, currentWaypoint.nextWaypoints.Count);
                currentWaypoint = currentWaypoint.nextWaypoints[nextIndex];
                if (currentWaypoint != null && currentWaypoint.targetSpeedKmh > 0f)
                {
                    maxSpeedKmh = currentWaypoint.targetSpeedKmh;
                }
            }
        }

        private void UpdateBrakeLights(bool braking)
        {
            if (brakeLightRenderers == null) return;
            foreach (var r in brakeLightRenderers)
            {
                if (r != null)
                {
                    r.sharedMaterial = braking ? brakeLightOnMat : brakeLightOffMat;
                }
            }
        }
    }
}
