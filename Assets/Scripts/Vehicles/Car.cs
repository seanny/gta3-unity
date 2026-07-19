using System;
using StarterAssets;
using UnityEngine;

namespace GTA3Unity.Vehicles
{
    public class Car: Vehicle
    {
        private float m_Steering;

        public override void OnInput(StarterAssetsInputs input)
        {
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