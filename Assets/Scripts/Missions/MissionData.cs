using UnityEngine;

namespace CarGame.Missions
{
    public enum MissionType
    {
        FreeRoam,
        Delivery,
        DriveToPoint,
        TimeTrial,
        ParkingChallenge
    }

    [CreateAssetMenu(fileName = "NewMission", menuName = "Car Game/Mission Data", order = 2)]
    public class MissionData : ScriptableObject
    {
        [Header("Mission Info")]
        public string missionID = "mission_01";
        public string title = "Morning Coffee Delivery";
        [TextArea] public string description = "Pick up coffee supplies and deliver them to Downtown Cafe before sunrise ends.";
        public MissionType missionType = MissionType.Delivery;

        [Header("Parameters")]
        public float timeLimitSeconds = 90f;
        public int rewardCash = 250;
        public int rewardXP = 100;

        [Header("Route Waypoint Positions")]
        public Vector3[] checkpointPositions;
    }
}
