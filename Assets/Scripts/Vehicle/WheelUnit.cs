using System;
using UnityEngine;

namespace CarGame.Vehicle
{
    [Serializable]
    public class WheelUnit
    {
        [Header("References")]
        public string wheelName = "Wheel";
        public WheelCollider wheelCollider;
        public Transform wheelVisualMesh;

        [Header("Axle Configuration")]
        public bool isSteerable;
        public bool isMotor;
        public bool hasBrakes = true;
        public bool hasHandbrake;

        [Header("Mesh Alignment Offset")]
        [Tooltip("Euler rotation offset to align imported 3D wheel models if their local axes differ.")]
        public Vector3 visualRotationOffset = Vector3.zero;

        [NonSerialized] public float forwardSlip;
        [NonSerialized] public float sidewaysSlip;
        [NonSerialized] public bool isGrounded;
        [NonSerialized] public WheelHit lastHit;

        /// <summary>
        /// Synchronizes the visual wheel mesh transform with the WheelCollider simulation pose.
        /// </summary>
        public void UpdateVisualPose()
        {
            if (wheelCollider == null || wheelVisualMesh == null) return;

            Vector3 worldPos;
            Quaternion worldRot;
            wheelCollider.GetWorldPose(out worldPos, out worldRot);

            wheelVisualMesh.position = worldPos;
            
            if (visualRotationOffset != Vector3.zero)
            {
                wheelVisualMesh.rotation = worldRot * Quaternion.Euler(visualRotationOffset);
            }
            else
            {
                wheelVisualMesh.rotation = worldRot;
            }
        }

        /// <summary>
        /// Updates wheel physics telemetry (ground contact, forward/sideways slip).
        /// </summary>
        public void UpdateTelemetry()
        {
            if (wheelCollider == null) return;

            isGrounded = wheelCollider.GetGroundHit(out lastHit);
            if (isGrounded)
            {
                forwardSlip = Mathf.Abs(lastHit.forwardSlip);
                sidewaysSlip = Mathf.Abs(lastHit.sidewaysSlip);
            }
            else
            {
                forwardSlip = 0f;
                sidewaysSlip = 0f;
            }
        }

        /// <summary>
        /// Applies motor torque directly to the WheelCollider.
        /// </summary>
        public void ApplyMotorTorque(float torque)
        {
            if (wheelCollider == null || !isMotor) return;
            wheelCollider.motorTorque = torque;
        }

        /// <summary>
        /// Applies brake torque to the WheelCollider.
        /// </summary>
        public void ApplyBrakeTorque(float torque)
        {
            if (wheelCollider == null) return;
            wheelCollider.brakeTorque = torque;
        }

        /// <summary>
        /// Applies steer angle to the WheelCollider.
        /// </summary>
        public void ApplySteerAngle(float angle)
        {
            if (wheelCollider == null || !isSteerable) return;
            wheelCollider.steerAngle = angle;
        }
    }
}
