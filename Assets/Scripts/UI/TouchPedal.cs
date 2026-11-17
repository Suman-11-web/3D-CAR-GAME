using UnityEngine;
using UnityEngine.EventSystems;
using CarGame.InputSystem;

namespace CarGame.UI
{
    public enum PedalType
    {
        Accelerator,
        Brake
    }

    /// <summary>
    /// Pressure-sensitive touch pedal (Accelerator or Brake).
    /// Simulates authentic hydraulic/cable pedal travel when held.
    /// Supports multi-touch independently from the steering wheel.
    /// </summary>
    public class TouchPedal : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("Pedal Type")]
        [SerializeField] private PedalType pedalType = PedalType.Accelerator;

        [Header("Pressure Dynamics")]
        [Tooltip("Seconds required to reach 100% pedal depression while held")]
        [SerializeField] private float pressRiseTime = 0.18f;

        [Tooltip("Seconds required to return to 0% after release")]
        [SerializeField] private float releaseFallTime = 0.08f;

        [Header("Visual Feedback (Optional)")]
        [SerializeField] private RectTransform pedalTransform;
        [SerializeField] private float visualPressDepressionY = -12f;

        private bool isHeld;
        private int activePointerId = -1;
        private float currentPedalValue;
        private Vector2 originalPosition;

        public float PedalValue => currentPedalValue; // 0.0 to 1.0

        private void Awake()
        {
            if (pedalTransform == null) pedalTransform = GetComponent<RectTransform>();
            if (pedalTransform != null) originalPosition = pedalTransform.anchoredPosition;
        }

        private void Update()
        {
            if (isHeld)
            {
                // Smoothly ramp up pressure to 1.0
                float riseSpeed = pressRiseTime > 0f ? (1f / pressRiseTime) : 100f;
                currentPedalValue = Mathf.MoveTowards(currentPedalValue, 1f, riseSpeed * Time.deltaTime);
            }
            else
            {
                // Rapidly release back to 0.0
                float fallSpeed = releaseFallTime > 0f ? (1f / releaseFallTime) : 100f;
                currentPedalValue = Mathf.MoveTowards(currentPedalValue, 0f, fallSpeed * Time.deltaTime);
            }

            // Transmit to Input Manager
            if (VehicleInputManager.Instance != null)
            {
                if (pedalType == PedalType.Accelerator)
                {
                    VehicleInputManager.Instance.SetVirtualThrottle(currentPedalValue);
                }
                else
                {
                    VehicleInputManager.Instance.SetVirtualBrake(currentPedalValue);
                }
            }

            // Visual depression feedback
            if (pedalTransform != null)
            {
                pedalTransform.anchoredPosition = originalPosition + new Vector2(0f, visualPressDepressionY * currentPedalValue);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isHeld) return;
            activePointerId = eventData.pointerId;
            isHeld = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Keep pressed while dragging within pedal zone
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId) return;
            isHeld = false;
            activePointerId = -1;
        }

        private void OnDisable()
        {
            isHeld = false;
            activePointerId = -1;
            currentPedalValue = 0f;
            if (pedalTransform != null) pedalTransform.anchoredPosition = originalPosition;
        }
    }
}
