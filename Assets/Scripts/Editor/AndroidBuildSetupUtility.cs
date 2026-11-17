#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace CarGame.EditorTools
{
    public static class AndroidBuildSetupUtility
    {
        [MenuItem("CarGame/Stage 8/Configure Android Player Settings (Production Ready)", false, 80)]
        public static void ConfigureAndroidSettings()
        {
            // 1. Lock Screen Orientation to Landscape
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

            // 2. Linear Color Space (Required for authentic URP wet road reflections)
            PlayerSettings.colorSpace = ColorSpace.Linear;

            // 3. Android Target Architecture & IL2CPP
            NamedBuildTarget androidTarget = NamedBuildTarget.Android;
            PlayerSettings.SetScriptingBackend(androidTarget, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            // 4. Android SDK Levels (Min API 24: Android 7.0 Nougat)
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            // 5. Optimization & Graphics Settings
            PlayerSettings.MTRendering = true; // Multithreaded rendering
            PlayerSettings.gpuSkinning = true;

            // 6. Set Package Name and App Title
            if (string.IsNullOrEmpty(PlayerSettings.companyName) || PlayerSettings.companyName == "DefaultCompany")
            {
                PlayerSettings.companyName = "StudioDrive";
            }
            PlayerSettings.productName = "Sunrise Hatchback Simulator";

            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog(
                    "Android Settings Configured",
                    "Successfully configured Android Build Settings:\n\n" +
                    "• Orientation: Landscape Left/Right\n" +
                    "• Scripting Backend: IL2CPP\n" +
                    "• Architecture: ARM64\n" +
                    "• Minimum API: Level 24 (Android 7.0)\n" +
                    "• Color Space: Linear (URP Wet Reflections)\n" +
                    "• Multithreaded Rendering: Enabled\n\n" +
                    "Ready for APK / AAB compilation!",
                    "Great!"
                );
            }

            Debug.Log("<color=green><b>[CarGame] Android Player Settings Configured for Production Release!</b></color>");
        }
    }
}
#endif
