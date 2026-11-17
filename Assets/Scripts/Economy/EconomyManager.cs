using System;
using UnityEngine;
using CarGame.UI;

namespace CarGame.Economy
{
    /// <summary>
    /// Manages player cash balance, purchases, mission rewards, and traffic fine deductions.
    /// </summary>
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        [Header("Starting Balance")]
        [SerializeField] private int playerBalance = 450; // Matches reference screenshot "450$"

        public event Action<int> OnBalanceChanged;

        public int CurrentBalance => playerBalance;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            UpdateHUD();
        }

        public void AddMoney(int amount)
        {
            if (amount <= 0) return;
            playerBalance += amount;
            UpdateHUD();
            OnBalanceChanged?.Invoke(playerBalance);
        }

        public bool SpendMoney(int amount)
        {
            if (amount <= 0) return true;
            if (playerBalance >= amount)
            {
                playerBalance -= amount;
                UpdateHUD();
                OnBalanceChanged?.Invoke(playerBalance);
                return true;
            }
            return false;
        }

        public void DeductFine(int fineAmount)
        {
            playerBalance = Mathf.Max(playerBalance - fineAmount, 0);
            UpdateHUD();
            OnBalanceChanged?.Invoke(playerBalance);
        }

        public void SetBalanceDirect(int newBalance)
        {
            playerBalance = Mathf.Max(newBalance, 0);
            UpdateHUD();
            OnBalanceChanged?.Invoke(playerBalance);
        }

        private void UpdateHUD()
        {
            SpeedometerTachometerUI speedo = FindObjectOfType<SpeedometerTachometerUI>();
            if (speedo != null)
            {
                speedo.SetMoney(playerBalance);
            }
        }
    }
}
