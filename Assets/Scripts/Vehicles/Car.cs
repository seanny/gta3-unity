using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

namespace GTA3Unity.Vehicles
{
    public class Car: Vehicle
    {
        private enum EDriveType
        {
            FrontWheel,
            BackWheel,
            BothWheel
        }

        // Anything above or below these values and wheels start to go crazy
        private const float MIN_SUSPENSION = 10_000f;
        private const float MAX_SUSPENSION = 25_000f;
        private const float MIN_DAMPER = 20_00f;
        private const float MAX_DAMPER = 50_00f;
        private const int FrontWheelCount = 2;

        private static readonly Vector3[] s_WheelPositions =
        {
            new(1, 0, -1), // Front Left
            new(-1, 0, -1), // Front Right
            new(1, 0, 1), // Back Left
            new(-1, 0, 1) // Back Right
        };

        [SerializeField] private EDriveType m_DriveType = EDriveType.BackWheel;
        [SerializeField] private float m_MaxSteeringAngle = 30.0f;
        [SerializeField] private float m_MaxMotorTorque = 1_500.0f;
        [SerializeField] private float m_MaxBrakeTorque = 2_500.0f;
        [SerializeField] private float m_MaxHandbrakeTorque = 20_000.0f;
        [SerializeField] private float m_SteeringResponse = 5.0f;

        private readonly List<WheelCollider> m_Wheels = new();
        private readonly List<Transform> m_WheelVisuals = new();
        private readonly List<Quaternion> m_WheelVisualRotationOffsets = new();

        private Vector2 m_MoveInput;
        private float m_Steering;
        protected bool m_IsHandbrakeOn;

        protected override void Start()
        {
            base.Start();
            CreateWheels();
        }

        public override void OnInput(StarterAssetsInputs input)
        {
            SetHandbrakeInput(input);

            m_MoveInput = input == null
                ? Vector2.zero
                : Vector2.ClampMagnitude(input.move, 1.0f);
        }

        private void FixedUpdate()
        {
            if(m_RigidBody == null || m_Wheels.Count != s_WheelPositions.Length)
            {
                return;
            }

            m_Steering = Mathf.MoveTowards(
                m_Steering,
                -m_MoveInput.x,
                Mathf.Max(0.0f, m_SteeringResponse) * Time.fixedDeltaTime);

            float steeringValue = m_Steering * Mathf.Abs(m_Steering);
            float steeringAngle = steeringValue * m_MaxSteeringAngle;

            float speed = Vector3.Dot(m_RigidBody.linearVelocity, transform.forward);
            float acceleration = Mathf.Clamp(m_MoveInput.y, -1.0f, 1.0f);
            float gasPedal = 0.0f;
            float brakePedal = 0.0f;

            if(Mathf.Abs(speed) < 0.01f)
            {
                gasPedal = acceleration;
            }
            else if(speed * acceleration < 0.0f)
            {
                brakePedal = Mathf.Abs(acceleration);
            }
            else
            {
                gasPedal = acceleration;
            }

            for(int i = 0; i < m_Wheels.Count; i++)
            {
                WheelCollider wheel = m_Wheels[i];
                bool isFrontWheel = i < FrontWheelCount;
                bool isHandbrakeWheel = !isFrontWheel;

                wheel.steerAngle = isFrontWheel ? steeringAngle : 0.0f;
                wheel.motorTorque = !m_IsHandbrakeOn && IsDrivenWheel(i)
                    ? gasPedal * m_MaxMotorTorque
                    : 0.0f;
                wheel.brakeTorque = brakePedal * m_MaxBrakeTorque +
                    (m_IsHandbrakeOn && isHandbrakeWheel ? m_MaxHandbrakeTorque : 0.0f);

                UpdateWheelVisual(i, wheel);
            }
        }

        private void CreateWheels()
        {
            for(int i = 0; i < s_WheelPositions.Length; i++)
            {
                GameObject wheelAnchor = new GameObject($"WheelCollider_{i}");
                wheelAnchor.transform.SetParent(transform, false);
                wheelAnchor.transform.localPosition = s_WheelPositions[i];
                wheelAnchor.transform.localRotation = Quaternion.identity;

                WheelCollider wheel = wheelAnchor.AddComponent<WheelCollider>();
                wheel.suspensionDistance = 0.3f;
                wheel.wheelDampingRate = 1.0f;

                JointSpring suspension = wheel.suspensionSpring;
                suspension.spring = MIN_SUSPENSION;
                suspension.damper = MIN_DAMPER;
                wheel.suspensionSpring = suspension;

                m_Wheels.Add(wheel);
                m_WheelVisuals.Add(null);
                m_WheelVisualRotationOffsets.Add(Quaternion.identity);

                if(FileLoader.Instance == null || !FileLoader.Instance.IsDone)
                {
                    continue;
                }

                GameObject wheelVisual = InstantiateModel(161);
                if(wheelVisual == null)
                {
                    continue;
                }

                Quaternion visualRotation = wheelVisual.transform.localRotation;
                wheelVisual.transform.SetParent(wheelAnchor.transform, false);
                wheelVisual.transform.localPosition = Vector3.zero;
                wheelVisual.transform.localRotation = visualRotation;

                m_WheelVisuals[i] = wheelVisual.transform;
                m_WheelVisualRotationOffsets[i] = visualRotation;
            }
        }

        private bool IsDrivenWheel(int wheelIndex)
        {
            bool isFrontWheel = wheelIndex < FrontWheelCount;

            return m_DriveType == EDriveType.BothWheel ||
                (isFrontWheel && m_DriveType == EDriveType.FrontWheel) ||
                (!isFrontWheel && m_DriveType == EDriveType.BackWheel);
        }

        private void UpdateWheelVisual(int wheelIndex, WheelCollider wheel)
        {
            Transform wheelVisual = m_WheelVisuals[wheelIndex];
            if(wheelVisual == null)
            {
                return;
            }

            wheel.GetWorldPose(out Vector3 position, out Quaternion rotation);
            wheelVisual.SetPositionAndRotation(
                position,
                rotation * m_WheelVisualRotationOffsets[wheelIndex]);
        }

        protected void SetHandbrakeInput(StarterAssetsInputs input)
        {
            m_IsHandbrakeOn = input != null && input.handBrake;
        }
    }
}
