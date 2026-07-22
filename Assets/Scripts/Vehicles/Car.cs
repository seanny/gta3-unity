using System;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using IdeCar = RenderWareIo.Structs.Ide.Car;

namespace GTA3Unity.Vehicles
{
    public class Car : Vehicle
    {
        private static readonly Quaternion s_DefaultVehicleRotation = Quaternion.Euler(270,-180,0);
        
        private const int WheelCount = 4;
        private const int FrontWheelCount = 2;
        private const float MinSuspensionSpring = 10_000.0f;
        private const float MaxSuspensionSpring = 25_000.0f;
        private const float MinSuspensionDamper = 2_000.0f;
        private const float MaxSuspensionDamper = 5_000.0f;
        private const float MinimumSuspensionTravel = 0.05f;
        private const float WheelMass = 20.0f;
        private const float HandbrakeTorque = 20_000.0f;
        private const float SteeringResponse = 5.0f;
        private const float KilometresPerHourToMetresPerSecond = 1.0f / 3.6f;

        private static readonly string[] s_WheelFrameNames =
        {
            "wheel_lf_dummy",
            "wheel_rf_dummy",
            "wheel_lb_dummy",
            "wheel_rb_dummy"
        };

        private readonly WheelCollider[] m_Wheels = new WheelCollider[WheelCount];
        private readonly Transform[] m_WheelVisuals = new Transform[WheelCount];
        private readonly Quaternion[] m_WheelVisualRotationOffsets = new Quaternion[WheelCount];

        private Vector2 m_MoveInput;
        private float m_SteeringInput;
        private float m_WheelRadius;
        private float m_MaxVelocity;
        private float m_MotorTorque;
        private float m_FrontBrakeTorque;
        private float m_RearBrakeTorque;
        private bool m_IsInitialized;

        private List<GameObject> m_CarPieces = new();

        protected bool m_IsHandbrakeOn;

        public override void SetModel(int modelIndex)
        {
            base.SetModel(modelIndex);
        }

        protected override void Start()
        {
            base.Start();

            if(FileLoader.Instance == null)
            {
                DisableVehicle("FileLoader is not available.");
                return;
            }

            StartCoroutine(InitializeWhenReady());
        }

        public override void OnInput(StarterAssetsInputs input)
        {
            SetHandbrakeInput(input);
            m_MoveInput = input == null
                ? Vector2.zero
                : Vector2.ClampMagnitude(input.move, 1.0f);
        }

        private IEnumerator InitializeWhenReady()
        {
            while(FileLoader.Instance != null && !FileLoader.Instance.IsActuallyInit)
            {
                yield return null;
            }

            if(FileLoader.Instance == null)
            {
                DisableVehicle("FileLoader was destroyed before initialization completed.");
                yield break;
            }

            InitializeVehicle();
        }

        private void InitializeVehicle()
        {
            if(string.IsNullOrWhiteSpace(VehicleIdentifier))
            {
                DisableVehicle("VehicleIdentifier is empty; GTA handling data is required.");
                return;
            }

            if(!SetHandlingData(VehicleIdentifier) || HandlingData == null)
            {
                DisableVehicle($"No handling data was found for '{VehicleIdentifier}'.");
                return;
            }

            if(!FileLoader.Instance.TryGetCarDefinition(VehicleIdentifier, out IdeCar ideCar))
            {
                DisableVehicle($"No vehicle IDE definition was found for '{VehicleIdentifier}'.");
                return;
            }

            if(ideCar.WheelScale <= 0.0f)
            {
                DisableVehicle($"Vehicle '{VehicleIdentifier}' has an invalid wheel scale.");
                return;
            }

            SetModel(ideCar.Id);
            if(m_PedModel == null)
            {
                DisableVehicle($"Could not load the vehicle model for '{VehicleIdentifier}'.");
                return;
            }

            List<Renderer> lod0Renderers = new();
            List<Renderer> lod1Renderers = new();
            Renderer[] modelRenderers = m_PedModel.GetComponentsInChildren<Renderer>(true);
            for(int i = 0; i < modelRenderers.Length; i++)
            {
                Renderer renderer = modelRenderers[i];
                if(renderer.gameObject.name.EndsWith("_hi", StringComparison.OrdinalIgnoreCase))
                {
                    lod0Renderers.Add(renderer);
                }
                else if(renderer.gameObject.name.EndsWith("_vlo", StringComparison.OrdinalIgnoreCase))
                {
                    lod1Renderers.Add(renderer);
                }
            }

            if(lod0Renderers.Count > 0 && lod1Renderers.Count > 0)
            {
                LODGroup lodGroup = m_PedModel.GetComponent<LODGroup>();
                if(lodGroup == null)
                {
                    lodGroup = m_PedModel.AddComponent<LODGroup>();
                }
                lodGroup.fadeMode = LODFadeMode.CrossFade;
                lodGroup.animateCrossFading = true;

                lodGroup.SetLODs(new[]
                {
                    new LOD(0.8f, lod0Renderers.ToArray()),
                    new LOD(0.1f, lod1Renderers.ToArray())
                });
                lodGroup.RecalculateBounds();
            }

            Transform[] wheelFrames = new Transform[WheelCount];
            Vector3[] wheelPositions = new Vector3[WheelCount];
            for(int i = 0; i < WheelCount; i++)
            {
                wheelFrames[i] = FindChildByName(m_PedModel.transform, s_WheelFrameNames[i]);
                if(wheelFrames[i] == null)
                {
                    DisableVehicle(
                        $"Vehicle '{VehicleIdentifier}' is missing the wheel frame '{s_WheelFrameNames[i]}'.");
                    return;
                }

                wheelPositions[i] = transform.InverseTransformPoint(wheelFrames[i].position);
            }

            ConfigureRigidbody();
            CreateWheels(ideCar, wheelFrames, wheelPositions);
            m_IsInitialized = true;
        }

