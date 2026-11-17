using UnityEngine;
using UnityEngine.Rendering;

namespace CarGame.Managers
{
    public enum DevicePerformanceTier
    {
        LowEnd,
        MidRange,
        HighEnd
    }

    /// <summary>
    /// Automatic mobile hardware detection and dynamic performance optimization.
    /// Manages 60 FPS frame pacing, shadow draw distance, and dynamic resolution.
    /// </summary>
    public class MobilePerformanceManager : MonoBehaviour
    {
        public static MobilePerformanceManager Instance { get; private set; }

        [Header("Target Frame Rate")]
        [SerializeField] private int targetFrameRate = 60;

        [Header("Quality Tuning")]
        [SerializeField] private bool autoDetectHardware = true;
        [SerializeField] private DevicePerformanceTier detectedTier = DevicePerformanceTier.MidRange;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                ApplyPerformanceProfile();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void ApplyPerformanceProfile()
        {
            // 1. Frame Rate Target
            Application.targetFrameRate = targetFrameRate;
            QualitySettings.vSyncCount = 0; // Handled by mobile driver

            // 2. Prevent mobile sleep during gameplay
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // 3. Hardware Auto-Detection
            if (autoDetectHardware)
            {
                detectedTier = EvaluateHardwareTier();
                ApplyTierSettings(detectedTier);
            }
        }

        private DevicePerformanceTier EvaluateHardwareTier()
        {
            int sysMemory = SystemInfo.systemMemorySize;
            int processorCount = SystemInfo.processorCount;
            int vram = SystemInfo.graphicsMemorySize;

            Debug.Log($"[Performance] Device Hardware: RAM={sysMemory}MB, Cores={processorCount}, VRAM={vram}MB, GPU={SystemInfo.graphicsDeviceName}");

            // High Tier: >= 6GB RAM and >= 8 Cores
            if (sysMemory >= 5500 && processorCount >= 8)
            {
                return DevicePerformanceTier.HighEnd;
            }
            // Low Tier: <= 3GB RAM or <= 4 Cores
            else if (sysMemory < 3200 || processorCount <= 4)
            {
                return DevicePerformanceTier.LowEnd;
            }

            return DevicePerformanceTier.MidRange;
        }

        public void ApplyTierSettings(DevicePerformanceTier tier)
        {
            detectedTier = tier;

            switch (tier)
            {
                case DevicePerformanceTier.LowEnd:
                    QualitySettings.SetQualityLevel(0, true); // Low
                    QualitySettings.shadowDistance = 35f;
                    QualitySettings.shadows = ShadowQuality.HardOnly;
                    Application.targetFrameRate = 30; // 30 FPS cap for thermal stability
                    break;

                case DevicePerformanceTier.MidRange:
                    QualitySettings.SetQualityLevel(1, true); // Medium
                    QualitySettings.shadowDistance = 65f;
                    QualitySettings.shadows = ShadowQuality.All;
                    Application.targetFrameRate = 60;
                    break;

                case DevicePerformanceTier.HighEnd:
                    QualitySettings.SetQualityLevel(2, true); // High
                    QualitySettings.shadowDistance = 110f;
                    QualitySettings.shadows = ShadowQuality.All;
                    Application.targetFrameRate = 60;
                    break;
            }

            Debug.Log($"[Performance] Applied optimization profile for tier: {tier}");
        }
    }
}
