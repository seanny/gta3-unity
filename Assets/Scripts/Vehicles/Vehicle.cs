using GTA3Unity.Core;
using StarterAssets;
using UnityEngine;

namespace GTA3Unity.Vehicles
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class Vehicle: GtaObject
    {
        public PedObject Driver => m_Driver;

        [SerializeField] private PedObject m_Driver;

        protected Rigidbody m_RigidBody;

        protected virtual void Start()
        {
            m_RigidBody = GetComponent<Rigidbody>();
            Debug.Assert(m_RigidBody != null);
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
