using System;
using System.Collections.Generic;
using GTA3Unity.Core;
using GTA3Unity.Peds;
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
        private List<WheelCollider> m_Wheels = new();

        private void Start()
        {
            m_RigidBody = GetComponent<Rigidbody>();
            Debug.Assert(m_RigidBody != null);

            for(int i = 0; i < 4; i++)
            {
                var wheelObj = InstantiateModel(161);
                if(wheelObj == null)
                {
                    Debug.LogWarning("Cannot load wheels");
                    return;
                }
                wheelObj.GetComponent<MeshCollider>().enabled = false;
                var wheelCol = wheelObj.AddComponent<WheelCollider>();
                wheelObj.transform.SetParent(transform);
                switch(i)
                {
                    case 0: // Front Left
                        wheelObj.transform.localPosition = new Vector3(1, 0, -1);
                        break;
                    case 1: // Front Right
                        wheelObj.transform.localPosition = new Vector3(-1, 0, -1);
                        break;
                    case 2: // Back Left
                        wheelObj.transform.localPosition = new Vector3(1, 0, 1);
                        break;
                    case 3: // Bacl Right
                        wheelObj.transform.localPosition = new Vector3(-1, 0, 1);
                        break;
                }
                m_Wheels.Add(wheelCol);
            }
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
