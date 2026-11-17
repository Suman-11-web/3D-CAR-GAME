#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using CarGame.FX;
using CarGame.Audio;
using CarGame.Damage;
using CarGame.Vehicle;
using CarGame.UI;

namespace CarGame.EditorTools
{
    public static class Stage3FXSetupUtility
    {
        [MenuItem("CarGame/Stage 3/Attach Lights, Audio and FX to Car", false, 30)]
        public static void AttachFXToVehicle()
        {
            GameObject carObj = GameObject.FindWithTag("Player");
            if (carObj == null)
            {
                carObj = GameObject.Find("Player_Hatchback");
            }
            if (carObj == null)
            {
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("Error", "Player car not found in scene. Run Stage 1 setup first!", "OK");
                }
                return;
            }

            Undo.RegisterCompleteObjectUndo(carObj, "Attach Stage 3 FX");

            // 1. VehicleLightingSystem
            VehicleLightingSystem lighting = carObj.GetComponent<VehicleLightingSystem>();
            if (lighting == null) lighting = carObj.AddComponent<VehicleLightingSystem>();

            Transform lightsRoot = carObj.transform.Find("Lights");
            if (lightsRoot == null)
            {
                GameObject lr = new GameObject("Lights");
                lr.transform.parent = carObj.transform;
                lr.transform.localPosition = Vector3.zero;
                lightsRoot = lr.transform;
            }

            // Headlights (Spotlights)
            Light flHead = CreateSpotlight(lightsRoot, "Headlight_FL", new Vector3(-0.65f, 0.65f, 2.05f), new Vector3(8f, 0f, 0f), Color.white, 3.2f, 50f, 42f);
            Light frHead = CreateSpotlight(lightsRoot, "Headlight_FR", new Vector3(0.65f, 0.65f, 2.05f), new Vector3(8f, 0f, 0f), Color.white, 3.2f, 50f, 42f);

            // Tail & Brake Lights (Red)
            Light rlTail = CreateSpotlight(lightsRoot, "TailLight_RL", new Vector3(-0.65f, 0.72f, -2.05f), new Vector3(0f, 180f, 0f), Color.red, 1.2f, 18f, 75f);
            Light rrTail = CreateSpotlight(lightsRoot, "TailLight_RR", new Vector3(0.65f, 0.72f, -2.05f), new Vector3(0f, 180f, 0f), Color.red, 1.2f, 18f, 75f);

            // Reverse Lights (White)
            Light rlRev = CreateSpotlight(lightsRoot, "ReverseLight_RL", new Vector3(-0.45f, 0.68f, -2.05f), new Vector3(0f, 180f, 0f), Color.white, 1.8f, 12f, 60f);
            Light rrRev = CreateSpotlight(lightsRoot, "ReverseLight_RR", new Vector3(0.45f, 0.68f, -2.05f), new Vector3(0f, 180f, 0f), Color.white, 1.8f, 12f, 60f);

            // Turn Signals (Amber)
            Color amber = new Color(1f, 0.62f, 0.05f);
            Light lTurnF = CreatePointLight(lightsRoot, "TurnSignal_FL", new Vector3(-0.85f, 0.65f, 1.95f), amber, 2.5f, 3.5f);
            Light rTurnF = CreatePointLight(lightsRoot, "TurnSignal_FR", new Vector3(0.85f, 0.65f, 1.95f), amber, 2.5f, 3.5f);
            Light lTurnR = CreatePointLight(lightsRoot, "TurnSignal_RL", new Vector3(-0.85f, 0.72f, -2.0f), amber, 2.5f, 3.5f);
            Light rTurnR = CreatePointLight(lightsRoot, "TurnSignal_RR", new Vector3(0.85f, 0.72f, -2.0f), amber, 2.5f, 3.5f);

            SerializedObject lightSO = new SerializedObject(lighting);
            AssignLightArray(lightSO.FindProperty("headLights"), new Light[] { flHead, frHead });
            AssignLightArray(lightSO.FindProperty("rearLights"), new Light[] { rlTail, rrTail });
            AssignLightArray(lightSO.FindProperty("reverseLights"), new Light[] { rlRev, rrRev });
            AssignLightArray(lightSO.FindProperty("leftTurnLights"), new Light[] { lTurnF, lTurnR });
            AssignLightArray(lightSO.FindProperty("rightTurnLights"), new Light[] { rTurnF, rTurnR });
            lightSO.ApplyModifiedProperties();

            // 2. Audio System
            VehicleAudioSystem audioSys = carObj.GetComponent<VehicleAudioSystem>();
            if (audioSys == null) audioSys = carObj.AddComponent<VehicleAudioSystem>();

            // 3. Skidmark & Spray System
            SkidmarkAndSpraySystem skidSys = carObj.GetComponent<SkidmarkAndSpraySystem>();
            if (skidSys == null) skidSys = carObj.AddComponent<SkidmarkAndSpraySystem>();

