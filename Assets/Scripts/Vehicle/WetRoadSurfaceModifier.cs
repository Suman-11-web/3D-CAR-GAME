using UnityEngine;

namespace CarGame.Vehicle
{
    /// <summary>
    /// Attach to puddle trigger colliders or wet road zones.
    /// Dynamically modulates vehicle tire grip and spray particle intensity.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class WetRoadSurfaceModifier : MonoBehaviour
    {
        [Header("Grip Tuning")]
        [Tooltip("Friction multiplier applied to wheels in this zone (e.g. 0.60 = 40% grip loss in deep puddles)")]
        [Range(0.2f, 1.2f)]
        [SerializeField] private float zoneFrictionMultiplier = 0.65f;

        private void OnTriggerEnter(Collider other)
        {
            VehicleController vehicle = other.GetComponentInParent<VehicleController>();
            if (vehicle != null)
            {
                vehicle.SetSurfaceFrictionMultiplier(zoneFrictionMultiplier);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            VehicleController vehicle = other.GetComponentInParent<VehicleController>();
            if (vehicle != null)
            {
                vehicle.SetSurfaceFrictionMultiplier(1.0f);
            }
        }
    }
}
