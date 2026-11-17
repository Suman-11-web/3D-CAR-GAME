using UnityEngine;
using CarGame.Vehicle;
using CarGame.InputSystem;

namespace CarGame.Audio
{
    /// <summary>
    /// Comprehensive vehicle audio management:
    /// Dynamic engine pitch & volume based on RPM and throttle load,
    /// dynamic tire screech on slip, collision impacts, horn, and gear shifting.
    /// Includes procedural audio fallbacks so audio works immediately without external clips.
    /// </summary>
    [RequireComponent(typeof(VehicleController))]
    public class VehicleAudioSystem : MonoBehaviour
    {
        [Header("Engine Audio")]
        [SerializeField] private AudioSource engineAudioSource;
        [SerializeField] private AudioClip engineIdleClip;
        [SerializeField] private float minPitch = 0.75f;
        [SerializeField] private float maxPitch = 2.4f;
        [SerializeField] private float minVolume = 0.35f;
        [SerializeField] private float maxVolume = 0.95f;

        [Header("Tire Skid Audio")]
        [SerializeField] private AudioSource skidAudioSource;
        [SerializeField] private AudioClip skidClip;
        [SerializeField] private float skidVolumeSensitivity = 1.8f;

        [Header("Horn Audio")]
        [SerializeField] private AudioSource hornAudioSource;
        [SerializeField] private AudioClip hornClip;

        [Header("Collision & Impacts")]
        [SerializeField] private AudioSource crashAudioSource;
        [SerializeField] private AudioClip[] crashClips;

        [Header("Gear Shift Click")]
        [SerializeField] private AudioSource gearShiftAudioSource;
        [SerializeField] private AudioClip gearShiftClip;

        private VehicleController vehicle;
        private int lastObservedGear = 1;

        private void Awake()
        {
            vehicle = GetComponent<VehicleController>();
            SetupAudioSources();
        }

        private void SetupAudioSources()
        {
            // Configure Engine Source
            if (engineAudioSource == null) engineAudioSource = gameObject.AddComponent<AudioSource>();
            engineAudioSource.loop = true;
            engineAudioSource.playOnAwake = false;
            engineAudioSource.spatialBlend = 0.5f;
            if (engineIdleClip == null) engineIdleClip = GenerateProceduralEngineClip();
            engineAudioSource.clip = engineIdleClip;

            // Configure Skid Source
            if (skidAudioSource == null) skidAudioSource = gameObject.AddComponent<AudioSource>();
            skidAudioSource.loop = true;
            skidAudioSource.playOnAwake = false;
            skidAudioSource.spatialBlend = 0.5f;
            if (skidClip == null) skidClip = GenerateProceduralSkidClip();
            skidAudioSource.clip = skidClip;
            skidAudioSource.volume = 0f;

            // Configure Horn Source
            if (hornAudioSource == null) hornAudioSource = gameObject.AddComponent<AudioSource>();
            hornAudioSource.loop = true;
            hornAudioSource.playOnAwake = false;
            if (hornClip == null) hornClip = GenerateProceduralHornClip();
            hornAudioSource.clip = hornClip;

            // Configure Crash Source
            if (crashAudioSource == null) crashAudioSource = gameObject.AddComponent<AudioSource>();
            crashAudioSource.loop = false;
            crashAudioSource.playOnAwake = false;
            if (crashClips == null || crashClips.Length == 0)
            {
                crashClips = new AudioClip[] { GenerateProceduralCrashClip() };
            }

            // Configure Gear Shift Source
            if (gearShiftAudioSource == null) gearShiftAudioSource = gameObject.AddComponent<AudioSource>();
            gearShiftAudioSource.loop = false;
            gearShiftAudioSource.playOnAwake = false;
        }

        private void Start()
        {
            if (engineAudioSource.clip != null && !engineAudioSource.isPlaying)
            {
                engineAudioSource.Play();
            }
            if (skidAudioSource.clip != null && !skidAudioSource.isPlaying)
            {
                skidAudioSource.Play();
            }
        }

