#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using CarGame.Environment;

namespace CarGame.EditorTools
{
    public static class Stage4CitySetupUtility
    {
        [MenuItem("CarGame/Stage 4/Generate Realistic City Environment & Minimap", false, 40)]
        public static void GenerateCityEnvironment()
        {
            GameObject cityRoot = GameObject.Find("--- CITY_WORLD ---");
            if (cityRoot != null)
            {
                if (!Application.isBatchMode)
                {
                    if (EditorUtility.DisplayDialog("City Exists", "--- CITY_WORLD --- already exists. Rebuild?", "Rebuild", "Cancel"))
                    {
                        Undo.DestroyObjectImmediate(cityRoot);
                    }
                    else return;
                }
                else
                {
                    Undo.DestroyObjectImmediate(cityRoot);
                }
            }

            cityRoot = new GameObject("--- CITY_WORLD ---");
            Undo.RegisterCreatedObjectUndo(cityRoot, "Generate City World");

            // Load or create materials
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material buildingMat1 = GetOrCreateMaterial("Assets/Materials/Mat_Building_Beige.mat", litShader, new Color(0.85f, 0.82f, 0.75f), 0.2f, 0.0f);
            Material buildingMat2 = GetOrCreateMaterial("Assets/Materials/Mat_Building_Concrete.mat", litShader, new Color(0.72f, 0.74f, 0.76f), 0.25f, 0.0f);
            Material windowMat = GetOrCreateMaterial("Assets/Materials/Mat_Building_Windows.mat", litShader, new Color(0.18f, 0.25f, 0.35f), 0.95f, 0.3f);
            Material hedgeMat = GetOrCreateMaterial("Assets/Materials/Mat_Median_Hedge.mat", litShader, new Color(0.15f, 0.42f, 0.18f), 0.2f, 0.0f);
            Material sidewalkMat = GetOrCreateMaterial("Assets/Materials/Mat_Sidewalk_Concrete.mat", litShader, new Color(0.68f, 0.68f, 0.68f), 0.3f, 0.0f);
            Material streetLampMat = GetOrCreateMaterial("Assets/Materials/Mat_StreetLamp_Metal.mat", litShader, new Color(0.2f, 0.22f, 0.24f), 0.7f, 0.8f);

            float streetLength = 650f;
            float roadHalfWidth = 7.0f;
            float sidewalkWidth = 3.5f;

            // 1. Sidewalks
            CreateBox(cityRoot.transform, "Sidewalk_Left", new Vector3(-roadHalfWidth - (sidewalkWidth * 0.5f), 0.22f, streetLength * 0.5f), new Vector3(sidewalkWidth, 0.25f, streetLength), sidewalkMat);
            CreateBox(cityRoot.transform, "Sidewalk_Right", new Vector3(roadHalfWidth + (sidewalkWidth * 0.5f), 0.22f, streetLength * 0.5f), new Vector3(sidewalkWidth, 0.25f, streetLength), sidewalkMat);

            // 2. Central Median Hedges (Matching reference screenshot)
            GameObject medianRoot = new GameObject("Median_Hedges");
            medianRoot.transform.parent = cityRoot.transform;
            for (float z = 20f; z < streetLength - 30f; z += 18f)
            {
                CreateBox(medianRoot.transform, $"Hedge_{z}", new Vector3(0f, 0.45f, z), new Vector3(0.9f, 0.7f, 14f), hedgeMat);
            }

            // 3. Flanking Apartment Blocks (Left & Right)
            GameObject buildingsRoot = new GameObject("Apartment_Blocks");
            buildingsRoot.transform.parent = cityRoot.transform;

            float buildingOffset = roadHalfWidth + sidewalkWidth + 14f;
            for (float z = 35f; z < streetLength - 60f; z += 45f)
            {
                // Left Side Apartment Block
                Material matL = (Mathf.RoundToInt(z) % 90 == 0) ? buildingMat1 : buildingMat2;
                CreateApartmentBlock(buildingsRoot.transform, $"Building_L_{z}", new Vector3(-buildingOffset, 14f, z), new Vector3(22f, 28f, 38f), matL, windowMat);

                // Right Side Apartment Block
                Material matR = (Mathf.RoundToInt(z) % 90 == 0) ? buildingMat2 : buildingMat1;
                CreateApartmentBlock(buildingsRoot.transform, $"Building_R_{z}", new Vector3(buildingOffset, 14f, z), new Vector3(22f, 28f, 38f), matR, windowMat);
            }

            // 4. Street Lamps along Sidewalks
            GameObject lampsRoot = new GameObject("Street_Lamps");
            lampsRoot.transform.parent = cityRoot.transform;
            for (float z = 30f; z < streetLength - 40f; z += 40f)
            {
                CreateStreetLamp(lampsRoot.transform, $"Lamp_L_{z}", new Vector3(-roadHalfWidth - 1.2f, 0f, z), streetLampMat);
                CreateStreetLamp(lampsRoot.transform, $"Lamp_R_{z}", new Vector3(roadHalfWidth + 1.2f, 0f, z), streetLampMat);
            }

            // 5. Time Of Day Manager & Weather System
            TimeOfDayManager tod = cityRoot.AddComponent<TimeOfDayManager>();
            WeatherSystem weather = cityRoot.AddComponent<WeatherSystem>();

            // 6. Minimap System
            MinimapSystem minimap = cityRoot.AddComponent<MinimapSystem>();

            Debug.Log("<color=green><b>[CarGame] Stage 4 City World, Apartment Blocks, Time-of-Day & Minimap Generated!</b></color>");
        }

