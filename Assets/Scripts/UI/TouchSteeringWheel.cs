using UnityEngine;
using UnityEngine.EventSystems;
using CarGame.InputSystem;

namespace CarGame.UI
{
    /// <summary>
    /// Interactive On-Screen Steering Wheel for mobile driving.
    /// Supports touch rotation, auto-centering on release, and center horn button.
    /// Multi-touch compatible via Unity EventSystem.
    /// </summary>
    public class TouchSteeringWheel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("Wheel Configuration")]
        [Tooltip("Maximum rotation angle of the steering wheel in either direction (degrees)")]
        [SerializeField] private float maxSteerAngle = 450f;

        [Tooltip("Speed at which the wheel returns to center when released")]
        [SerializeField] private float autoCenterSpeed = 12f;

        [Tooltip("Threshold distance from center considered a Horn tap")]
        [SerializeField] private float hornRadiusThreshold = 55f;

        [Header("References")]
        [SerializeField] private RectTransform wheelRect;

        // State
        private bool isHeld;
        private int activePointerId = -1;
        private float currentWheelAngle;
        private Vector2 centerPoint;
        private float initialAngleOffset;

        public float SteeringOutput { get; private set; } // -1.0 to +1.0
        public bool IsHornPressed { get; private set; }

        private void Awake()
        {
            if (wheelRect == null) wheelRect = GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (!isHeld)
            {
                // Smoothly return to center
                currentWheelAngle = Mathf.MoveTowards(currentWheelAngle, 0f, autoCenterSpeed * 100f * Time.deltaTime);
                SteeringOutput = Mathf.Clamp(currentWheelAngle / maxSteerAngle, -1f, 1f);
                UpdateWheelVisual();
            }

            // Transmit input to vehicle input manager
            if (VehicleInputManager.Instance != null && isHeld)
            {
                VehicleInputManager.Instance.SetVirtualSteering(SteeringOutput);
            }
            else if (VehicleInputManager.Instance != null && !isHeld && Mathf.Abs(SteeringOutput) > 0.001f)
            {
                VehicleInputManager.Instance.SetVirtualSteering(SteeringOutput);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isHeld) return; // Already controlled by another touch

            activePointerId = eventData.pointerId;
            isHeld = true;

            // Compute center point in screen coordinates
            centerPoint = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, wheelRect.position);

            Vector2 dir = eventData.position - centerPoint;

            // Check if user tapped inside the center horn area
            if (dir.magnitude <= hornRadiusThreshold)
            {
                IsHornPressed = true;
                if (VehicleInputManager.Instance != null) VehicleInputManager.Instance.SetHorn(true);
            }

            // Calculate touch angle relative to wheel center
            float touchAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            initialAngleOffset = touchAngle - currentWheelAngle;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isHeld || eventData.pointerId != activePointerId) return;

            Vector2 dir = eventData.position - centerPoint;
            float touchAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            float targetAngle = Mathf.DeltaAngle(0f, touchAngle - initialAngleOffset);
            currentWheelAngle = Mathf.Clamp(currentWheelAngle + Mathf.DeltaAngle(currentWheelAngle, targetAngle), -maxSteerAngle, maxSteerAngle);

            SteeringOutput = Mathf.Clamp(currentWheelAngle / maxSteerAngle, -1f, 1f);
            UpdateWheelVisual();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId) return;

            isHeld = false;
            activePointerId = -1;

            if (IsHornPressed)
            {
                IsHornPressed = false;
                if (VehicleInputManager.Instance != null) VehicleInputManager.Instance.SetHorn(false);
            }
        }

        private void UpdateWheelVisual()
        {
            if (wheelRect != null)
            {
                wheelRect.localEulerAngles = new Vector3(0f, 0f, -currentWheelAngle);
            }
        }

        public void ResetWheel()
        {
            isHeld = false;
            activePointerId = -1;
            currentWheelAngle = 0f;
            SteeringOutput = 0f;
            UpdateWheelVisual();
        }
    }
}
