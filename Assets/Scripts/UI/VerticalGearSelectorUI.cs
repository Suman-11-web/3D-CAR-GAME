using UnityEngine;
using UnityEngine.UI;
using CarGame.Vehicle;
using CarGame.InputSystem;

namespace CarGame.UI
{
    /// <summary>
    /// Vertical Automatic Transmission Gear Selector (P / R / N / D).
    /// Matches the vertical gear selector layout on the right side of the HUD.
    /// </summary>
    public class VerticalGearSelectorUI : MonoBehaviour
    {
        [Header("Gear Buttons")]
        [SerializeField] private Button btnPark;
        [SerializeField] private Button btnReverse;
        [SerializeField] private Button btnNeutral;
        [SerializeField] private Button btnDrive;

        [Header("Visual Feedback Colors")]
        [SerializeField] private Color activeColor = new Color(0.2f, 0.85f, 0.3f, 1.0f);
        [SerializeField] private Color inactiveColor = new Color(0.85f, 0.85f, 0.85f, 0.6f);

        [Header("Visual Indicators (Images or Backgrounds)")]
        [SerializeField] private Image imgPark;
        [SerializeField] private Image imgReverse;
        [SerializeField] private Image imgNeutral;
        [SerializeField] private Image imgDrive;

        private GearMode currentMode = GearMode.D;

        private void Start()
        {
            if (btnPark != null) btnPark.onClick.AddListener(() => SelectGear(GearMode.P));
            if (btnReverse != null) btnReverse.onClick.AddListener(() => SelectGear(GearMode.R));
            if (btnNeutral != null) btnNeutral.onClick.AddListener(() => SelectGear(GearMode.N));
            if (btnDrive != null) btnDrive.onClick.AddListener(() => SelectGear(GearMode.D));

            UpdateVisuals();
        }

        public void SelectGear(GearMode mode)
        {
            currentMode = mode;
            if (VehicleInputManager.Instance != null)
            {
                VehicleInputManager.Instance.SetGearMode(mode);
            }
            UpdateVisuals();
        }

        public void SetExternalGear(GearMode mode)
        {
            if (currentMode != mode)
            {
                currentMode = mode;
                UpdateVisuals();
            }
        }

        private void UpdateVisuals()
        {
            SetIndicatorColor(imgPark, currentMode == GearMode.P);
            SetIndicatorColor(imgReverse, currentMode == GearMode.R);
            SetIndicatorColor(imgNeutral, currentMode == GearMode.N);
            SetIndicatorColor(imgDrive, currentMode == GearMode.D);
        }

        private void SetIndicatorColor(Image img, bool isActive)
        {
            if (img != null)
            {
                img.color = isActive ? activeColor : inactiveColor;
                img.transform.localScale = isActive ? Vector3.one * 1.15f : Vector3.one;
            }
        }
    }
}
