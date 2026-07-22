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
            if(!string.IsNullOrEmpty(m_VehicleIdentifier))
            {
                // Allow testing in the editor
                SetHandlingData(m_VehicleIdentifier);
            }
        }

        public bool SetHandlingData(string vehicleIdentifier)
        {
            Debug.Assert(HandlingManager.Data.Count > 0);

            if(!HandlingManager.Data.ContainsKey(vehicleIdentifier))
            {
                return false;
            }
            m_VehicleIdentifier = vehicleIdentifier;
            m_HandlingData = HandlingManager.Data[vehicleIdentifier];
            return true;
        }

        public override void SetModel(int modelIndex)
        {
            base.SetModel(modelIndex);
        }

        public void SetDriver(PedObject ped)
        {
            m_Driver = ped;
            ped.transform.SetParent(transform);
            ped.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.eulerAngles.z);
        }

        public abstract void OnInput(StarterAssetsInputs input);
    }
}
