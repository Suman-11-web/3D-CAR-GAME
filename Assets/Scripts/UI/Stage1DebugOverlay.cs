using UnityEngine;
using CarGame.Vehicle;
using CarGame.CameraSystem;

namespace CarGame.UI
{
    /// <summary>
    /// Temporary on-screen overlay for Stage 1 testing in Editor.
    /// Shows real-time vehicle telemetry (Speed, RPM, Gear, Assists) and controls reference.
    /// (Replaced by production Touch Canvas HUD in Stage 2).
    /// </summary>
    public class Stage1DebugOverlay : MonoBehaviour
    {
        [SerializeField] private VehicleController vehicle;
        [SerializeField] private CameraController camController;

        private void Start()
        {
            if (vehicle == null) vehicle = FindObjectOfType<VehicleController>();
            if (camController == null) camController = FindObjectOfType<CameraController>();
        }

        private void OnGUI()
        {
            if (vehicle == null) return;

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.fontSize = 14;
            boxStyle.alignment = TextAnchor.UpperLeft;
            boxStyle.normal.textColor = Color.white;

            // Telemetry Panel (Top Right)
            GUILayout.BeginArea(new Rect(Screen.width - 290, 20, 270, 230), boxStyle);
            GUILayout.Label("<b>--- VEHICLE TELEMETRY ---</b>");
            GUILayout.Label($"Speed: <color=yellow><b>{Mathf.RoundToInt(vehicle.CurrentSpeedKmh)} km/h</b></color> ({Mathf.RoundToInt(vehicle.CurrentSpeedMph)} mph)");
            GUILayout.Label($"RPM: <b>{Mathf.RoundToInt(vehicle.CurrentRPM)}</b> / {Mathf.RoundToInt(vehicle.MaxRPM)}");
            
            string gearText = vehicle.CurrentGearMode == GearMode.D ? $"D {vehicle.CurrentGear}" : vehicle.CurrentGearMode.ToString();
            GUILayout.Label($"Gearbox: <color=cyan><b>[{gearText}]</b></color> {(vehicle.IsShifting ? "<color=orange>(Shifting)</color>" : "")}");

            GUILayout.Label($"TCS (Traction): {(vehicle.TractionControlActive ? "<color=red><b>ACTIVE (INTERVENING)</b></color>" : "<color=green>READY</color>")}");
            GUILayout.Label($"ABS (Braking): {(vehicle.AbsActive ? "<color=red><b>ACTIVE (INTERVENING)</b></color>" : "<color=green>READY</color>")}");

            if (camController != null)
            {
                GUILayout.Label($"Camera View: <color=magenta><b>{camController.CurrentMode}</b></color>");
            }
            GUILayout.EndArea();

            // Controls Cheat-sheet (Top Left)
            GUILayout.BeginArea(new Rect(20, 20, 310, 190), boxStyle);
            GUILayout.Label("<b>--- STAGE 1 TEST CONTROLS ---</b>");
            GUILayout.Label("<b>W / Up Arrow:</b> Throttle (Accelerate)");
            GUILayout.Label("<b>S / Down Arrow:</b> Brake / Reverse (in R)");
            GUILayout.Label("<b>A / D / Left / Right:</b> Steer");
            GUILayout.Label("<b>Space:</b> Handbrake");
            GUILayout.Label("<b>P / R / N / D:</b> Shift Transmission Mode");
            GUILayout.Label("<b>C or V:</b> Cycle Camera View Mode");
            GUILayout.Label("<b>H:</b> Horn (Stage 3 ready)");
            GUILayout.EndArea();
        }
    }
}
