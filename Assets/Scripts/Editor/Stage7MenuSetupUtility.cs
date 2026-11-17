#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CarGame.Menu;
using CarGame.SaveSystem;

namespace CarGame.EditorTools
{
    public static class Stage7MenuSetupUtility
    {
        [MenuItem("CarGame/Stage 7/Setup In-Game Pause and Settings UI", false, 70)]
        public static void SetupPauseAndSettings()
        {
            // 1. Ensure SaveManager exists in scene
            if (Object.FindObjectOfType<SaveManager>() == null)
            {
                GameObject saveObj = new GameObject("SaveManager");
                saveObj.AddComponent<SaveManager>();
                Undo.RegisterCreatedObjectUndo(saveObj, "Create SaveManager");
            }

            // 2. Find HUD Canvas
            GameObject canvasObj = GameObject.Find("HUD_Canvas");
            if (canvasObj == null)
            {
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("Error", "HUD_Canvas not found. Please run Stage 2 setup first!", "OK");
                }
                return;
            }

            // 3. Pause Menu Overlay Panel
            Transform existingPause = canvasObj.transform.Find("Panel_PauseMenu");
            if (existingPause != null) Object.DestroyImmediate(existingPause.gameObject);

            GameObject pausePanel = new GameObject("Panel_PauseMenu");
            pausePanel.transform.parent = canvasObj.transform;
            RectTransform pRT = pausePanel.AddComponent<RectTransform>();
            pRT.anchorMin = Vector2.zero;
            pRT.anchorMax = Vector2.one;
            pRT.offsetMin = Vector2.zero;
            pRT.offsetMax = Vector2.zero;
            Image pImg = pausePanel.AddComponent<Image>();
            pImg.color = new Color(0f, 0f, 0f, 0.75f);

            PauseMenuController pauseCtrl = pausePanel.AddComponent<PauseMenuController>();

            // Title
            CreateText(pausePanel.transform, "Title", "PAUSED", 48, Color.white, new Vector2(0f, 180f), new Vector2(400f, 70f));

            // Buttons
            GameObject btnResume = CreateMenuButton(pausePanel.transform, "Btn_Resume", "RESUME", new Vector2(0f, 60f));
            GameObject btnRestart = CreateMenuButton(pausePanel.transform, "Btn_Restart", "RESTART", new Vector2(0f, -20f));
            GameObject btnSettings = CreateMenuButton(pausePanel.transform, "Btn_Settings", "SETTINGS", new Vector2(0f, -100f));
            GameObject btnMainMenu = CreateMenuButton(pausePanel.transform, "Btn_MainMenu", "MAIN MENU", new Vector2(0f, -180f));

            // Hook listeners
            btnResume.GetComponent<Button>().onClick.AddListener(pauseCtrl.ResumeGame);
            btnRestart.GetComponent<Button>().onClick.AddListener(pauseCtrl.RestartTrack);
            btnSettings.GetComponent<Button>().onClick.AddListener(pauseCtrl.OpenSettings);
            btnMainMenu.GetComponent<Button>().onClick.AddListener(pauseCtrl.ReturnToMainMenu);

            // 4. Settings Panel
            GameObject settingsPanel = new GameObject("Panel_Settings");
            settingsPanel.transform.parent = canvasObj.transform;
            RectTransform sRT = settingsPanel.AddComponent<RectTransform>();
            sRT.anchorMin = Vector2.zero;
            sRT.anchorMax = Vector2.one;
            sRT.offsetMin = Vector2.zero;
            sRT.offsetMax = Vector2.zero;
            Image sImg = settingsPanel.AddComponent<Image>();
            sImg.color = new Color(0.08f, 0.1f, 0.12f, 0.95f);

            SettingsMenuController settingsCtrl = settingsPanel.AddComponent<SettingsMenuController>();

            CreateText(settingsPanel.transform, "Title", "SETTINGS", 44, Color.white, new Vector2(0f, 220f), new Vector2(400f, 60f));

            GameObject btnCloseSettings = CreateMenuButton(settingsPanel.transform, "Btn_CloseSettings", "BACK", new Vector2(0f, -240f));
            btnCloseSettings.GetComponent<Button>().onClick.AddListener(pauseCtrl.CloseSettings);

            // Connect references
            SerializedObject pauseSO = new SerializedObject(pauseCtrl);
            pauseSO.FindProperty("pausePanel").objectReferenceValue = pausePanel;
            pauseSO.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
            pauseSO.ApplyModifiedProperties();

            // Default hidden
            pausePanel.SetActive(false);
            settingsPanel.SetActive(false);

            Debug.Log("<color=green><b>[CarGame] Stage 7 Pause & Settings Menus Configured!</b></color>");
        }

        private static GameObject CreateMenuButton(Transform parent, string name, string label, Vector2 anchoredPos)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.parent = parent;
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(260f, 55f);

            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.22f, 0.25f, 0.3f, 0.9f);

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(0.35f, 0.4f, 0.48f);
            cb.pressedColor = new Color(0.15f, 0.18f, 0.22f);
            btn.colors = cb;

            CreateText(btnObj.transform, "Label", label, 24, Color.white, Vector2.zero, rt.sizeDelta);
            return btnObj;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string text, float fontSize, Color color, Vector2 pos, Vector2 size)
        {
            GameObject txtObj = new GameObject(name);
            txtObj.transform.parent = parent;
            RectTransform rt = txtObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            return tmp;
        }
    }
}
#endif
