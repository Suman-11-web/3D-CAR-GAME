using System;
using UnityEngine;
using CarGame.Data;
using CarGame.Economy;
using CarGame.SaveSystem;

namespace CarGame.Garage
{
    /// <summary>
    /// Garage and Showroom manager.
    /// Drives the 3D rotating turntable, car selection, paint color changes,
    /// and mechanical performance upgrades (Engine, Brakes, Tires).
    /// </summary>
    public class GarageManager : MonoBehaviour
    {
        public static GarageManager Instance { get; private set; }

        [Header("3D Turntable")]
        [SerializeField] private Transform turntableTransform;
        [SerializeField] private float turntableRotationSpeed = 16f;

        [Header("Showcase Vehicle")]
        [SerializeField] private CarData[] availableCars;
        [SerializeField] private Renderer carBodyRenderer;
        [SerializeField] private Color[] paintPalette = new Color[]
        {
            new Color(0.48f, 0.08f, 0.12f), // Metallic Burgundy (Screenshot)
            new Color(0.12f, 0.35f, 0.72f), // Royal Blue
            new Color(0.15f, 0.15f, 0.16f), // Midnight Black
            new Color(0.85f, 0.86f, 0.88f), // Glacier Silver
            new Color(0.95f, 0.75f, 0.10f)  // Sunset Yellow
        };

        [Header("Upgrade Costs & Scaling")]
        [SerializeField] private int baseUpgradeCost = 200;

        private int currentCarIndex = 0;
        private int currentPaintIndex = 0;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            LoadSavedGarageState();
        }

        private void Update()
        {
            if (turntableTransform != null)
            {
                turntableTransform.Rotate(Vector3.up * turntableRotationSpeed * Time.deltaTime);
            }
        }

        private void LoadSavedGarageState()
        {
            if (SaveManager.Instance != null)
            {
                currentPaintIndex = SaveManager.Instance.Data.selectedPaintIndex;
                ApplyPaintColor(currentPaintIndex);
            }
        }

        public void ApplyPaintColor(int colorIndex)
        {
            if (paintPalette == null || paintPalette.Length == 0) return;
            currentPaintIndex = Mathf.Clamp(colorIndex, 0, paintPalette.Length - 1);
            Color chosenColor = paintPalette[currentPaintIndex];

            if (carBodyRenderer != null && carBodyRenderer.material != null)
            {
                if (carBodyRenderer.material.HasProperty("_BaseColor"))
                {
                    carBodyRenderer.material.SetColor("_BaseColor", chosenColor);
                }
                else if (carBodyRenderer.material.HasProperty("_Color"))
                {
                    carBodyRenderer.material.SetColor("_Color", chosenColor);
                }
            }

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.Data.selectedPaintIndex = currentPaintIndex;
                SaveManager.Instance.SaveGame();
            }
        }

        public bool UpgradeEngine()
        {
            return TryPerformUpgrade(ref SaveManager.Instance.Data.engineUpgradeLevel);
        }

        public bool UpgradeBrakes()
        {
            return TryPerformUpgrade(ref SaveManager.Instance.Data.brakeUpgradeLevel);
        }

        public bool UpgradeTires()
        {
            return TryPerformUpgrade(ref SaveManager.Instance.Data.tireUpgradeLevel);
        }

        private bool TryPerformUpgrade(ref int currentLevel)
        {
            if (currentLevel >= 3) return false; // Max level reached

            int cost = baseUpgradeCost * (currentLevel + 1);
            if (EconomyManager.Instance != null && EconomyManager.Instance.SpendMoney(cost))
            {
                currentLevel++;
                SaveManager.Instance.SaveGame();
                Debug.Log($"[GarageManager] Upgraded to level {currentLevel}! Spent ${cost}");
                return true;
            }

            Debug.LogWarning("[GarageManager] Not enough cash for upgrade!");
            return false;
        }
    }
}
