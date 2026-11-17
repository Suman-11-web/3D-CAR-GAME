using System;
using System.Collections.Generic;

namespace CarGame.SaveSystem
{
    [Serializable]
    public class PlayerSaveData
    {
        public int money = 450;
        public string selectedCarID = "hatchback_01";
        public List<string> unlockedCarIDs = new List<string> { "hatchback_01" };

        // Upgrades (0 to 3)
        public int engineUpgradeLevel = 0;
        public int brakeUpgradeLevel = 0;
        public int tireUpgradeLevel = 0;
        public int selectedPaintIndex = 0;

        // Settings
        public int graphicsQualityLevel = 1; // 0: Low, 1: Medium, 2: High
        public float sfxVolume = 1.0f;
        public float musicVolume = 0.8f;
        public int controlType = 0; // 0: SteeringWheel, 1: TouchButtons, 2: TiltGyro
        public float steeringSensitivity = 1.0f;
        public bool useMph = false;
    }
}
