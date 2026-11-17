#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CarGame.Vehicle;
using CarGame.Data;
using CarGame.CameraSystem;
using CarGame.InputSystem;
using CarGame.UI;

namespace CarGame.EditorTools
{
    public static class Stage1SetupUtility
    {
        [MenuItem("CarGame/Stage 1/Generate Test Scene and Car Prototype", false, 10)]
        public static void GenerateStage1Prototype()
        {
            // 1. Ensure required directories exist
            EnsureFolderExists("Assets/Materials");
            EnsureFolderExists("Assets/Prefabs");
            EnsureFolderExists("Assets/Scenes");

            // 2. Create or Load Default CarData Asset
            CarData carData = AssetDatabase.LoadAssetAtPath<CarData>("Assets/Prefabs/Hatchback_CarData.asset");
            if (carData == null)
            {
                carData = ScriptableObject.CreateInstance<CarData>();
                AssetDatabase.CreateAsset(carData, "Assets/Prefabs/Hatchback_CarData.asset");
                AssetDatabase.SaveAssets();
                Debug.Log("[Stage1Setup] Created new CarData asset at Assets/Prefabs/Hatchback_CarData.asset");
            }

            // 3. Create Materials
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");

            Material carBodyMat = GetOrCreateMaterial("Assets/Materials/Mat_CarBody_Burgundy.mat", litShader, new Color(0.48f, 0.08f, 0.12f), 0.88f, 0.5f);
            Material carCabinMat = GetOrCreateMaterial("Assets/Materials/Mat_CarCabin_Glass.mat", litShader, new Color(0.1f, 0.12f, 0.15f), 0.95f, 0.1f);
            Material wheelTireMat = GetOrCreateMaterial("Assets/Materials/Mat_WheelTire.mat", litShader, new Color(0.12f, 0.12f, 0.12f), 0.25f, 0.0f);
            Material wheelRimMat = GetOrCreateMaterial("Assets/Materials/Mat_WheelRim_Silver.mat", litShader, new Color(0.75f, 0.75f, 0.75f), 0.85f, 0.7f);
            Material asphaltMat = GetOrCreateMaterial("Assets/Materials/Mat_Asphalt_WetRoad.mat", litShader, new Color(0.18f, 0.19f, 0.20f), 0.82f, 0.15f);
            Material roadLineMat = GetOrCreateMaterial("Assets/Materials/Mat_RoadMarking_White.mat", litShader, new Color(0.95f, 0.95f, 0.9f), 0.3f, 0.0f);
            Material barrierMat = GetOrCreateMaterial("Assets/Materials/Mat_RoadBarrier.mat", litShader, new Color(0.55f, 0.25f, 0.15f), 0.4f, 0.2f);

            // 4. Create Track Environment
            GameObject envRoot = new GameObject("--- ENVIRONMENT ---");
            Undo.RegisterCreatedObjectUndo(envRoot, "Generate Environment");

            // Directional Light (Golden Sunrise match)
            GameObject sunObj = new GameObject("Directional Light (Sunrise)");
            sunObj.transform.parent = envRoot.transform;
            sunObj.transform.rotation = Quaternion.Euler(14f, 15f, 0f);
            Light sunLight = sunObj.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.color = new Color(1.0f, 0.86f, 0.72f);
            sunLight.intensity = 1.35f;
            sunLight.shadows = LightShadows.Soft;

            // Road Track (500 meters straight road with turnaround loop)
            GameObject roadTrack = new GameObject("TestRoad_WetAsphalt");
            roadTrack.transform.parent = envRoot.transform;

            // Main Straightway
            CreateRoadSection(roadTrack.transform, "MainHighway_500m", new Vector3(0f, 0f, 250f), new Vector3(14f, 0.2f, 600f), asphaltMat);
            
            // White Center and Edge Lines
            CreateRoadMarkings(roadTrack.transform, 600f, roadLineMat);

            // Side Curbs / Barriers
            CreateBoxBarrier(roadTrack.transform, "Barrier_Left", new Vector3(-7.2f, 0.45f, 250f), new Vector3(0.5f, 0.7f, 600f), barrierMat);
            CreateBoxBarrier(roadTrack.transform, "Barrier_Right", new Vector3(7.2f, 0.45f, 250f), new Vector3(0.5f, 0.7f, 600f), barrierMat);

            // Turnaround circular test area
            CreateRoadSection(roadTrack.transform, "Turnaround_Pad", new Vector3(0f, 0f, 550f), new Vector3(60f, 0.2f, 60f), asphaltMat);

            // 5. Build Prototype Hatchback Car
            GameObject carRoot = new GameObject("Player_Hatchback");
            Undo.RegisterCreatedObjectUndo(carRoot, "Generate Hatchback");
            carRoot.transform.position = new Vector3(0f, 0.5f, 10f);
            carRoot.tag = "Player";

            Rigidbody carRb = carRoot.AddComponent<Rigidbody>();
            VehicleInputManager inputMgr = carRoot.AddComponent<VehicleInputManager>();
            VehicleController controller = carRoot.AddComponent<VehicleController>();

            // Setup SerializedObject to hook properties cleanly
            SerializedObject ctrlSO = new SerializedObject(controller);
            ctrlSO.FindProperty("carData").objectReferenceValue = carData;

            // Create Visual Body
            GameObject bodyRoot = new GameObject("Body_Visual");
            bodyRoot.transform.parent = carRoot.transform;
            bodyRoot.transform.localPosition = Vector3.zero;

            // Lower Chassis (Red Burgundy)
            GameObject lowerChassis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lowerChassis.name = "LowerChassis";
            lowerChassis.transform.parent = bodyRoot.transform;
            lowerChassis.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            lowerChassis.transform.localScale = new Vector3(1.82f, 0.55f, 4.15f);
            lowerChassis.GetComponent<Renderer>().sharedMaterial = carBodyMat;
            Object.DestroyImmediate(lowerChassis.GetComponent<Collider>());

            // Cabin / Roof (Dark Tinted Glass)
            GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabin.name = "Cabin_Roof";
            cabin.transform.parent = bodyRoot.transform;
            cabin.transform.localPosition = new Vector3(0f, 1.10f, -0.22f);
            cabin.transform.localScale = new Vector3(1.55f, 0.58f, 2.25f);
            cabin.GetComponent<Renderer>().sharedMaterial = carCabinMat;
            Object.DestroyImmediate(cabin.GetComponent<Collider>());

            // Front Bonnet / Hood Slope
            GameObject hood = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hood.name = "Hood_Front";
            hood.transform.parent = bodyRoot.transform;
            hood.transform.localPosition = new Vector3(0f, 0.62f, 1.35f);
            hood.transform.localScale = new Vector3(1.72f, 0.42f, 1.25f);
            hood.GetComponent<Renderer>().sharedMaterial = carBodyMat;
            Object.DestroyImmediate(hood.GetComponent<Collider>());

            // Main Car Box Collider (Elevated above wheels to allow full suspension travel)
            BoxCollider carCol = carRoot.AddComponent<BoxCollider>();
            carCol.center = new Vector3(0f, 0.85f, 0f);
            carCol.size = new Vector3(1.82f, 1.05f, 4.15f);

            // 6. Build Wheels & WheelColliders
            GameObject wheelsRoot = new GameObject("Wheels");
            wheelsRoot.transform.parent = carRoot.transform;
            wheelsRoot.transform.localPosition = Vector3.zero;

            float trackWidth = 0.88f;  // Half-width
            float wheelbase = 1.32f;   // Half-length
            float wheelY = 0.35f;

            WheelSetupRefs flRefs = CreateWheelAssembly(wheelsRoot.transform, "FrontLeft", new Vector3(-trackWidth, wheelY, wheelbase), carData, wheelTireMat, wheelRimMat);
            WheelSetupRefs frRefs = CreateWheelAssembly(wheelsRoot.transform, "FrontRight", new Vector3(trackWidth, wheelY, wheelbase), carData, wheelTireMat, wheelRimMat);
            WheelSetupRefs rlRefs = CreateWheelAssembly(wheelsRoot.transform, "RearLeft", new Vector3(-trackWidth, wheelY, -wheelbase), carData, wheelTireMat, wheelRimMat);
            WheelSetupRefs rrRefs = CreateWheelAssembly(wheelsRoot.transform, "RearRight", new Vector3(trackWidth, wheelY, -wheelbase), carData, wheelTireMat, wheelRimMat);

            // Assign wheel serialized properties
            AssignWheelProperty(ctrlSO.FindProperty("frontLeftWheel"), flRefs.collider, flRefs.visual, true, true, true, false);
            AssignWheelProperty(ctrlSO.FindProperty("frontRightWheel"), frRefs.collider, frRefs.visual, true, true, true, false);
            AssignWheelProperty(ctrlSO.FindProperty("rearLeftWheel"), rlRefs.collider, rlRefs.visual, false, false, true, true);
            AssignWheelProperty(ctrlSO.FindProperty("rearRightWheel"), rrRefs.collider, rrRefs.visual, false, false, true, true);
            ctrlSO.ApplyModifiedProperties();

            // 7. Setup Camera
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
                camObj.AddComponent<AudioListener>();
            }