        private static void CreateApartmentBlock(Transform parent, string name, Vector3 pos, Vector3 size, Material wallMat, Material winMat)
        {
            GameObject bldg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bldg.name = name;
            bldg.transform.parent = parent;
            bldg.transform.position = pos;
            bldg.transform.localScale = size;
            bldg.GetComponent<Renderer>().sharedMaterial = wallMat;

            // Simple window facade band
            GameObject winBand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            winBand.name = "WindowBands";
            winBand.transform.parent = bldg.transform;
            winBand.transform.localPosition = new Vector3(pos.x < 0 ? 0.51f : -0.51f, 0f, 0f);
            winBand.transform.localScale = new Vector3(0.04f, 0.75f, 0.85f);
            winBand.GetComponent<Renderer>().sharedMaterial = winMat;
            Object.DestroyImmediate(winBand.GetComponent<Collider>());
        }

        private static void CreateStreetLamp(Transform parent, string name, Vector3 pos, Material metalMat)
        {
            GameObject lampObj = new GameObject(name);
            lampObj.transform.parent = parent;
            lampObj.transform.position = pos;

            // Pole
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "Pole";
            pole.transform.parent = lampObj.transform;
            pole.transform.localPosition = new Vector3(0f, 3.2f, 0f);
            pole.transform.localScale = new Vector3(0.18f, 3.2f, 0.18f);
            pole.GetComponent<Renderer>().sharedMaterial = metalMat;
            Object.DestroyImmediate(pole.GetComponent<Collider>());

            // Light Arm
            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arm.name = "Arm";
            arm.transform.parent = lampObj.transform;
            arm.transform.localPosition = new Vector3(pos.x < 0 ? 0.7f : -0.7f, 6.2f, 0f);
            arm.transform.localScale = new Vector3(1.5f, 0.15f, 0.15f);
            arm.GetComponent<Renderer>().sharedMaterial = metalMat;
            Object.DestroyImmediate(arm.GetComponent<Collider>());

            // Point Light
            GameObject ltObj = new GameObject("Lamp_Light");
            ltObj.transform.parent = arm.transform;
            ltObj.transform.localPosition = new Vector3(pos.x < 0 ? 0.5f : -0.5f, -0.2f, 0f);
            Light lt = ltObj.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = new Color(1.0f, 0.92f, 0.75f);
            lt.intensity = 1.8f;
            lt.range = 14f;
            lt.shadows = LightShadows.None; // Optimized for mobile
        }

        private static GameObject CreateBox(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.parent = parent;
            box.transform.position = pos;
            box.transform.localScale = size;
            box.GetComponent<Renderer>().sharedMaterial = mat;
            return box;
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
    }
}
#endif