            Transform wheelsRoot = carObj.transform.Find("Wheels");
            if (wheelsRoot != null)
            {
                TrailRenderer[] trails = new TrailRenderer[4];
                int idx = 0;
                foreach (Transform wheel in wheelsRoot)
                {
                    if (idx >= 4) break;
                    TrailRenderer tr = wheel.GetComponent<TrailRenderer>();
                    if (tr == null)
                    {
                        tr = wheel.gameObject.AddComponent<TrailRenderer>();
                        tr.time = 4.5f;
                        tr.startWidth = 0.28f;
                        tr.endWidth = 0.28f;
                        tr.emitting = false;
                        tr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        tr.material = new Material(Shader.Find("Sprites/Default")) { color = new Color(0.12f, 0.12f, 0.12f, 0.65f) };
                    }
                    trails[idx++] = tr;
                }

                SerializedObject skidSO = new SerializedObject(skidSys);
                SerializedProperty trailProp = skidSO.FindProperty("wheelSkidTrails");
                trailProp.arraySize = 4;
                for (int i = 0; i < 4; i++) trailProp.GetArrayElementAtIndex(i).objectReferenceValue = trails[i];
                skidSO.ApplyModifiedProperties();
            }

            // 4. Damage System
            VehicleDamageSystem dmgSys = carObj.GetComponent<VehicleDamageSystem>();
            if (dmgSys == null) dmgSys = carObj.AddComponent<VehicleDamageSystem>();

            // Engine Smoke Particle System on Hood
            Transform existingSmoke = carObj.transform.Find("FX_EngineDamageSmoke");
            if (existingSmoke == null)
            {
                GameObject smokeObj = new GameObject("FX_EngineDamageSmoke");
                smokeObj.transform.parent = carObj.transform;
                smokeObj.transform.localPosition = new Vector3(0f, 0.85f, 1.4f);
                ParticleSystem ps = smokeObj.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.startLifetime = 1.6f;
                main.startSpeed = 1.2f;
                main.startSize = 0.8f;
                main.startColor = new Color(0.2f, 0.2f, 0.2f, 0.6f);
                var em = ps.emission;
                em.rateOverTime = 20f;
                em.enabled = false;

                SerializedObject dmgSO = new SerializedObject(dmgSys);
                dmgSO.FindProperty("engineSmokeParticle").objectReferenceValue = ps;
                dmgSO.ApplyModifiedProperties();
            }

            // 5. Connect HUD Repair Button
            GameObject btnWrench = GameObject.Find("Btn_WrenchRepair");
            if (btnWrench != null)
            {
                UnityEngine.UI.Button btn = btnWrench.GetComponent<UnityEngine.UI.Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(dmgSys.RepairVehicle);
                }
            }

            // 6. Spawn Wet Road Puddles on Highway
            SpawnRoadPuddleZone("Puddle_Zone_1", new Vector3(0f, 0.12f, 120f), new Vector3(12f, 0.05f, 25f));
            SpawnRoadPuddleZone("Puddle_Zone_2", new Vector3(0f, 0.12f, 320f), new Vector3(12f, 0.05f, 30f));

            Debug.Log("<color=green><b>[CarGame] Stage 3 FX, Audio, Lights, Damage & Wet Puddles Attached Successfully!</b></color>");
        }

        private static void SpawnRoadPuddleZone(string name, Vector3 pos, Vector3 size)
        {
            GameObject existing = GameObject.Find(name);
            if (existing != null) Object.DestroyImmediate(existing);

            GameObject puddle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            puddle.name = name;
            puddle.transform.position = pos;
            puddle.transform.localScale = size;

            Collider col = puddle.GetComponent<Collider>();
            col.isTrigger = true;

            puddle.AddComponent<WetRoadSurfaceModifier>();

            Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material puddleMat = new Material(litShader);
            puddleMat.name = "Mat_Puddle_Water";
            if (puddleMat.HasProperty("_BaseColor")) puddleMat.SetColor("_BaseColor", new Color(0.12f, 0.14f, 0.16f, 0.95f));
            else if (puddleMat.HasProperty("_Color")) puddleMat.SetColor("_Color", new Color(0.12f, 0.14f, 0.16f, 0.95f));

            if (puddleMat.HasProperty("_Smoothness")) puddleMat.SetFloat("_Smoothness", 0.98f); // Glossy puddle reflection
            if (puddleMat.HasProperty("_Metallic")) puddleMat.SetFloat("_Metallic", 0.1f);

            puddle.GetComponent<Renderer>().sharedMaterial = puddleMat;
        }

        private static Light CreateSpotlight(Transform parent, string name, Vector3 pos, Vector3 rot, Color color, float intensity, float range, float spotAngle)
        {
            GameObject obj = new GameObject(name);
            obj.transform.parent = parent;
            obj.transform.localPosition = pos;
            obj.transform.localEulerAngles = rot;
            Light lt = obj.AddComponent<Light>();
            lt.type = LightType.Spot;
            lt.color = color;
            lt.intensity = intensity;
            lt.range = range;
            lt.spotAngle = spotAngle;
            return lt;
        }

        private static Light CreatePointLight(Transform parent, string name, Vector3 pos, Color color, float intensity, float range)
        {
            GameObject obj = new GameObject(name);
            obj.transform.parent = parent;
            obj.transform.localPosition = pos;
            Light lt = obj.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = color;
            lt.intensity = intensity;
            lt.range = range;
            return lt;
        }

        private static void AssignLightArray(SerializedProperty prop, Light[] lights)
        {
            prop.arraySize = lights.Length;
            for (int i = 0; i < lights.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = lights[i];
            }
        }
    }
}
#endif
