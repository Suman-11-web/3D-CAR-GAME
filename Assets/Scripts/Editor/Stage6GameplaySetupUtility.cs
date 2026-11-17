#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using CarGame.Gameplay;
using CarGame.Economy;
using CarGame.Missions;
using CarGame.Vehicle;

namespace CarGame.EditorTools
{
    public static class Stage6GameplaySetupUtility
    {
        [MenuItem("CarGame/Stage 6/Setup Missions, Fuel, Economy & Traffic Rules", false, 60)]
        public static void SetupGameplaySystems()
        {
            // 1. Attach FuelSystem to Player Vehicle
            GameObject car = GameObject.FindWithTag("Player") ?? GameObject.Find("Player_Hatchback");
            if (car != null)
            {
                if (car.GetComponent<FuelSystem>() == null)
                {
                    car.AddComponent<FuelSystem>();
                    Debug.Log("[Stage6] FuelSystem attached to vehicle.");
                }
            }

            // 2. Setup Gameplay Manager object
            GameObject mgrObj = GameObject.Find("--- GAMEPLAY_SYSTEMS ---");
            if (mgrObj == null)
            {
                mgrObj = new GameObject("--- GAMEPLAY_SYSTEMS ---");
                Undo.RegisterCreatedObjectUndo(mgrObj, "Setup Gameplay Systems");
            }

            if (mgrObj.GetComponent<EconomyManager>() == null) mgrObj.AddComponent<EconomyManager>();
            if (mgrObj.GetComponent<TrafficRuleScoringSystem>() == null) mgrObj.AddComponent<TrafficRuleScoringSystem>();
            MissionManager missionMgr = mgrObj.GetComponent<MissionManager>();
            if (missionMgr == null) missionMgr = mgrObj.AddComponent<MissionManager>();

            // 3. Create Sample Delivery MissionData Asset
            MissionData mission = AssetDatabase.LoadAssetAtPath<MissionData>("Assets/Prefabs/Mission_MorningCoffee.asset");
            if (mission == null)
            {
                mission = ScriptableObject.CreateInstance<MissionData>();
                mission.missionID = "delivery_01";
                mission.title = "Sunrise City Delivery";
                mission.description = "Deliver urgent morning cargo down the boulevard to the turnaround station.";
                mission.timeLimitSeconds = 120f;
                mission.rewardCash = 350;
                mission.rewardXP = 150;
                mission.checkpointPositions = new Vector3[]
                {
                    new Vector3(3.5f, 0.5f, 150f),
                    new Vector3(3.5f, 0.5f, 320f),
                    new Vector3(0.0f, 0.5f, 540f)
                };

                AssetDatabase.CreateAsset(mission, "Assets/Prefabs/Mission_MorningCoffee.asset");
                AssetDatabase.SaveAssets();
            }

            Debug.Log("<color=green><b>[CarGame] Stage 6 Missions, Fuel, Economy & Traffic Scoring Configured!</b></color>");
        }
    }
}
#endif
