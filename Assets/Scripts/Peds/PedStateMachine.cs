using UnityEngine;
using UnityEngine.AI;

namespace GTA3Unity.Peds
{
    public enum EPedState
    {
        None,
        Idle,
        WanderPath,
        PedFleePosition,
        PedFleeGameObject,
        Attack,
        Fight,
        MoveToPosition,
        MoveToGameObject,
        OnFire
    };

    public sealed class PedStateMachine
    {
        private NavMeshAgent m_NavMeshAgent;
        private EPedState m_PedState;
        private Ped m_AttachedPed;

        private const float MIN_MOVE_DISTANCE = 1.0f;

        public PedStateMachine(Ped attachedPed)
        {
            m_AttachedPed = attachedPed;
            m_NavMeshAgent = attachedPed.NavMeshAgent;
        }

        public void MoveToPosition(Vector3 position, float speed = 1.0f, string movementAnimation = "walk_civi")
        {
            m_PedState = EPedState.MoveToPosition;
            m_NavMeshAgent.SetDestination(position);
            m_NavMeshAgent.speed = speed;
            m_NavMeshAgent.isStopped = false;
        }

        public void SetIdle()
        {
            m_AttachedPed.PlayAnimation("idle_stance");
            m_NavMeshAgent.isStopped = true;
        }

        public void OnUpdate(float deltaTime)
        {
            switch (m_PedState)
            {
                case EPedState.Idle:
                    var node = PathNode.GetNearestPathNode(m_AttachedPed.transform.position);
                    if(node != null)
                    {
                        MoveToPosition(node.transform.position);
                    }
                    break;
                case EPedState.MoveToPosition:
                    float distance = Vector3.Distance(m_NavMeshAgent.transform.position, m_NavMeshAgent.destination);
                    if (distance < MIN_MOVE_DISTANCE)
                    {
                        SetIdle();
                    }
                    break;
            }
        }
    }
}