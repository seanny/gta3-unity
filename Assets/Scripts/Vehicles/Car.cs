using System;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

namespace GTA3Unity.Vehicles
{
    public class Car: Vehicle
    {
        // Anything above or below these values and wheels start to go crazy
        private const float MIN_SUSPENSION = 10_000f;
        private const float MAX_SUSPENSION = 25_000f;
        private const float MIN_DAMPER = 20_00f;
        private const float MAX_DAMPER = 50_00f;

        private float m_Steering;
        private List<WheelCollider> m_Wheels = new();

        protected override void Start()
        {
            base.Start();
            for(int i = 0; i < 4; i++)
            {
                var wheelObj = InstantiateModel(161);
                if(wheelObj == null)
                {
                    Debug.LogWarning("Cannot load wheels");
                    return;
                }
                var wheelCol = wheelObj.AddComponent<WheelCollider>();
                var suspension = wheelCol.suspensionSpring;
                suspension.spring = MIN_SUSPENSION;
                suspension.damper = MIN_DAMPER;

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

        public override void OnInput(StarterAssetsInputs input)
        {
            // TODO: Move some of this code out of OnInput (which is called by Update) and into FixedUpdate()
            if (input == null)
            {
                return;
            }

            if (m_RigidBody == null)
            {
                m_RigidBody = GetComponent<Rigidbody>();
            }

            Vector2 move = input.move;
            float throttle = Mathf.Clamp(move.y, -1.0f, 1.0f);
            float targetSpeed = throttle * 10.0f;
            Vector3 velocity = m_RigidBody.linearVelocity;
            Vector3 forwardVelocity = transform.forward * Vector3.Dot(velocity, transform.forward);
            Vector3 targetVelocity = transform.forward * targetSpeed;

            m_RigidBody.AddForce(targetVelocity - forwardVelocity, ForceMode.Acceleration);

            m_Steering = Mathf.MoveTowards(m_Steering, move.x, Time.deltaTime * 5.0f);
            float speedFactor = Mathf.Clamp01(Mathf.Abs(Vector3.Dot(velocity, transform.forward)) / 10.0f);
            m_RigidBody.MoveRotation(m_RigidBody.rotation * Quaternion.Euler(0.0f,
                m_Steering * 90.0f * speedFactor * Time.deltaTime, 0.0f));
        }
    }
}