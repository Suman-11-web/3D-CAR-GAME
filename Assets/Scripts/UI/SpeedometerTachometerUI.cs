using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CarGame.Vehicle;

namespace CarGame.UI
{
    /// <summary>
    /// Analog & Digital Speedometer, Tachometer Dial, Fuel Bar, and Money HUD.
    /// Accurately matches the top-right instrumentation dial from the reference screenshot.
    /// </summary>
    public class SpeedometerTachometerUI : MonoBehaviour
    {
        [Header("Vehicle Reference")]
        [SerializeField] private VehicleController vehicle;

        [Header("Analog Dial & Needle")]
        [SerializeField] private RectTransform dialNeedle;
        [Tooltip("Needle angle at 0 RPM / 0 km/h (degrees)")]
        [SerializeField] private float needleMinAngle = 135f;
        [Tooltip("Needle angle at Max RPM / Top Speed (degrees)")]
        [SerializeField] private float needleMaxAngle = -135f;
        [SerializeField] private bool needleTracksRPM = true;

        [Header("Digital Readouts")]
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private TextMeshProUGUI gearText;
        [SerializeField] private TextMeshProUGUI speedUnitText;
        [SerializeField] private TextMeshProUGUI moneyText;

        [Header("Assists & Status Indicators")]
        [SerializeField] private Image tcsIcon;
        [SerializeField] private Image absIcon;
        [SerializeField] private Color assistActiveColor = new Color(1f, 0.65f, 0f, 1f); // Amber warning
        [SerializeField] private Color assistInactiveColor = new Color(0.35f, 0.35f, 0.35f, 0.4f);

        [Header("Fuel / Battery Bar (Top-Left)")]
        [SerializeField] private Image fuelBarFill;
        [SerializeField] private TextMeshProUGUI fuelPercentText;
        [SerializeField] private float simulatedFuelLevel = 0.95f; // Starts at 95% like screenshot

        // Internal State
        private float displayedSpeed;

        private void Start()
        {
            if (vehicle == null) vehicle = FindObjectOfType<VehicleController>();
            if (speedUnitText != null) speedUnitText.text = "km/h";
            SetMoney(450); // Matches reference screenshot "450$"
        }

        private void Update()
        {
            if (vehicle == null) return;

            // 1. Digital Speed Readout (Smooth filter for visual stability)
            float rawSpeed = vehicle.CurrentSpeedKmh;
            displayedSpeed = Mathf.Lerp(displayedSpeed, rawSpeed, Time.deltaTime * 15f);

            if (speedText != null)
            {
                speedText.text = Mathf.RoundToInt(displayedSpeed).ToString();
            }

            // 2. Current Gear Readout
            if (gearText != null)
            {
                if (vehicle.CurrentGearMode == GearMode.D)
                {
                    gearText.text = vehicle.CurrentGear.ToString();
                }
                else
                {
                    gearText.text = vehicle.CurrentGearMode.ToString();
                }
            }

            // 3. Analog Needle Rotation
            if (dialNeedle != null)
            {
                float factor = needleTracksRPM
                    ? Mathf.Clamp01(vehicle.CurrentRPM / vehicle.MaxRPM)
                    : Mathf.Clamp01(vehicle.CurrentSpeedKmh / (vehicle.Data != null ? vehicle.Data.topSpeedKmh : 220f));

                float targetAngle = Mathf.Lerp(needleMinAngle, needleMaxAngle, factor);
                dialNeedle.localEulerAngles = new Vector3(0f, 0f, targetAngle);
            }

            // 4. TCS & ABS Indicators
            if (tcsIcon != null)
            {
                tcsIcon.color = vehicle.TractionControlActive ? assistActiveColor : assistInactiveColor;
            }
            if (absIcon != null)
            {
                absIcon.color = vehicle.AbsActive ? assistActiveColor : assistInactiveColor;
            }

            // 5. Fuel / Battery Drain (Simulated gradual drain while driving)
            if (rawSpeed > 1f)
            {
                simulatedFuelLevel = Mathf.Max(simulatedFuelLevel - (Time.deltaTime * 0.0005f), 0f);
            }
            UpdateFuelDisplay();
        }

        public void SetMoney(int amount)
        {
            if (moneyText != null)
            {
                moneyText.text = amount + "$";
            }
        }

        public void SetFuelLevel(float level01)
        {
            simulatedFuelLevel = Mathf.Clamp01(level01);
            UpdateFuelDisplay();
        }

        private void UpdateFuelDisplay()
        {
            if (fuelBarFill != null)
            {
                fuelBarFill.fillAmount = simulatedFuelLevel;
            }
            if (fuelPercentText != null)
            {
                fuelPercentText.text = Mathf.RoundToInt(simulatedFuelLevel * 100f) + "%";
            }
        }
    }
}
