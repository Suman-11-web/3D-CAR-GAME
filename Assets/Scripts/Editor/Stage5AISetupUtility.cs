#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using CarGame.AI;

namespace CarGame.EditorTools
{
    public static class Stage5AISetupUtility
    {
        [MenuItem("CarGame/Stage 5/Generate Traffic Waypoints, AI Cars & Traffic Lights", false, 50)]
        public static void GenerateTrafficSystem()
        {
            GameObject trafficRoot = GameObject.Find("--- TRAFFIC_SYSTEM ---");
            if (trafficRoot != null)
            {
                if (EditorUtility.DisplayDialog("Traffic System Exists", "--- TRAFFIC_SYSTEM --- already exists. Rebuild?", "Rebuild", "Cancel"))
                {
                    Undo.DestroyObjectImmediate(trafficRoot);
                }
                else return;
            }

            trafficRoot = new GameObject("--- TRAFFIC_SYSTEM ---");
            Undo.RegisterCreatedObjectUndo(trafficRoot, "Generate Traffic System");

            Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material trafficCarMat1 = GetOrCreateMaterial("Assets/Materials/Mat_Traffic_Blue.mat", litShader, new Color(0.12f, 0.35f, 0.72f), 0.75f, 0.4f);
            Material trafficCarMat2 = GetOrCreateMaterial("Assets/Materials/Mat_Traffic_Silver.mat", litShader, new Color(0.75f, 0.76f, 0.78f), 0.85f, 0.7f);
            Material trafficCarMat3 = GetOrCreateMaterial("Assets/Materials/Mat_Traffic_White.mat", litShader, new Color(0.92f, 0.92f, 0.92f), 0.7f, 0.2f);
            Material trafficLightMat = GetOrCreateMaterial("Assets/Materials/Mat_TrafficLight_Post.mat", litShader, new Color(0.15f, 0.16f, 0.18f), 0.3f, 0.2f);

            // 1. Waypoints: Northbound (Forward) and Southbound (Reverse)
            GameObject waypointsRoot = new GameObject("Waypoints_Lanes");
            waypointsRoot.transform.parent = trafficRoot.transform;

            TrafficWaypoint prevNorth = null;
            TrafficWaypoint firstNorth = null;

            // Northbound Lane (X = 3.5m)
            for (float z = 30f; z <= 550f; z += 65f)
            {
                GameObject wpObj = new GameObject($"WP_North_{z}");
                wpObj.transform.parent = waypointsRoot.transform;
                wpObj.transform.position = new Vector3(3.5f, 0.5f, z);
                TrafficWaypoint wp = wpObj.AddComponent<TrafficWaypoint>();
                wp.targetSpeedKmh = 45f;

                if (prevNorth != null) prevNorth.nextWaypoints.Add(wp);
                else firstNorth = wp;

                prevNorth = wp;
            }

            // Loop back or turnaround
            if (prevNorth != null && firstNorth != null)
            {
                prevNorth.nextWaypoints.Add(firstNorth);
            }

            // 2. Traffic Light Intersection at z = 280m
            GameObject lightObj = new GameObject("TrafficLight_Intersection");
            lightObj.transform.parent = trafficRoot.transform;
            lightObj.transform.position = new Vector3(0f, 0f, 280f);
            TrafficLightIntersection tli = lightObj.AddComponent<TrafficLightIntersection>();

            // Virtual AI Stop Barrier Box
            GameObject barrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrier.name = "AI_RedLight_Barrier";
            barrier.transform.parent = lightObj.transform;
            barrier.transform.position = new Vector3(3.5f, 1.0f, 275f);
            barrier.transform.localScale = new Vector3(7.0f, 2.0f, 0.5f);
            barrier.GetComponent<Renderer>().enabled = false; // Invisible barrier

            SerializedObject tliSO = new SerializedObject(tli);
            tliSO.FindProperty("redLightStopBarrier").objectReferenceValue = barrier;
            tliSO.ApplyModifiedProperties();

            // 3. Spawn Initial AI Traffic Cars
            GameObject carsRoot = new GameObject("AI_Traffic_Cars");
            carsRoot.transform.parent = trafficRoot.transform;

            Material[] carMats = { trafficCarMat1, trafficCarMat2, trafficCarMat3 };
            float[] startZ = { 60f, 140f, 220f, 360f, 440f };

            TrafficPoolManager poolMgr = trafficRoot.AddComponent<TrafficPoolManager>();

            for (int i = 0; i < startZ.Length; i++)
            {
                Material mat = carMats[i % carMats.Length];
                GameObject aiCar = CreateTrafficCarPlaceholder(carsRoot.transform, $"AI_Car_{i + 1}", new Vector3(3.5f, 0.6f, startZ[i]), mat);
                AITrafficCar aiComp = aiCar.GetComponent<AITrafficCar>();
                aiComp.InitializeRoute(firstNorth, 40f);
                poolMgr.RegisterTrafficCar(aiComp);
            }

            // 4. Pedestrian Agent at zebra crossing
            GameObject pedObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            pedObj.name = "Pedestrian_Crossing";
            pedObj.transform.parent = trafficRoot.transform;
            pedObj.transform.position = new Vector3(-6.5f, 0.9f, 280f);
            pedObj.transform.localScale = new Vector3(0.5f, 0.9f, 0.5f);

            GameObject pA = new GameObject("PointA");
            pA.transform.parent = pedObj.transform;
            pA.transform.position = new Vector3(-6.5f, 0.9f, 280f);

            GameObject pB = new GameObject("PointB");
            pB.transform.parent = pedObj.transform;
            pB.transform.position = new Vector3(6.5f, 0.9f, 280f);

            PedestrianAgent ped = pedObj.AddComponent<PedestrianAgent>();
            SerializedObject pedSO = new SerializedObject(ped);
            pedSO.FindProperty("pointA").objectReferenceValue = pA.transform;
            pedSO.FindProperty("pointB").objectReferenceValue = pB.transform;
            pedSO.ApplyModifiedProperties();

            Debug.Log("<color=green><b>[CarGame] Stage 5 Traffic System, AI Cars, Traffic Lights & Pedestrian Generated!</b></color>");
        }

        private static GameObject CreateTrafficCarPlaceholder(Transform parent, string name, Vector3 pos, Material bodyMat)
        {
            GameObject car = new GameObject(name);
            car.transform.parent = parent;
            car.transform.position = pos;

            Rigidbody rb = car.AddComponent<Rigidbody>();
            rb.mass = 1400f;
            rb.drag = 0.1f;
            rb.angularDrag = 2.0f;

            BoxCollider col = car.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.45f, 0f);
            col.size = new Vector3(1.8f, 1.2f, 4.2f);

            // Visual Body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.parent = car.transform;
            body.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            body.transform.localScale = new Vector3(1.8f, 0.85f, 4.2f);
            body.GetComponent<Renderer>().sharedMaterial = bodyMat;
            Object.DestroyImmediate(body.GetComponent<Collider>());

            // Cabin
            GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabin.name = "Cabin";
            cabin.transform.parent = car.transform;
            cabin.transform.localPosition = new Vector3(0f, 0.95f, -0.2f);
            cabin.transform.localScale = new Vector3(1.5f, 0.65f, 2.2f);
            cabin.GetComponent<Renderer>().sharedMaterial = bodyMat;
            Object.DestroyImmediate(cabin.GetComponent<Collider>());

            AITrafficCar aiCar = car.AddComponent<AITrafficCar>();
            return car;
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
