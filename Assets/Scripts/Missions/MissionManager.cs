using System;
using System.Collections.Generic;
using UnityEngine;
using CarGame.Economy;

namespace CarGame.Missions
{
    /// <summary>
    /// Coordinates mission flow: delivery routes, drive-to-point checkpoints,
    /// countdown timers, parking triggers, and cash/XP reward payouts.
    /// </summary>
    public class MissionManager : MonoBehaviour
    {
        public static MissionManager Instance { get; private set; }

        [Header("Active Mission")]
        [SerializeField] private MissionData activeMission;
        [SerializeField] private int currentCheckpointIndex;
        [SerializeField] private float timeRemaining;
        [SerializeField] private bool isMissionActive;

        [Header("Visual Objective Marker")]
        [SerializeField] private GameObject checkpointMarkerPrefab;
        private GameObject activeMarkerInstance;

        public event Action<MissionData> OnMissionStarted;
        public event Action<bool, int> OnMissionEnded; // success, reward

        public bool IsMissionActive => isMissionActive;
        public float TimeRemaining => timeRemaining;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Update()
        {
            if (!isMissionActive || activeMission == null) return;

            // Timer countdown
            if (activeMission.timeLimitSeconds > 0f)
            {
                timeRemaining -= Time.deltaTime;
                if (timeRemaining <= 0f)
                {
                    FailMission("Time Expired!");
                }
            }
        }

        public void StartMission(MissionData mission)
        {
            if (mission == null) return;
            activeMission = mission;
            currentCheckpointIndex = 0;
            timeRemaining = mission.timeLimitSeconds;
            isMissionActive = true;

            SpawnCheckpointMarker();
            OnMissionStarted?.Invoke(mission);
            Debug.Log($"[MissionManager] Started mission: {mission.title}");
        }

        public void ReachCheckpoint(int checkpointIdx)
        {
            if (!isMissionActive || checkpointIdx != currentCheckpointIndex) return;

            currentCheckpointIndex++;
            if (activeMission.checkpointPositions != null && currentCheckpointIndex < activeMission.checkpointPositions.Length)
            {
                SpawnCheckpointMarker();
            }
            else
            {
                CompleteMission();
            }
        }

        private void SpawnCheckpointMarker()
        {
            if (activeMission.checkpointPositions == null || currentCheckpointIndex >= activeMission.checkpointPositions.Length) return;

            if (activeMarkerInstance == null)
            {
                activeMarkerInstance = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                activeMarkerInstance.name = "ActiveCheckpointMarker";
                activeMarkerInstance.transform.localScale = new Vector3(4.5f, 0.2f, 4.5f);
                var col = activeMarkerInstance.GetComponent<Collider>();
                col.isTrigger = true;

                CheckpointTrigger trigger = activeMarkerInstance.AddComponent<CheckpointTrigger>();
                trigger.Setup(this);
            }

            activeMarkerInstance.transform.position = activeMission.checkpointPositions[currentCheckpointIndex];
            activeMarkerInstance.SetActive(true);
        }

        private void CompleteMission()
        {
            isMissionActive = false;
            if (activeMarkerInstance != null) activeMarkerInstance.SetActive(false);

            int cash = activeMission != null ? activeMission.rewardCash : 150;
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddMoney(cash);
            }

            OnMissionEnded?.Invoke(true, cash);
            Debug.Log($"<color=green>[MissionManager] Mission Completed! Earned ${cash}</color>");
        }

        public void FailMission(string reason)
        {
            isMissionActive = false;
            if (activeMarkerInstance != null) activeMarkerInstance.SetActive(false);
            OnMissionEnded?.Invoke(false, 0);
            Debug.LogWarning($"[MissionManager] Mission Failed: {reason}");
        }
    }

    public class CheckpointTrigger : MonoBehaviour
    {
        private MissionManager manager;

        public void Setup(MissionManager mgr)
        {
            manager = mgr;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") || other.GetComponentInParent<CarGame.Vehicle.VehicleController>() != null)
            {
                if (manager != null)
                {
                    manager.ReachCheckpoint(0); // advances
                }
            }
        }
    }
}