        private void ConfigureRigidbody()
        {
            float mass = Mathf.Max(1.0f, HandlingData.Mass);
            m_RigidBody.mass = mass;
            m_RigidBody.centerOfMass = ConvertGtaVectorToUnity(HandlingData.CentreOfMass);

            Vector3 dimensions = HandlingData.Dimensions;
            float aerodynamicArea = Mathf.Abs(dimensions.x * dimensions.z);
            m_RigidBody.linearDamping = aerodynamicArea / mass;
            m_RigidBody.angularDamping = 0.05f;
            m_RigidBody.interpolation = RigidbodyInterpolation.Interpolate;
            m_RigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private void CreateWheels(
            IdeCar ideCar,
            Transform[] wheelFrames,
            Vector3[] wheelPositions)
        {
            m_WheelRadius = ideCar.WheelScale * 0.5f;

            float suspensionTravel = Mathf.Max(
                MinimumSuspensionTravel,
                Mathf.Abs(HandlingData.SuspensionUpperLimit - HandlingData.SuspensionLowerLimit));
            float suspensionTarget = Mathf.InverseLerp(
                HandlingData.SuspensionUpperLimit,
                HandlingData.SuspensionLowerLimit,
                0.0f);
            float suspensionSpring = Mathf.Clamp(
                Mathf.Abs(HandlingData.SuspensionForceLevel) * 10_000.0f,
                MinSuspensionSpring,
                MaxSuspensionSpring);
            float suspensionDamper = Mathf.Clamp(
                Mathf.Abs(HandlingData.SuspensionDampingLevel) * 20_000.0f,
                MinSuspensionDamper,
                MaxSuspensionDamper);

            float frontSuspensionShare = Mathf.Clamp01(HandlingData.SuspensionBias);
            float rearSuspensionShare = 1.0f - frontSuspensionShare;
            float frontGripBias = Mathf.Max(0.01f, 2.0f * HandlingData.TractionBias);
            float rearGripBias = Mathf.Max(0.01f, 2.0f - frontGripBias);

            float totalBrakeTorque =
                Mathf.Max(0.0f, HandlingData.Mass) *
                Mathf.Max(0.0f, HandlingData.BrakeDeceleration) *
                m_WheelRadius;
            m_FrontBrakeTorque = totalBrakeTorque * Mathf.Max(0.0f, 2.0f * HandlingData.BrakeBias) * 0.25f;
            m_RearBrakeTorque = totalBrakeTorque * Mathf.Max(0.0f, 2.0f * (1.0f - HandlingData.BrakeBias)) * 0.25f;

            int drivenWheelCount = 0;
            for(int i = 0; i < WheelCount; i++)
            {
                if(IsDrivenWheel(i))
                {
                    drivenWheelCount++;
                }
            }

            float engineForce = Mathf.Max(0.0f, HandlingData.TransmissionData.EngineAcceleration) *
                Mathf.Max(1.0f, HandlingData.Mass);
            m_MotorTorque = drivenWheelCount > 0
                ? engineForce * m_WheelRadius / drivenWheelCount
                : 0.0f;
            m_MaxVelocity = Mathf.Max(0.0f, HandlingData.TransmissionData.MaxVelocity) *
                KilometresPerHourToMetresPerSecond;

            for(int i = 0; i < WheelCount; i++)
            {
                bool isFrontWheel = i < FrontWheelCount;
                GameObject wheelAnchor = new GameObject($"WheelCollider_{s_WheelFrameNames[i]}");
                wheelAnchor.transform.SetParent(transform, false);
                wheelAnchor.transform.localPosition = wheelPositions[i];

                // GTA vehicle models use the opposite forward axis to the Unity vehicle root.
                wheelAnchor.transform.localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);

                WheelCollider wheel = wheelAnchor.AddComponent<WheelCollider>();
                wheel.radius = m_WheelRadius;
                wheel.mass = WheelMass;
                wheel.suspensionDistance = suspensionTravel;
                wheel.wheelDampingRate = 1.0f;
                wheel.sprungMass = Mathf.Max(
                    1.0f,
                    m_RigidBody.mass * (isFrontWheel ? frontSuspensionShare : rearSuspensionShare) * 0.5f);

                JointSpring suspension = wheel.suspensionSpring;
                suspension.spring = suspensionSpring;
                suspension.damper = suspensionDamper;
                suspension.targetPosition = Mathf.Clamp01(suspensionTarget);
                wheel.suspensionSpring = suspension;

                float axleGripBias = isFrontWheel ? frontGripBias : rearGripBias;
                wheel.forwardFriction = CreateFrictionCurve(
                    Mathf.Max(0.01f, HandlingData.TractionLoss) * axleGripBias);
                wheel.sidewaysFriction = CreateFrictionCurve(
                    Mathf.Max(0.01f, HandlingData.TractionMultiplier) * axleGripBias);

                wheelFrames[i].gameObject.SetActive(false);
                m_Wheels[i] = wheel;
                CreateWheelVisual(i, wheelAnchor.transform, ideCar.WheelModelId);
            }
        }

