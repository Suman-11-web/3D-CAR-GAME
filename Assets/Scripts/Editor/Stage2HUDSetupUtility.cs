#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using CarGame.UI;
using CarGame.Vehicle;
using CarGame.InputSystem;
using CarGame.CameraSystem;

namespace CarGame.EditorTools
{
    public static class Stage2HUDSetupUtility
    {
        [MenuItem("CarGame/Stage 2/Generate Touch HUD Canvas", false, 20)]
        public static void GenerateHUD()
        {
            // 1. Ensure EventSystem exists in scene
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(esObj, "Create EventSystem");
            }

            // 2. Check for existing HUD Canvas or create new
            GameObject hudCanvasObj = GameObject.Find("HUD_Canvas");
            if (hudCanvasObj != null)
            {
                if (EditorUtility.DisplayDialog("HUD Canvas Exists", "HUD_Canvas already exists. Replace with new layout?", "Replace", "Cancel"))
                {
                    Undo.DestroyObjectImmediate(hudCanvasObj);
                }
                else return;
            }

            hudCanvasObj = new GameObject("HUD_Canvas");
            Undo.RegisterCreatedObjectUndo(hudCanvasObj, "Generate HUD Canvas");

            Canvas canvas = hudCanvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = hudCanvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            hudCanvasObj.AddComponent<GraphicRaycaster>();
            VehicleHUDController hudController = hudCanvasObj.AddComponent<VehicleHUDController>();

            // 3. Top-Left: Minimap & Fuel Bar
            GameObject topLeftRoot = CreateUIContainer(hudCanvasObj.transform, "Anchor_TopLeft", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(250f, -120f), new Vector2(300f, 220f));

            // Minimap circular placeholder
            GameObject minimapBg = CreateImage(topLeftRoot.transform, "Minimap_Frame", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(110f, -100f), new Vector2(180f, 180f), new Color(0.12f, 0.14f, 0.16f, 0.85f));
            CreateText(minimapBg.transform, "Label", "MAP", 22, Color.gray, TextAlignmentOptions.Center);

