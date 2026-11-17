using System.Collections;
using UnityEngine;

namespace CarGame.AI
{
    public enum TrafficLightColor
    {
        Red,
        Yellow,
        Green
    }

    /// <summary>
    /// Controls city intersection traffic lights.
    /// Manages signal timing cycles and activates virtual stopping barriers for AI traffic.
    /// </summary>
    public class TrafficLightIntersection : MonoBehaviour
    {
        [Header("Timing Cycles (Seconds)")]
        [SerializeField] private float greenDuration = 12f;
        [SerializeField] private float yellowDuration = 3.5f;
        [SerializeField] private float redDuration = 14f;

        [Header("Active State")]
        [SerializeField] private TrafficLightColor currentColor = TrafficLightColor.Green;

        [Header("Visual Bulbs")]
        [SerializeField] private Renderer redBulb;
        [SerializeField] private Renderer yellowBulb;
        [SerializeField] private Renderer greenBulb;
        [SerializeField] private Material bulbOnMat;
        [SerializeField] private Material bulbOffMat;

        [Header("AI Stop Barrier (Collider detected by AI raycast)")]
        [SerializeField] private GameObject redLightStopBarrier;

        public TrafficLightColor CurrentColor => currentColor;

        private void Start()
        {
            StartCoroutine(TrafficLightCycleRoutine());
        }

        private IEnumerator TrafficLightCycleRoutine()
        {
            while (true)
            {
                // Green Phase
                SetSignalState(TrafficLightColor.Green);
                yield return new WaitForSeconds(greenDuration);

                // Yellow Phase
                SetSignalState(TrafficLightColor.Yellow);
                yield return new WaitForSeconds(yellowDuration);

                // Red Phase
                SetSignalState(TrafficLightColor.Red);
                yield return new WaitForSeconds(redDuration);
            }
        }

        public void SetSignalState(TrafficLightColor state)
        {
            currentColor = state;

            // Update Bulbs
            if (redBulb != null) redBulb.sharedMaterial = (state == TrafficLightColor.Red) ? bulbOnMat : bulbOffMat;
            if (yellowBulb != null) yellowBulb.sharedMaterial = (state == TrafficLightColor.Yellow) ? bulbOnMat : bulbOffMat;
            if (greenBulb != null) greenBulb.sharedMaterial = (state == TrafficLightColor.Green) ? bulbOnMat : bulbOffMat;

            // Activate stopping barrier on Red or Yellow
            if (redLightStopBarrier != null)
            {
                redLightStopBarrier.SetActive(state == TrafficLightColor.Red || state == TrafficLightColor.Yellow);
            }
        }
    }
}