        private void CreateWheelVisual(int wheelIndex, Transform wheelAnchor, int wheelModelId)
        {
            if(wheelModelId <= 0)
            {
                Debug.LogWarning($"Vehicle '{VehicleIdentifier}' has no wheel model.");
                return;
            }

            GameObject wheelVisual = InstantiateModel(wheelModelId);
            if(wheelVisual == null)
            {
                Debug.LogWarning(
                    $"Could not load wheel model {wheelModelId} for vehicle '{VehicleIdentifier}'.");
                return;
            }

            Quaternion visualRotation = wheelVisual.transform.localRotation;
            wheelVisual.transform.SetParent(wheelAnchor, false);
            wheelVisual.transform.localPosition = Vector3.zero;
            wheelVisual.transform.localRotation = visualRotation;

            m_WheelVisuals[wheelIndex] = wheelVisual.transform;
            m_WheelVisualRotationOffsets[wheelIndex] = visualRotation;
        }

        private void FixedUpdate()
        {
            if(!m_IsInitialized)
            {
                return;
            }

            m_SteeringInput = Mathf.MoveTowards(
                m_SteeringInput,
                -m_MoveInput.x,
                SteeringResponse * Time.fixedDeltaTime);

            float steeringValue = m_SteeringInput * Mathf.Abs(m_SteeringInput);
            float steeringAngle = steeringValue * HandlingData.SteeringLock;
            float speed = Vector3.Dot(m_RigidBody.linearVelocity, GetVehicleForward());
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

            float speedFactor = m_MaxVelocity > 0.0f
                ? Mathf.Clamp01(1.0f - Mathf.Abs(speed) / m_MaxVelocity)
                : 0.0f;

            for(int i = 0; i < WheelCount; i++)
            {
                WheelCollider wheel = m_Wheels[i];
                bool isFrontWheel = i < FrontWheelCount;
                bool isHandbrakeWheel = !isFrontWheel;

                wheel.steerAngle = isFrontWheel ? steeringAngle : 0.0f;
                wheel.motorTorque = !m_IsHandbrakeOn && IsDrivenWheel(i)
                    ? gasPedal * m_MotorTorque * speedFactor
                    : 0.0f;
                wheel.brakeTorque = brakePedal *
                    (isFrontWheel ? m_FrontBrakeTorque : m_RearBrakeTorque) +
                    (m_IsHandbrakeOn && isHandbrakeWheel ? HandbrakeTorque : 0.0f);

                UpdateWheelVisual(i, wheel);
            }
        }

        private bool IsDrivenWheel(int wheelIndex)
        {
            bool isFrontWheel = wheelIndex < FrontWheelCount;
            EDriveType driveType = HandlingData.TransmissionData.DriveType;

            return driveType == EDriveType.BothWheel ||
                (isFrontWheel && driveType == EDriveType.FrontWheel) ||
                (!isFrontWheel && driveType == EDriveType.BackWheel);
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

        private void SetHandbrakeInput(StarterAssetsInputs input)
        {
            m_IsHandbrakeOn = input != null && input.handBrake;
        }

        private void DisableVehicle(string reason)
        {
            m_IsInitialized = false;
            Debug.LogError($"Vehicle '{name}' disabled: {reason}", this);
            enabled = false;
        }

        private static Transform FindChildByName(Transform root, string targetName)
        {
            if(string.Equals(root.name, targetName, StringComparison.OrdinalIgnoreCase))
            {
                return root;
            }

            for(int i = 0; i < root.childCount; i++)
            {
                Transform match = FindChildByName(root.GetChild(i), targetName);
                if(match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static WheelFrictionCurve CreateFrictionCurve(float stiffness)
        {
            return new WheelFrictionCurve
            {
                extremumSlip = 0.4f,
                extremumValue = 1.0f,
                asymptoteSlip = 0.8f,
                asymptoteValue = 0.75f,
                stiffness = stiffness
            };
        }

        private static Vector3 ConvertGtaVectorToUnity(Vector3 value)
        {
            return new Vector3(value.x, value.z, -value.y);
        }

        private Vector3 GetVehicleForward()
        {
            return -transform.forward;
        }
    }
}