            CameraController camController = mainCam.GetComponent<CameraController>();
            if (camController == null)
            {
                camController = mainCam.gameObject.AddComponent<CameraController>();
            }

            SerializedObject camSO = new SerializedObject(camController);
            camSO.FindProperty("targetVehicle").objectReferenceValue = controller;
            camSO.ApplyModifiedProperties();

            camController.SnapToTarget();

            // 8. Add Stage 1 Debug Telemetry Overlay to Camera
            if (mainCam.GetComponent<Stage1DebugOverlay>() == null)
            {
                mainCam.gameObject.AddComponent<Stage1DebugOverlay>();
            }

            // Mark Scene Dirty
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = carRoot;

            Debug.Log("<color=green><b>[CarGame] Stage 1 Setup Complete!</b></color> Drivable prototype hatchback, test track, and chase camera successfully assembled. Press <b>Play</b> to drive!");
        }

        private struct WheelSetupRefs
        {
            public WheelCollider collider;
            public Transform visual;
        }

        private static WheelSetupRefs CreateWheelAssembly(Transform parent, string name, Vector3 localPos, CarData data, Material tireMat, Material rimMat)
        {
            // Root Wheel Transform
            GameObject wheelObj = new GameObject("Wheel_" + name);
            wheelObj.transform.parent = parent;
            wheelObj.transform.localPosition = localPos;

            // Wheel Collider
            WheelCollider wc = wheelObj.AddComponent<WheelCollider>();
            wc.radius = data.wheelRadius;
            wc.suspensionDistance = data.wheelSuspensionDistance;

            JointSpring spr = wc.suspensionSpring;
            spr.spring = data.suspensionSpring;
            spr.damper = data.suspensionDamper;
            spr.targetPosition = data.suspensionTargetPosition;
            wc.suspensionSpring = spr;

            // Visual Mesh Holder (Updated every frame by WheelUnit)
            GameObject visualHolder = new GameObject(name + "_Visual");
            visualHolder.transform.parent = wheelObj.transform;
            visualHolder.transform.localPosition = Vector3.zero;

            // Tire Cylinder
            GameObject tireMesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tireMesh.name = "Tire";
            tireMesh.transform.parent = visualHolder.transform;
            tireMesh.transform.localPosition = Vector3.zero;
            tireMesh.transform.localRotation = Quaternion.Euler(0f, 0f, 90f); // Lay cylinder on side
            tireMesh.transform.localScale = new Vector3(data.wheelRadius * 2f, 0.12f, data.wheelRadius * 2f);
            tireMesh.GetComponent<Renderer>().sharedMaterial = tireMat;
            Object.DestroyImmediate(tireMesh.GetComponent<Collider>());

            // Rim Cylinder (Slightly smaller, metallic center)
            GameObject rimMesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rimMesh.name = "Rim";
            rimMesh.transform.parent = visualHolder.transform;
            rimMesh.transform.localPosition = Vector3.zero;
            rimMesh.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            rimMesh.transform.localScale = new Vector3(data.wheelRadius * 1.35f, 0.125f, data.wheelRadius * 1.35f);
            rimMesh.GetComponent<Renderer>().sharedMaterial = rimMat;
            Object.DestroyImmediate(rimMesh.GetComponent<Collider>());

            return new WheelSetupRefs { collider = wc, visual = visualHolder.transform };
        }

