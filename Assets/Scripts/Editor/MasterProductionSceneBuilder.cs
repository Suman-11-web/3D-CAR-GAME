#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CarGame.EditorTools
{
    public static class MasterProductionSceneBuilder
    {
        [MenuItem("CarGame/★ Build Complete Game Scene (All Stages 1-8) ★", false, 1)]
        public static void BuildFullProductionGame()
        {
            if (!EditorUtility.DisplayDialog(
                "Build Full Game Scene",
                "This will build the complete production-ready 3D Driving Simulator scene with all systems (Car Physics, Touch HUD, Lights/Audio/FX, Sunrise City, AI Traffic, Missions, Fuel, Economy, and Menus).\n\nProceed?",
                "Yes, Build Scene",
                "Cancel"))
            {
                return;
            }

            Debug.Log("<color=cyan><b>=== [CarGame] Building Stage 1: Vehicle Physics & Track ===</b></color>");
            Stage1SetupUtility.GenerateStage1Prototype();

            Debug.Log("<color=cyan><b>=== [CarGame] Building Stage 2: Touch HUD Canvas ===</b></color>");
            Stage2HUDSetupUtility.GenerateHUD();

            Debug.Log("<color=cyan><b>=== [CarGame] Building Stage 3: Lights, Audio, Skidmarks & Damage ===</b></color>");
            Stage3FXSetupUtility.AttachFXToVehicle();

            Debug.Log("<color=cyan><b>=== [CarGame] Building Stage 4: City World, Apartment Blocks & Minimap ===</b></color>");
            Stage4CitySetupUtility.GenerateCityEnvironment();

            Debug.Log("<color=cyan><b>=== [CarGame] Building Stage 5: AI Traffic, Traffic Lights & Pedestrians ===</b></color>");
            Stage5AISetupUtility.GenerateTrafficSystem();

            Debug.Log("<color=cyan><b>=== [CarGame] Building Stage 6: Missions, Economy, Fuel & Traffic Scoring ===</b></color>");
            Stage6GameplaySetupUtility.SetupGameplaySystems();

            Debug.Log("<color=cyan><b>=== [CarGame] Building Stage 7: Pause & Settings Menus ===</b></color>");
            Stage7MenuSetupUtility.SetupPauseAndSettings();

            Debug.Log("<color=cyan><b>=== [CarGame] Building Stage 8: Mobile Performance Manager ===</b></color>");
            if (Object.FindObjectOfType<CarGame.Managers.MobilePerformanceManager>() == null)
            {
                GameObject perfObj = new GameObject("MobilePerformanceManager");
                perfObj.AddComponent<CarGame.Managers.MobilePerformanceManager>();
            }

            EditorUtility.DisplayDialog(
                "Production Scene Built!",
                "All 8 Stages have been successfully generated and configured in your current scene!\n\n" +
                "1. Press Ctrl+S (Cmd+S) to save scene as 'GameCity.unity'.\n" +
                "2. Press 'Play' in Unity to drive with Touch or Keyboard controls!\n" +
                "3. Check 'CarGame > Stage 8 > Configure Android Player Settings' for APK building.",
                "Awesome!"
            );

            Debug.Log("<color=green><b>★ [CarGame] FULL PRODUCTION GAME ASSEMBLED SUCCESSFULLY! ★</b></color>");
        }
    }
}
#endif
