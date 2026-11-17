using UnityEngine;

namespace CarGame.Environment
{
    public enum TimeOfDayPreset
    {
        Sunrise, // Golden morning light (matches reference screenshot)
        Day,     // Clear bright midday
        Sunset,  // Deep warm orange dusk
        Night    // Dark night with city street lamps
    }

    /// <summary>
    /// Manages celestial sun orientation, color temperature, ambient light,
    /// and triggers street lamp illumination based on the time of day.
    /// </summary>
    public class TimeOfDayManager : MonoBehaviour
    {
        public static TimeOfDayManager Instance { get; private set; }

        [Header("Lighting References")]
        [SerializeField] private Light sunLight;

        [Header("Preset & Progression")]
        [SerializeField] private TimeOfDayPreset activePreset = TimeOfDayPreset.Sunrise;
        [SerializeField] private bool autoProgressTime = false;
        [SerializeField] private float dayCycleDurationSeconds = 300f; // 5 min full day cycle
        [Range(0f, 24f)] [SerializeField] private float currentHour = 6.8f; // 6:48 AM Sunrise

        [Header("Sunrise Preset")]
        [SerializeField] private Vector3 sunriseSunRotation = new Vector3(14f, 15f, 0f);
        [SerializeField] private Color sunriseSunColor = new Color(1.0f, 0.85f, 0.72f);
        [SerializeField] private float sunriseSunIntensity = 1.35f;

        [Header("Day Preset")]
        [SerializeField] private Vector3 daySunRotation = new Vector3(65f, 50f, 0f);
        [SerializeField] private Color daySunColor = new Color(1.0f, 0.98f, 0.92f);
        [SerializeField] private float daySunIntensity = 1.5f;

        [Header("Sunset Preset")]
        [SerializeField] private Vector3 sunsetSunRotation = new Vector3(10f, 190f, 0f);
        [SerializeField] private Color sunsetSunColor = new Color(1.0f, 0.52f, 0.25f);
        [SerializeField] private float sunsetSunIntensity = 1.2f;

        [Header("Night Preset")]
        [SerializeField] private Vector3 nightSunRotation = new Vector3(-35f, 180f, 0f);
        [SerializeField] private Color nightSunColor = new Color(0.15f, 0.22f, 0.45f);
        [SerializeField] private float nightSunIntensity = 0.15f;

        [Header("City Street Lights (Auto on during Night/Dusk)")]
        [SerializeField] private Light[] streetLights;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (sunLight == null)
            {
                sunLight = RenderSettings.sun != null ? RenderSettings.sun : FindObjectOfType<Light>();
            }
        }

        private void Start()
        {
            ApplyPreset(activePreset);
        }

        private void Update()
        {
            if (autoProgressTime)
            {
                float hourStep = (24f / dayCycleDurationSeconds) * Time.deltaTime;
                currentHour = (currentHour + hourStep) % 24f;
                UpdateSunFromHour(currentHour);
            }
        }

        public void ApplyPreset(TimeOfDayPreset preset)
        {
            activePreset = preset;
            if (sunLight == null) return;

            switch (preset)
            {
                case TimeOfDayPreset.Sunrise:
                    currentHour = 6.8f;
                    sunLight.transform.rotation = Quaternion.Euler(sunriseSunRotation);
                    sunLight.color = sunriseSunColor;
                    sunLight.intensity = sunriseSunIntensity;
                    SetStreetLights(false);
                    break;

                case TimeOfDayPreset.Day:
                    currentHour = 12.0f;
                    sunLight.transform.rotation = Quaternion.Euler(daySunRotation);
                    sunLight.color = daySunColor;
                    sunLight.intensity = daySunIntensity;
                    SetStreetLights(false);
                    break;

                case TimeOfDayPreset.Sunset:
                    currentHour = 18.2f;
                    sunLight.transform.rotation = Quaternion.Euler(sunsetSunRotation);
                    sunLight.color = sunsetSunColor;
                    sunLight.intensity = sunsetSunIntensity;
                    SetStreetLights(true);
                    break;

                case TimeOfDayPreset.Night:
                    currentHour = 23.5f;
                    sunLight.transform.rotation = Quaternion.Euler(nightSunRotation);
                    sunLight.color = nightSunColor;
                    sunLight.intensity = nightSunIntensity;
                    SetStreetLights(true);
                    break;
            }
        }

        private void UpdateSunFromHour(float hour)
        {
            if (hour >= 6f && hour < 8f) ApplyPreset(TimeOfDayPreset.Sunrise);
            else if (hour >= 8f && hour < 17f) ApplyPreset(TimeOfDayPreset.Day);
            else if (hour >= 17f && hour < 20f) ApplyPreset(TimeOfDayPreset.Sunset);
            else ApplyPreset(TimeOfDayPreset.Night);
        }

        private void SetStreetLights(bool on)
        {
            if (streetLights == null) return;
            foreach (var lt in streetLights)
            {
                if (lt != null) lt.enabled = on;
            }
        }
    }
}
