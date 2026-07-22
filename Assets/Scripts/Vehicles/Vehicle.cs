using GTA3Unity.Core;
using StarterAssets;
using UnityEngine;

namespace GTA3Unity.Vehicles
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class Vehicle: GtaObject
    {
        public string VehicleIdentifier => m_VehicleIdentifier;
        public PedObject Driver => m_Driver;
        public HandlingData HandlingData => m_HandlingData;

        [SerializeField] private string m_VehicleIdentifier;
        [SerializeField] protected HandlingData m_HandlingData;
        [SerializeField] private PedObject m_Driver;

        protected Rigidbody m_RigidBody;

        protected virtual void Start()
        {
            m_RigidBody = GetComponent<Rigidbody>();
            Debug.Assert(m_RigidBody != null);
            if(!string.IsNullOrEmpty(m_VehicleIdentifier) && HandlingManager.Data.Count > 0)
            {
                // Allow testing in the editor
                SetHandlingData(m_VehicleIdentifier);
            }
        }

        public bool SetHandlingData(string vehicleIdentifier)
        {
            if(!HandlingManager.Data.TryGetValue(vehicleIdentifier, out HandlingData handlingData))
            {
                return false;
            }
            m_VehicleIdentifier = vehicleIdentifier;
            m_HandlingData = handlingData;
            return true;
        }

        public override void SetModel(int modelIndex)
        {
            base.SetModel(modelIndex);

            if(m_PedModel == null)
            {
                return;
            }

            // Vehicle bodies use WheelColliders for physics. Concave MeshColliders
            // cannot be attached to their dynamic Rigidbody.
            MeshCollider[] meshColliders = m_PedModel.GetComponentsInChildren<MeshCollider>(true);
            for(int i = 0; i < meshColliders.Length; i++)
            {
                meshColliders[i].enabled = false;
            }
        }

        public void SetDriver(PedObject ped)
        {
            m_Driver = ped;
            ped.transform.SetParent(transform);
            ped.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.eulerAngles.z);
        }

        public abstract void OnInput(StarterAssetsInputs input);

        protected override GameObject InstantiateModel(int modelIndex)
        {
            GameObject template = FileLoader.Instance.GetModel(modelIndex);

            if (template == null)
            {
                Debug.LogWarning($"Could not load model {modelIndex}.");
                return null;
            }

            var spawnedModel = GameObject.Instantiate<GameObject>(template);
            spawnedModel.name = template.name.Replace("_Template", string.Empty);
            spawnedModel.transform.SetParent(transform, worldPositionStays: false);
            spawnedModel.transform.localPosition = s_ModelBasisPosition;
            spawnedModel.transform.localRotation = Quaternion.identity;
            spawnedModel.transform.localScale = Vector3.one;
            spawnedModel.SetActive(true);
            return spawnedModel;
        }
    }
}
