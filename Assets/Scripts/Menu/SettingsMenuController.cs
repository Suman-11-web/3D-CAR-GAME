using UnityEngine;
using UnityEngine.UI;
using CarGame.SaveSystem;
using CarGame.InputSystem;
using CarGame.UI;

namespace CarGame.Menu
{
    /// <summary>
    /// Settings menu controller:
    /// Configures graphics quality (Low/Medium/High), audio levels,
    /// steering modes (Wheel/Buttons/Gyro), steering sensitivity, and speed units.
    /// </summary>
    public class SettingsMenuController : MonoBehaviour
    {
        [Header("Audio Sliders")]
        [SerializeField] private Slider masterVolumeSlider;

        [Header("Graphics Quality Buttons / Dropdown")]
        [SerializeField] private int activeQualityLevel = 1;

        [Header("Steering Mode")]
        [SerializeField] private SteeringControlType selectedSteeringMode = SteeringControlType.SteeringWheel;
        [SerializeField] private Slider steeringSensitivitySlider;

        [Header("Units")]
        [SerializeField] private Toggle mphToggle;

        private void Start()
        {
            LoadSettings();
        }

        public void LoadSettings()
        {
            if (SaveManager.Instance == null) return;
            var data = SaveManager.Instance.Data;

            // Audio
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = data.sfxVolume;
                masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            }

            // Graphics
            SetGraphicsQuality(data.graphicsQualityLevel);

            // Steering Mode
            SetSteeringControlMode((SteeringControlType)data.controlType);

            // Sensitivity
            if (steeringSensitivitySlider != null)
            {
                steeringSensitivitySlider.value = data.steeringSensitivity;
                steeringSensitivitySlider.onValueChanged.AddListener(SetSteeringSensitivity);
            }

            // Unit
            if (mphToggle != null)
            {
                mphToggle.isOn = data.useMph;
                mphToggle.onValueChanged.AddListener(SetSpeedUnitMph);
            }
        }

        public void SetMasterVolume(float vol)
        {
            AudioListener.volume = vol;
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.Data.sfxVolume = vol;
                SaveManager.Instance.SaveGame();
            }
        }

        public void SetGraphicsQuality(int level)
        {
            activeQualityLevel = Mathf.Clamp(level, 0, QualitySettings.names.Length - 1);
            QualitySettings.SetQualityLevel(activeQualityLevel, true);

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.Data.graphicsQualityLevel = activeQualityLevel;
                SaveManager.Instance.SaveGame();
            }
        }

        public void SetSteeringControlMode(SteeringControlType mode)
        {
            selectedSteeringMode = mode;
            if (VehicleHUDController.Instance != null)
            {
                VehicleHUDController.Instance.ApplySteeringMode(mode);
            }
            if (VehicleInputManager.Instance != null)
            {
                VehicleInputManager.Instance.controlType = mode;
            }

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.Data.controlType = (int)mode;
                SaveManager.Instance.SaveGame();
            }
        }

        public void SetSteeringSensitivity(float sens)
        {
            if (VehicleInputManager.Instance != null)
            {
                VehicleInputManager.Instance.touchSensitivity = sens;
                VehicleInputManager.Instance.tiltSensitivity = sens;
            }

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.Data.steeringSensitivity = sens;
                SaveManager.Instance.SaveGame();
            }
        }

        public void SetSpeedUnitMph(bool isMph)
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.Data.useMph = isMph;
                SaveManager.Instance.SaveGame();
            }
        }
    }
}
