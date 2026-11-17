using UnityEngine;
using UnityEngine.SceneManagement;

namespace CarGame.Menu
{
    /// <summary>
    /// Master Main Menu navigation controller:
    /// Handles Play game, Garage showroom, Missions, Settings dialogs, and Quit.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Menu Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject garagePanel;
        [SerializeField] private GameObject missionsPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Scene Loading")]
        [SerializeField] private string gameCitySceneName = "GameCity";

        private void Start()
        {
            ShowMainPanel();
        }

        public void PlayGame()
        {
            SceneManager.LoadScene(gameCitySceneName);
        }

        public void OpenGarage()
        {
            SetPanel(garagePanel);
        }

        public void OpenMissions()
        {
            SetPanel(missionsPanel);
        }

        public void OpenSettings()
        {
            SetPanel(settingsPanel);
        }

        public void ShowMainPanel()
        {
            SetPanel(mainPanel);
        }

        public void QuitGame()
        {
            Debug.Log("[MainMenu] Quitting game...");
            Application.Quit();
        }

        private void SetPanel(GameObject target)
        {
            if (mainPanel != null) mainPanel.SetActive(target == mainPanel);
            if (garagePanel != null) garagePanel.SetActive(target == garagePanel);
            if (missionsPanel != null) missionsPanel.SetActive(target == missionsPanel);
            if (settingsPanel != null) settingsPanel.SetActive(target == settingsPanel);
        }
    }
}
