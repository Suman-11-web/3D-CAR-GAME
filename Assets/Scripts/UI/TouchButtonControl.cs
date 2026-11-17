using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CarGame.UI
{
    public enum ButtonBehavior
    {
        Momentary, // Active while pressed down
        Toggle     // Flips state on tap
    }

    /// <summary>
    /// Multi-touch compliant UI button for auxiliary vehicle actions
    /// (Handbrake, Hazard lights, Turn signals, Camera view, Horn).
    /// </summary>
    public class TouchButtonControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Behavior")]
        [SerializeField] private ButtonBehavior behavior = ButtonBehavior.Momentary;

        [Header("Visual Feedback")]
        [SerializeField] private Image buttonImage;
        [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.7f);
        [SerializeField] private Color activeColor = new Color(1f, 0.6f, 0.1f, 1.0f);

        [Header("Events")]
        public UnityEvent<bool> onStateChanged;

        private bool isPressed;
        private bool isToggled;
        private int activePointerId = -1;

        public bool IsActive => behavior == ButtonBehavior.Toggle ? isToggled : isPressed;

        private void Awake()
        {
            if (buttonImage == null) buttonImage = GetComponent<Image>();
            UpdateVisual();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (behavior == ButtonBehavior.Momentary)
            {
                if (isPressed) return;
                activePointerId = eventData.pointerId;
                isPressed = true;
                UpdateVisual();
                onStateChanged?.Invoke(true);
            }
            else // Toggle
            {
                isToggled = !isToggled;
                UpdateVisual();
                onStateChanged?.Invoke(isToggled);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (behavior == ButtonBehavior.Momentary && eventData.pointerId == activePointerId)
            {
                isPressed = false;
                activePointerId = -1;
                UpdateVisual();
                onStateChanged?.Invoke(false);
            }
        }

        public void SetState(bool active)
        {
            if (behavior == ButtonBehavior.Toggle)
            {
                isToggled = active;
            }
            else
            {
                isPressed = active;
            }
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (buttonImage != null)
            {
                buttonImage.color = IsActive ? activeColor : normalColor;
            }
        }
    }
}
