using System.Collections.Generic;
using UnityEngine;

namespace CarGame.AI
{
    /// <summary>
    /// Waypoint node for AI traffic navigation along city lanes.
    /// </summary>
    public class TrafficWaypoint : MonoBehaviour
    {
        [Header("Lane Navigation")]
        public float targetSpeedKmh = 50f;
        public List<TrafficWaypoint> nextWaypoints = new List<TrafficWaypoint>();

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(transform.position, 0.6f);

            if (nextWaypoints != null)
            {
                Gizmos.color = Color.yellow;
                foreach (var next in nextWaypoints)
                {
                    if (next != null)
                    {
                        Gizmos.DrawLine(transform.position, next.transform.position);
                    }
                }
            }
        }
    }
}
