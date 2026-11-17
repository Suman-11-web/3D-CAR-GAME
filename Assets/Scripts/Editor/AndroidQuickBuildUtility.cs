#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CarGame.EditorTools
{
    public static class AndroidQuickBuildUtility
    {
        [MenuItem("CarGame/Stage 8/★ 1-Click Build Android APK ★", false, 85)]
        public static void BuildAndroidAPK()
        {
            // 1. Ensure Player Settings are tuned
            AndroidBuildSetupUtility.ConfigureAndroidSettings();

            // 2. Determine Scene to build
            Scene activeScene = SceneManager.GetActiveScene();
            string scenePath = activeScene.path;

            if (string.IsNullOrEmpty(scenePath))
            {
                // Save active scene first if unsaved
                if (!Directory.Exists("Assets/Scenes")) Directory.CreateDirectory("Assets/Scenes");
                scenePath = "Assets/Scenes/GameCity.unity";
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(activeScene, scenePath);
                Debug.Log($"[AndroidBuild] Saved active scene to {scenePath}");
            }

            // 3. Setup Output Folder
            string outputDir = Path.GetFullPath("Builds/Android");
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            string apkFileName = "SunriseHatchback_Simulator.apk";
            string apkFullPath = Path.Combine(outputDir, apkFileName);

            // 4. Configure Build Options
            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = new[] { scenePath },
                locationPathName = apkFullPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            Debug.Log($"<color=cyan><b>[AndroidBuild] Starting APK Build...</b> Target: {apkFullPath}</color>");

            // 5. Execute Build
            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"<color=green><b>★ APK BUILD SUCCEEDED! ★</b> File size: {summary.totalSize / (1024 * 1024)} MB</color>");
                if (!Application.isBatchMode)
                {
                    EditorUtility.RevealInFinder(apkFullPath);
                    EditorUtility.DisplayDialog(
                        "APK Build Succeeded!",
                        $"Your Android APK was built successfully!\n\n" +
                        $"Location: {apkFullPath}\n" +
                        $"Size: {summary.totalSize / (1024 * 1024)} MB\n\n" +
                        "You can now transfer this APK to your phone via USB or Google Drive and install it!",
                        "Open Folder"
                    );
                }
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError($"<color=red><b>[AndroidBuild] Build Failed with {summary.totalErrors} errors.</b> Check Unity Console for details.</color>");
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog(
                        "Build Failed",
                        "The APK build could not complete.\n\n" +
                        "Common Causes:\n" +
                        "1. Unity Hub does not have 'Android Build Support' + NDK/SDK installed.\n" +
                        "2. Android platform is not switched in File > Build Settings.\n\n" +
                        "Check the Unity Console for exact error messages.",
                        "OK"
                    );
                }
                else
                {
                    EditorApplication.Exit(1);
                }
            }
        }
    }
}
#endif
