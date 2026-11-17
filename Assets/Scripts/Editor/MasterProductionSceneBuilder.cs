#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CarGame.EditorTools
{
    public static class MasterProductionSceneBuilder
    {
        [MenuItem("CarGame/★ Build Complete Game Scene (All Stages 1-8) ★", false, 1)]
        public static void BuildFullProductionGame()
        {
            if (!Application.isBatchMode)
            {
                if (!EditorUtility.DisplayDialog(
                    "Build Full Game Scene",
                    "This will build the complete production-ready 3D Driving Simulator scene with all systems (Car Physics, Touch HUD, Lights/Audio/FX, Sunrise City, AI Traffic, Missions, Fuel, Economy, and Menus).\n\nProceed?",
                    "Yes, Build Scene",
                    "Cancel"))
                {
                    return;
                }
            }

            GenerateSceneInternal();

            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog(
                    "Production Scene Built!",
                    "All 8 Stages have been successfully generated and configured in your current scene!\n\n" +
                    "1. Saved as 'Assets/Scenes/GameCity.unity'.\n" +
                    "2. Press 'Play' in Unity to drive with Touch or Keyboard controls!\n" +
                    "3. Check 'CarGame > Stage 8 > Configure Android Player Settings' for APK building.",
                    "Awesome!"
                );
            }

            Debug.Log("<color=green><b>★ [CarGame] FULL PRODUCTION GAME ASSEMBLED SUCCESSFULLY! ★</b></color>");
        }

        /// <summary>
        /// Assembles all stages, saves GameCity.unity, and registers it in Build Settings.
        /// Fully headless-safe (no GUI dialogs).
        /// </summary>
        public static void GenerateSceneInternal()
        {
            Debug.Log("[CarGame] Generating Stage 1: Vehicle Physics & Track...");
            Stage1SetupUtility.GenerateStage1Prototype();

            Debug.Log("[CarGame] Generating Stage 2: Touch HUD Canvas...");
            Stage2HUDSetupUtility.GenerateHUD();

            Debug.Log("[CarGame] Generating Stage 3: Lights, Audio, Skidmarks & Damage...");
            Stage3FXSetupUtility.AttachFXToVehicle();

            Debug.Log("[CarGame] Generating Stage 4: City World, Apartment Blocks & Minimap...");
            Stage4CitySetupUtility.GenerateCityEnvironment();

            Debug.Log("[CarGame] Generating Stage 5: AI Traffic, Traffic Lights & Pedestrians...");
            Stage5AISetupUtility.GenerateTrafficSystem();

            Debug.Log("[CarGame] Generating Stage 6: Missions, Economy, Fuel & Traffic Scoring...");
            Stage6GameplaySetupUtility.SetupGameplaySystems();

            Debug.Log("[CarGame] Generating Stage 7: Pause & Settings Menus...");
            Stage7MenuSetupUtility.SetupPauseAndSettings();

            Debug.Log("[CarGame] Generating Stage 8: Mobile Performance Manager...");
            if (Object.FindObjectOfType<CarGame.Managers.MobilePerformanceManager>() == null)
            {
                GameObject perfObj = new GameObject("MobilePerformanceManager");
                perfObj.AddComponent<CarGame.Managers.MobilePerformanceManager>();
            }

            // Ensure directory exists and save GameCity.unity
            if (!Directory.Exists("Assets/Scenes")) Directory.CreateDirectory("Assets/Scenes");
            string scenePath = "Assets/Scenes/GameCity.unity";
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);

            // Register in EditorBuildSettings
            EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };

            Debug.Log($"[CarGame] GameCity.unity saved and set as primary scene in Build Settings.");
        }

        /// <summary>
        /// Called automatically in headless CI/CD (GitHub Actions) to build the Android APK.
        /// </summary>
        public static void BuildAndroidAPKHeadless()
        {
            Debug.Log("=================================================");
            Debug.Log("[CarGame CI] Starting Headless Android Build...");
            Debug.Log("=================================================");

            // 1. Generate full game scene
            GenerateSceneInternal();

            // 2. Configure Android settings (Landscape, ARM64, etc.)
            AndroidBuildSetupUtility.ConfigureAndroidSettings();

            // 3. Ensure destination path
            string outputDir = Path.GetFullPath("build/Android");
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);
            string apkPath = Path.Combine(outputDir, "SunriseHatchback_Simulator.apk");

            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/GameCity.unity" },
                locationPathName = apkPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            Debug.Log($"[CarGame CI] Compiling Android APK to: {apkPath}");
            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[CarGame CI] ★ BUILD SUCCEEDED! ★ Size: {summary.totalSize / (1024 * 1024)} MB");
            }
            else
            {
                Debug.LogError($"[CarGame CI] Build failed with {summary.totalErrors} errors!");
                EditorApplication.Exit(1);
            }
        }
    }
}
#endif