            // Fuel Bar
            GameObject fuelRoot = CreateUIContainer(topLeftRoot.transform, "FuelBar_Container", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(110f, -210f), new Vector2(180f, 32f));
            CreateImage(fuelRoot.transform, "Bg", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.15f, 0.15f, 0.15f, 0.8f));
            GameObject fuelFill = CreateImage(fuelRoot.transform, "Fill", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.2f, 0.8f, 0.4f, 0.9f));
            Image imgFuel = fuelFill.GetComponent<Image>();
            imgFuel.type = Image.Type.Filled;
            imgFuel.fillMethod = Image.FillMethod.Horizontal;
            imgFuel.fillAmount = 0.95f;
            TextMeshProUGUI txtFuel = CreateText(fuelRoot.transform, "Text", "95%", 18, Color.white, TextAlignmentOptions.Center);

            // Wrench (Repair) button
            GameObject btnWrench = CreateButton(topLeftRoot.transform, "Btn_WrenchRepair", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, -100f), new Vector2(50f, 50f), new Color(0.25f, 0.28f, 0.32f, 0.8f), "🔧");

            // 4. Top-Right: Speedometer Dial, Money Counter, Camera Button
            GameObject topRightRoot = CreateUIContainer(hudCanvasObj.transform, "Anchor_TopRight", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-220f, -140f), new Vector2(380f, 260f));

            // Money Counter (450$)
            GameObject moneyObj = CreateUIContainer(topRightRoot.transform, "MoneyContainer", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-120f, -30f), new Vector2(150f, 40f));
            TextMeshProUGUI txtMoney = CreateText(moneyObj.transform, "Text_Money", "450$", 26, new Color(1f, 0.85f, 0.2f), TextAlignmentOptions.Right);

            // Camera Switch Button
            GameObject btnCam = CreateButton(topRightRoot.transform, "Btn_CameraSwitch", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-270f, -110f), new Vector2(60f, 60f), new Color(0.2f, 0.22f, 0.26f, 0.85f), "📷");
            TouchButtonControl tbcCam = btnCam.AddComponent<TouchButtonControl>();

            // Speedometer Dial Container
            GameObject dialBg = CreateImage(topRightRoot.transform, "Speedometer_Dial", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-120f, -120f), new Vector2(210f, 210f), new Color(0.08f, 0.1f, 0.12f, 0.88f));

            // Analog Needle
            GameObject needleObj = CreateImage(dialBg.transform, "Needle", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(6f, 85f), new Color(1f, 0.2f, 0.2f, 1.0f));
            needleObj.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.05f);

            // Digital Speed Text
            TextMeshProUGUI txtSpeed = CreateText(dialBg.transform, "Text_DigitalSpeed", "0", 42, Color.white, TextAlignmentOptions.Center);
            txtSpeed.rectTransform.anchoredPosition = new Vector2(0f, -15f);

            // Speed Unit Label (km/h)
            TextMeshProUGUI txtUnit = CreateText(dialBg.transform, "Text_Unit", "km/h", 16, Color.gray, TextAlignmentOptions.Center);
            txtUnit.rectTransform.anchoredPosition = new Vector2(0f, -42f);

            // Gear Text
            TextMeshProUGUI txtGear = CreateText(dialBg.transform, "Text_Gear", "1", 28, new Color(0.3f, 0.85f, 1.0f), TextAlignmentOptions.Center);
            txtGear.rectTransform.anchoredPosition = new Vector2(0f, 25f);

            // TCS & ABS Icons
            GameObject tcsObj = CreateImage(topRightRoot.transform, "Icon_TCS", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-270f, -180f), new Vector2(40f, 40f), new Color(0.3f, 0.3f, 0.3f, 0.4f));
            CreateText(tcsObj.transform, "Label", "TC", 18, Color.yellow, TextAlignmentOptions.Center);
            Image imgTCS = tcsObj.GetComponent<Image>();

            // SpeedometerTachometerUI component setup
            SpeedometerTachometerUI speedoUI = dialBg.AddComponent<SpeedometerTachometerUI>();
            SerializedObject spSO = new SerializedObject(speedoUI);
            spSO.FindProperty("dialNeedle").objectReferenceValue = needleObj.GetComponent<RectTransform>();
            spSO.FindProperty("speedText").objectReferenceValue = txtSpeed;
            spSO.FindProperty("gearText").objectReferenceValue = txtGear;
            spSO.FindProperty("speedUnitText").objectReferenceValue = txtUnit;
            spSO.FindProperty("moneyText").objectReferenceValue = txtMoney;
            spSO.FindProperty("fuelBarFill").objectReferenceValue = imgFuel;
            spSO.FindProperty("fuelPercentText").objectReferenceValue = txtFuel;
            spSO.FindProperty("tcsIcon").objectReferenceValue = imgTCS;
            spSO.ApplyModifiedProperties();

            // 5. Left-Bottom: Steering Wheel & Bottom Indicators
            GameObject bottomLeftRoot = CreateUIContainer(hudCanvasObj.transform, "Anchor_BottomLeft", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(210f, 210f), new Vector2(360f, 360f));

            // Steering Wheel
            GameObject steerObj = CreateImage(bottomLeftRoot.transform, "SteeringWheel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(320f, 320f), new Color(0.2f, 0.22f, 0.25f, 0.95f));
            TouchSteeringWheel wheelComp = steerObj.AddComponent<TouchSteeringWheel>();

            // Center Horn Button
            GameObject hornObj = CreateImage(steerObj.transform, "HornCenter", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(110f, 110f), new Color(0.28f, 0.3f, 0.34f, 1.0f));
            CreateText(hornObj.transform, "HornIcon", "📯", 34, Color.white, TextAlignmentOptions.Center);

            // Bottom-Left Indicator Buttons (Hazard & Turn Signals)
            GameObject indRoot = CreateUIContainer(hudCanvasObj.transform, "Indicator_Buttons", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(210f, 40f), new Vector2(260f, 60f));
            GameObject btnTurnL = CreateButton(indRoot.transform, "Btn_TurnLeft", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-75f, 0f), new Vector2(55f, 50f), new Color(0.2f, 0.2f, 0.2f, 0.7f), "◀");
            GameObject btnHazard = CreateButton(indRoot.transform, "Btn_Hazard", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(55f, 50f), new Color(0.35f, 0.15f, 0.15f, 0.8f), "▲");
            GameObject btnTurnR = CreateButton(indRoot.transform, "Btn_TurnRight", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(75f, 0f), new Vector2(55f, 50f), new Color(0.2f, 0.2f, 0.2f, 0.7f), "▶");

            TouchButtonControl tbcTL = btnTurnL.AddComponent<TouchButtonControl>();
            TouchButtonControl tbcHz = btnHazard.AddComponent<TouchButtonControl>();
            TouchButtonControl tbcTR = btnTurnR.AddComponent<TouchButtonControl>();

            // 6. Right-Bottom: Accelerator & Brake Pedals
            GameObject bottomRightRoot = CreateUIContainer(hudCanvasObj.transform, "Anchor_BottomRight", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-190f, 160f), new Vector2(340f, 260f));

            // Brake Pedal (Left)
            GameObject brakeObj = CreateImage(bottomRightRoot.transform, "Pedal_Brake", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-70f, 0f), new Vector2(100f, 160f), new Color(0.32f, 0.33f, 0.36f, 0.95f));
            CreateText(brakeObj.transform, "Label", "BRAKE", 20, Color.white, TextAlignmentOptions.Center);
            TouchPedal brakePedal = brakeObj.AddComponent<TouchPedal>();
            SerializedObject brkSO = new SerializedObject(brakePedal);
            brkSO.FindProperty("pedalType").enumValueIndex = (int)PedalType.Brake;
            brkSO.ApplyModifiedProperties();

            // Accelerator Pedal (Right)
            GameObject accelObj = CreateImage(bottomRightRoot.transform, "Pedal_Accelerator", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(70f, 0f), new Vector2(90f, 210f), new Color(0.24f, 0.25f, 0.28f, 0.95f));
            CreateText(accelObj.transform, "Label", "GAS", 22, Color.white, TextAlignmentOptions.Center);
            TouchPedal accelPedal = accelObj.AddComponent<TouchPedal>();
            SerializedObject accSO = new SerializedObject(accelPedal);
            accSO.FindProperty("pedalType").enumValueIndex = (int)PedalType.Accelerator;
            accSO.ApplyModifiedProperties();

            // 7. Right-Middle: Vertical Gear Selector (P / R / N / D)
            GameObject gearRoot = CreateUIContainer(hudCanvasObj.transform, "Anchor_RightMiddle", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-70f, 40f), new Vector2(70f, 260f));
            CreateImage(gearRoot.transform, "Selector_Bg", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.1f, 0.12f, 0.14f, 0.85f));

            VerticalGearSelectorUI gearUI = gearRoot.AddComponent<VerticalGearSelectorUI>();

            GameObject btnD = CreateButton(gearRoot.transform, "Btn_D", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), new Vector2(50f, 50f), new Color(0.2f, 0.8f, 0.3f), "D");
            GameObject btnN = CreateButton(gearRoot.transform, "Btn_N", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(50f, 50f), new Color(0.3f, 0.3f, 0.3f), "N");
            GameObject btnR = CreateButton(gearRoot.transform, "Btn_R", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -30f), new Vector2(50f, 50f), new Color(0.3f, 0.3f, 0.3f), "R");
            GameObject btnP = CreateButton(gearRoot.transform, "Btn_P", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -90f), new Vector2(50f, 50f), new Color(0.3f, 0.3f, 0.3f), "P");

            SerializedObject grSO = new SerializedObject(gearUI);
            grSO.FindProperty("btnDrive").objectReferenceValue = btnD.GetComponent<Button>();
            grSO.FindProperty("btnNeutral").objectReferenceValue = btnN.GetComponent<Button>();
            grSO.FindProperty("btnReverse").objectReferenceValue = btnR.GetComponent<Button>();
            grSO.FindProperty("btnPark").objectReferenceValue = btnP.GetComponent<Button>();
            grSO.FindProperty("imgDrive").objectReferenceValue = btnD.GetComponent<Image>();
            grSO.FindProperty("imgNeutral").objectReferenceValue = btnN.GetComponent<Image>();
            grSO.FindProperty("imgReverse").objectReferenceValue = btnR.GetComponent<Image>();
            grSO.FindProperty("imgPark").objectReferenceValue = btnP.GetComponent<Image>();
            grSO.ApplyModifiedProperties();

            // Handbrake Button
            GameObject btnHb = CreateButton(bottomRightRoot.transform, "Btn_Handbrake", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-145f, -20f), new Vector2(60f, 60f), new Color(0.38f, 0.2f, 0.2f, 0.85f), "P! ");
            TouchButtonControl tbcHb = btnHb.AddComponent<TouchButtonControl>();

            // 8. Connect Everything to VehicleHUDController
            SerializedObject hudSO = new SerializedObject(hudController);
            hudSO.FindProperty("steeringWheel").objectReferenceValue = wheelComp;
            hudSO.FindProperty("acceleratorPedal").objectReferenceValue = accelPedal;
            hudSO.FindProperty("brakePedal").objectReferenceValue = brakePedal;
            hudSO.FindProperty("gearSelector").objectReferenceValue = gearUI;
            hudSO.FindProperty("btnHandbrake").objectReferenceValue = tbcHb;
            hudSO.FindProperty("btnCameraSwitch").objectReferenceValue = tbcCam;
            hudSO.FindProperty("btnHazard").objectReferenceValue = tbcHz;
            hudSO.FindProperty("btnTurnLeft").objectReferenceValue = tbcTL;
            hudSO.FindProperty("btnTurnRight").objectReferenceValue = tbcTR;
            hudSO.FindProperty("speedometerUI").objectReferenceValue = speedoUI;
            hudSO.ApplyModifiedProperties();

            Debug.Log("<color=green><b>[CarGame] Stage 2 Touch HUD Canvas Generated Successfully!</b></color> Exactly matches the reference HUD layout.");
        }

        private static GameObject CreateUIContainer(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
        {
            GameObject obj = new GameObject(name);
            obj.transform.parent = parent;
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return obj;
        }

        private static GameObject CreateImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Color color)
        {
            GameObject obj = CreateUIContainer(parent, name, anchorMin, anchorMax, pos, size);
            Image img = obj.AddComponent<Image>();
            img.color = color;
            return obj;
        }

        private static GameObject CreateButton(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Color color, string text)
        {
            GameObject obj = CreateImage(parent, name, anchorMin, anchorMax, pos, size, color);
            obj.AddComponent<Button>();
            if (!string.IsNullOrEmpty(text))
            {
                CreateText(obj.transform, "Label", text, 20, Color.white, TextAlignmentOptions.Center);
            }
            return obj;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string content, float fontSize, Color color, TextAlignmentOptions align)
        {
            GameObject obj = new GameObject(name);
            obj.transform.parent = parent;
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = align;
            return tmp;
        }
    }
}
#endif
