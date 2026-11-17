using UnityEngine;

namespace CarGame.AI
{
    /// <summary>
    /// Pedestrian crossing agent.
    /// Walks back and forth across zebra crossings with obstacle awareness.
    /// </summary>
    public class PedestrianAgent : MonoBehaviour
    {
        [Header("Waypoints")]
        [SerializeField] private Transform pointA;
        [SerializeField] private Transform pointB;
        [SerializeField] private float walkSpeed = 1.35f;
        [SerializeField] private float arrivalThreshold = 0.5f;

        [Header("Safety Sensor")]
        [SerializeField] private float stopDistanceForCars = 3.0f;
        [SerializeField] private LayerMask carLayer;

        private Transform targetPoint;
        private bool isWaiting;

        private void Start()
        {
            targetPoint = pointB;
        }

        private void Update()
        {
            if (targetPoint == null || isWaiting) return;

            // 1. Check for incoming cars
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, stopDistanceForCars, carLayer))
            {
                return; // Wait for car to pass
            }

            // 2. Walk towards target curb point
            Vector3 targetPos = targetPoint.position;
            targetPos.y = transform.position.y;
            Vector3 dir = (targetPos - transform.position).normalized;

            if (dir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 6f);
            }

            transform.position += dir * walkSpeed * Time.deltaTime;

            // 3. Check arrival
            if (Vector3.Distance(transform.position, targetPos) <= arrivalThreshold)
            {
                targetPoint = (targetPoint == pointA) ? pointB : pointA;
            }
        }
    }
}
