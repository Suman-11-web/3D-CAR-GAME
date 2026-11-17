using System.IO;
using UnityEngine;

namespace CarGame.SaveSystem
{
    /// <summary>
    /// Persistent JSON save and load manager.
    /// Operates on Application.persistentDataPath for cross-platform Android/Desktop persistence.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private string saveFilePath;
        private PlayerSaveData currentSaveData;

        public PlayerSaveData Data => currentSaveData ?? (currentSaveData = new PlayerSaveData());

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
                LoadGame();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SaveGame()
        {
            try
            {
                string json = JsonUtility.ToJson(Data, true);
                File.WriteAllText(saveFilePath, json);
                Debug.Log($"[SaveManager] Game saved to: {saveFilePath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveManager] Failed to save game: {ex.Message}");
            }
        }

        public void LoadGame()
        {
            try
            {
                if (File.Exists(saveFilePath))
                {
                    string json = File.ReadAllText(saveFilePath);
                    currentSaveData = JsonUtility.FromJson<PlayerSaveData>(json);
                    Debug.Log("[SaveManager] Game loaded successfully.");
                }
                else
                {
                    currentSaveData = new PlayerSaveData();
                    SaveGame();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[SaveManager] Error loading save, generating defaults: {ex.Message}");
                currentSaveData = new PlayerSaveData();
            }
        }

        public void ResetSave()
        {
            currentSaveData = new PlayerSaveData();
            SaveGame();
        }
    }
}
