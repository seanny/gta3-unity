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
        private static readonly Quaternion s_WheelColliderRotationCorrection =
            Quaternion.Euler(0.0f, 180.0f, 0.0f);
        
        private const int WheelCount = 4;
        private const int FrontWheelCount = 2;
        private const float MinSuspensionSpring = 10_000.0f;
        private const float MaxSuspensionSpring = 25_000.0f;
        private const float MinSuspensionDamper = 2_000.0f;
        private const float MaxSuspensionDamper = 5_000.0f;
        private const float MinimumSuspensionTravel = 0.05f;
        private const float WheelMass = 20.0f;

        private List<WheelCollider> m_FrontWheels = new();
        private List<WheelCollider> m_BackWheels = new();

        private static readonly string[] s_WheelFrameNames =
        {
            "wheel_lf_dummy",
            "wheel_rf_dummy",
            "wheel_lb_dummy",
            "wheel_rb_dummy"
        };

        private readonly Transform[] m_WheelVisuals = new Transform[WheelCount];
        private readonly Quaternion[] m_WheelVisualRotationOffsets = new Quaternion[WheelCount];

        private float m_WheelRadius;
        private LayerMask m_LayerMaskVehicleBody;

        public override void SetModel(int modelIndex)
        {
            base.SetModel(modelIndex);
        }

        void Awake()
        {
            m_LayerMaskVehicleBody = LayerMask.GetMask("VehicleBody", "VehicleWheel");
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
            HandleMotor(input);
            HandleSteering(input);
            UpdateWheels();
        }

        private void UpdateWheels()
        {
            foreach(var wheel in m_FrontWheels)
            {
                UpdateWheel(wheel);
            }
            foreach(var wheel in m_BackWheels)
            {
                UpdateWheel(wheel);
            }
        }

        private void UpdateWheel(WheelCollider wheel)
        {
            Vector3 pos;
            Quaternion rot;
            wheel.GetWorldPose(out pos, out rot);
            wheel.transform.position = pos;
            wheel.transform.rotation = rot;
        }

        private void HandleSteering(StarterAssetsInputs input)
        {
            if(m_HandlingData.TransmissionData.DriveType == EDriveType.FrontWheel)
            {
                foreach(var frontWheel in m_FrontWheels)
                {
                    frontWheel.steerAngle = input.move.y * m_HandlingData.SteeringLock;
                }
            }
        }

        private void HandleMotor(StarterAssetsInputs input)
        {
            HandleFrontWheelDrive(input);
            HandleBackWheelDrive(input);
        }

        private void HandleFrontWheelDrive(StarterAssetsInputs input)
        {
            if (m_HandlingData.TransmissionData.DriveType == EDriveType.FrontWheel || m_HandlingData.TransmissionData.DriveType == EDriveType.BothWheel)
            {
                foreach (var wheel in m_FrontWheels)
                {
                    SetWheelTorque(wheel, input);
                }
            }
        }

        private void HandleBackWheelDrive(StarterAssetsInputs input)
        {
            if (m_HandlingData.TransmissionData.DriveType == EDriveType.BackWheel || m_HandlingData.TransmissionData.DriveType == EDriveType.BothWheel)
            {
                foreach (var wheel in m_BackWheels)
                {
                    SetWheelTorque(wheel, input);
                }
            }
        }

        private void SetWheelTorque(WheelCollider wheel, StarterAssetsInputs input)
        {
            wheel.motorTorque = input.move.x * m_HandlingData.TransmissionData.EngineAcceleration;
            if (input.handBrake == true)
            {
                wheel.brakeTorque = m_HandlingData.BrakeDeceleration;
                input.handBrake = false;
            }
            else
            {
                wheel.brakeTorque = 0f;
            }
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

            ConfigureChassisCollision();

            Transform[] wheelFrames = new Transform[WheelCount];
            for(int i = 0; i < WheelCount; i++)
            {
                wheelFrames[i] = FindChildByName(m_PedModel.transform, s_WheelFrameNames[i]);
                if(wheelFrames[i] == null)
                {
                    DisableVehicle(
                        $"Vehicle '{VehicleIdentifier}' is missing the wheel frame '{s_WheelFrameNames[i]}'.");
                    return;
                }
            }

            ConfigureRigidbody();
            CreateWheels(ideCar, wheelFrames);
            LoadVehicleDummy();
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
            Transform[] wheelFrames)
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

            int drivenWheelCount = 0;
            for(int i = 0; i < WheelCount; i++)
            {
                if(IsDrivenWheel(i))
                {
                    drivenWheelCount++;
                }
            }

            for(int i = 0; i < WheelCount; i++)
            {
                bool isFrontWheel = i < FrontWheelCount;
                GameObject wheelAnchor = new GameObject($"WheelCollider_{s_WheelFrameNames[i]}");
                wheelAnchor.transform.SetParent(transform, false);
                wheelAnchor.transform.SetPositionAndRotation(
                    wheelFrames[i].position,
                    wheelFrames[i].rotation * s_WheelColliderRotationCorrection);

                WheelCollider wheel = wheelAnchor.AddComponent<WheelCollider>();
                wheel.radius = m_WheelRadius;
                wheel.mass = WheelMass;
                wheel.suspensionDistance = suspensionTravel;
                wheel.wheelDampingRate = 1.0f;
                wheel.sprungMass = Mathf.Max(
                    1.0f,
                    m_RigidBody.mass * (isFrontWheel ? frontSuspensionShare : rearSuspensionShare) * 0.5f);
                wheel.excludeLayers = m_LayerMaskVehicleBody;

                if(wheelFrames[i].name.Contains("_lf") || wheelFrames[i].name.Contains("_rf"))
                {
                    m_FrontWheels.Add(wheel);
                }
                if(wheelFrames[i].name.Contains("_lb") || wheelFrames[i].name.Contains("_rb"))
                {
                    m_BackWheels.Add(wheel);
                }

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

                CreateWheelVisual(
                    i,
                    wheelAnchor.transform,
                    ideCar.WheelModelId,
                    ideCar.WheelScale);
            }
        }

        private void ConfigureChassisCollision()
        {
            MeshFilter chassisMesh = FindMeshFilterByName(
                m_PedModel.transform,
                "chassis_vlo");

            if(chassisMesh == null || chassisMesh.sharedMesh == null)
            {
                Debug.LogWarning(
                    $"Vehicle '{VehicleIdentifier}' has no chassis_vlo mesh for collision.");
                return;
            }

            MeshCollider chassisCollider =
                chassisMesh.GetComponent<MeshCollider>();
            if(chassisCollider == null)
            {
                chassisCollider = chassisMesh.gameObject.AddComponent<MeshCollider>();
            }

            chassisCollider.sharedMesh = chassisMesh.sharedMesh;
            chassisCollider.convex = true;
            chassisCollider.enabled = true;
        }

        private void LoadVehicleDummy()
        {
            LoadHeadlights();
            LoadTaillights();
            LoadReverseLights();
            LoadBrakeLights();
            LoadIndicators();
            LoadExhaust();
        }

        private void LoadHeadlights()
        {
            // TODO: Implement headlights.
        }

        private void LoadTaillights()
        {
            // TODO: Implement taillights.
        }

        private void LoadReverseLights()
        {
            // TODO: Implement reverse lights.
        }

        private void LoadBrakeLights()
        {
            // TODO: Implement brake lights.
        }

        private void LoadIndicators()
        {
            // TODO: Implement indicators.
        }

        private void LoadExhaust()
        {
            // TODO: Implement exhaust effects.
        }

        private void CreateWheelVisual(
            int wheelIndex,
            Transform wheelAnchor,
            int wheelModelId,
            float wheelScale)
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
            wheelVisual.transform.localScale = Vector3.one * Mathf.Max(0.01f, wheelScale);

            m_WheelVisuals[wheelIndex] = wheelVisual.transform;
            m_WheelVisualRotationOffsets[wheelIndex] = visualRotation;
        }

        private bool IsDrivenWheel(int wheelIndex)
        {
            bool isFrontWheel = wheelIndex < FrontWheelCount;
            EDriveType driveType = HandlingData.TransmissionData.DriveType;

            return driveType == EDriveType.BothWheel ||
                (isFrontWheel && driveType == EDriveType.FrontWheel) ||
                (!isFrontWheel && driveType == EDriveType.BackWheel);
        }

        private void DisableVehicle(string reason)
        {
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

        private static MeshFilter FindMeshFilterByName(
            Transform root,
            string targetName)
        {
            MeshFilter meshFilter = root.GetComponent<MeshFilter>();
            if(meshFilter != null &&
                string.Equals(root.name, targetName, StringComparison.OrdinalIgnoreCase))
            {
                return meshFilter;
            }

            for(int i = 0; i < root.childCount; i++)
            {
                MeshFilter match = FindMeshFilterByName(root.GetChild(i), targetName);
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
