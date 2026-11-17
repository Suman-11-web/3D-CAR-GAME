using UnityEngine;
using CarGame.Vehicle;

namespace CarGame.Environment
{
    public enum WeatherType
    {
        Clear,
        Rain
    }

    /// <summary>
    /// Controls weather effects (rain particles, wet road reflections, and ambient rain audio).
    /// </summary>
    public class WeatherSystem : MonoBehaviour
    {
        public static WeatherSystem Instance { get; private set; }

        [Header("Weather Mode")]
        [SerializeField] private WeatherType currentWeather = WeatherType.Rain;

        [Header("Rain Effects")]
        [SerializeField] private ParticleSystem rainParticleSystem;
        [SerializeField] private AudioSource rainAudioSource;

        [Header("Wet Road Material / Puddles")]
        [SerializeField] private Material asphaltMaterial;
        [SerializeField] private float wetSmoothness = 0.88f;
        [SerializeField] private float drySmoothness = 0.35f;

        [Header("Vehicle Reference")]
        [SerializeField] private VehicleController playerVehicle;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (playerVehicle == null) playerVehicle = FindObjectOfType<VehicleController>();
            ApplyWeather(currentWeather);
        }

        public void ApplyWeather(WeatherType type)
        {
            currentWeather = type;
            bool isRaining = type == WeatherType.Rain;

            // 1. Rain Particles
            if (rainParticleSystem != null)
            {
                var emission = rainParticleSystem.emission;
                emission.enabled = isRaining;
            }

            // 2. Wet Road Material Reflections
            if (asphaltMaterial != null)
            {
                float targetSmoothness = isRaining ? wetSmoothness : drySmoothness;
                if (asphaltMaterial.HasProperty("_Smoothness"))
                {
                    asphaltMaterial.SetFloat("_Smoothness", targetSmoothness);
                }
                else if (asphaltMaterial.HasProperty("_Glossiness"))
                {
                    asphaltMaterial.SetFloat("_Glossiness", targetSmoothness);
                }
            }

            // 3. Ambient Audio
            if (rainAudioSource != null)
            {
                if (isRaining && !rainAudioSource.isPlaying) rainAudioSource.Play();
                else if (!isRaining && rainAudioSource.isPlaying) rainAudioSource.Stop();
            }

            // 4. Vehicle Grip
            if (playerVehicle != null)
            {
                playerVehicle.SetSurfaceFrictionMultiplier(isRaining ? 0.85f : 1.0f);
            }
        }

        public void ToggleRain()
        {
            ApplyWeather(currentWeather == WeatherType.Clear ? WeatherType.Rain : WeatherType.Clear);
        }
    }
}