        private void Update()
        {
            if (vehicle == null) return;

            // 1. Modulate Engine Sound with RPM & Throttle
            float rpmRatio = Mathf.Clamp01((vehicle.CurrentRPM - vehicle.IdleRPM) / (vehicle.MaxRPM - vehicle.IdleRPM));
            float targetPitch = Mathf.Lerp(minPitch, maxPitch, rpmRatio);
            engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, targetPitch, Time.deltaTime * 12f);

            float throttle = VehicleInputManager.Instance != null ? VehicleInputManager.Instance.Throttle : 0f;
            float targetVolume = Mathf.Lerp(minVolume, maxVolume, (rpmRatio * 0.7f) + (throttle * 0.3f));
            engineAudioSource.volume = Mathf.Lerp(engineAudioSource.volume, targetVolume, Time.deltaTime * 10f);

            // 2. Modulate Skid Audio
            float skidInput = (vehicle.TractionControlActive || vehicle.AbsActive || (VehicleInputManager.Instance != null && VehicleInputManager.Instance.Handbrake && vehicle.CurrentSpeedKmh > 10f)) ? 0.75f : 0f;
            skidAudioSource.volume = Mathf.Lerp(skidAudioSource.volume, skidInput * skidVolumeSensitivity, Time.deltaTime * 18f);

            // 3. Horn Audio
            bool hornRequested = VehicleInputManager.Instance != null && VehicleInputManager.Instance.HornActive;
            if (hornRequested && !hornAudioSource.isPlaying && hornAudioSource.clip != null)
            {
                hornAudioSource.Play();
            }
            else if (!hornRequested && hornAudioSource.isPlaying)
            {
                hornAudioSource.Stop();
            }

            // 4. Gear Shift Click
            if (vehicle.CurrentGear != lastObservedGear)
            {
                lastObservedGear = vehicle.CurrentGear;
                PlayGearShiftSound();
            }
        }

        public void PlayCrashSound(float impactIntensity)
        {
            if (crashClips != null && crashClips.Length > 0 && crashAudioSource != null)
            {
                AudioClip clip = crashClips[Random.Range(0, crashClips.Length)];
                float vol = Mathf.Clamp01(impactIntensity / 25f);
                crashAudioSource.PlayOneShot(clip, Mathf.Max(vol, 0.4f));
            }
        }

        private void PlayGearShiftSound()
        {
            if (gearShiftAudioSource != null && gearShiftClip != null)
            {
                gearShiftAudioSource.PlayOneShot(gearShiftClip, 0.6f);
            }
        }

        #region Procedural Audio Generators (Zero External Asset Dependencies)

        private AudioClip GenerateProceduralEngineClip()
        {
            int sampleRate = 22050;
            int samples = sampleRate;
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                data[i] = (Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.5f) +
                          (Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.3f) +
                          (Random.Range(-0.06f, 0.06f));
            }
            AudioClip clip = AudioClip.Create("Procedural_Engine_Idle", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateProceduralSkidClip()
        {
            int sampleRate = 22050;
            int samples = sampleRate;
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                data[i] = Random.Range(-0.35f, 0.35f) * (0.8f + 0.2f * Mathf.Sin(2f * Mathf.PI * 400f * t));
            }
            AudioClip clip = AudioClip.Create("Procedural_Tire_Skid", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateProceduralHornClip()
        {
            int sampleRate = 22050;
            int samples = sampleRate;
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                data[i] = (Mathf.Sin(2f * Mathf.PI * 440f * t) * 0.45f) +
                          (Mathf.Sin(2f * Mathf.PI * 520f * t) * 0.45f);
            }
            AudioClip clip = AudioClip.Create("Procedural_Car_Horn", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateProceduralCrashClip()
        {
            int sampleRate = 22050;
            int samples = sampleRate / 2;
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float decay = 1f - ((float)i / samples);
                data[i] = Random.Range(-0.85f, 0.85f) * decay * decay;
            }
            AudioClip clip = AudioClip.Create("Procedural_Crash_Impact", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        #endregion
    }
}