        private static void AssignWheelProperty(SerializedProperty prop, WheelCollider wc, Transform visual, bool steer, bool motor, bool brakes, bool handbrake)
        {
            prop.FindPropertyRelative("wheelCollider").objectReferenceValue = wc;
            prop.FindPropertyRelative("wheelVisualMesh").objectReferenceValue = visual;
            prop.FindPropertyRelative("isSteerable").boolValue = steer;
            prop.FindPropertyRelative("isMotor").boolValue = motor;
            prop.FindPropertyRelative("hasBrakes").boolValue = brakes;
            prop.FindPropertyRelative("hasHandbrake").boolValue = handbrake;
            prop.FindPropertyRelative("visualRotationOffset").vector3Value = Vector3.zero;
        }

        private static void CreateRoadSection(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
        {
            GameObject roadObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roadObj.name = name;
            roadObj.transform.parent = parent;
            roadObj.transform.position = pos;
            roadObj.transform.localScale = size;
            roadObj.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void CreateBoxBarrier(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
        {
            GameObject barrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrier.name = name;
            barrier.transform.parent = parent;
            barrier.transform.position = pos;
            barrier.transform.localScale = size;
            barrier.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void CreateRoadMarkings(Transform parent, float roadLength, Material lineMat)
        {
            GameObject markingsRoot = new GameObject("RoadMarkings");
            markingsRoot.transform.parent = parent;

            // Center Dashed Yellow/White Line
            float dashLength = 4f;
            float gapLength = 4f;
            float step = dashLength + gapLength;

            for (float z = 0f; z < roadLength; z += step)
            {
                GameObject dash = GameObject.CreatePrimitive(PrimitiveType.Cube);
                dash.name = $"Dash_{z}";
                dash.transform.parent = markingsRoot.transform;
                dash.transform.position = new Vector3(0f, 0.11f, z);
                dash.transform.localScale = new Vector3(0.2f, 0.05f, dashLength);
                dash.GetComponent<Renderer>().sharedMaterial = lineMat;
                Object.DestroyImmediate(dash.GetComponent<Collider>());
            }
        }

        private static Material GetOrCreateMaterial(string path, Shader shader, Color color, float smoothness, float metallic)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                else if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);

                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
                else if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);

                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);

                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        private static void EnsureFolderExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string[] parts = path.Split('/');
                string current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                    {
                        AssetDatabase.CreateFolder(current, parts[i]);
                    }
                    current = next;
                }
            }
        }
    }
}
#endif
