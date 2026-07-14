using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

namespace GTA3Unity.Peds
{
    public class Ped: MonoBehaviour
    {
        private static readonly Quaternion s_ModelBasisRotation =
            Quaternion.Euler(-90.0f, 0.0f, 0.0f);

        public NavMeshAgent NavMeshAgent => m_NavMeshAgent;

        private GameObject m_PedModel;
        private NavMeshAgent m_NavMeshAgent;
        private PedStateMachine m_StateMachine;
        #region Unity Lifecycle
        private IEnumerator Start()
        {
            Debug.Assert(FileLoader.Instance != null);
            while(!FileLoader.Instance.IsDone)
            {
                yield return null;
            }

            m_StateMachine = new PedStateMachine(this);

            int randIndex = Random.Range(0, 127);
            while(randIndex >= 26 && randIndex <= 29)
            {
                // Ranges 26-29 are invalid
                randIndex = Random.Range(0, 126);
            }

            SetModel(randIndex);
        }

        private void Update()
        {
            if(m_StateMachine == null)
            {
                return;
            }

            m_StateMachine.OnUpdate(Time.deltaTime);
        }
        #endregion

        public void PlayAnimation(string animName)
        {
            if(m_PedModel == null)
            {
                Debug.LogError($"Ped {name} does not have a PedModel attached.");
                return;
            }

            FileLoader.Instance.PlayPedAnimation(m_PedModel, animName);
        }

        public void SetModel(int modelIndex)
        {
            if(FileLoader.Instance == null || !FileLoader.Instance.IsDone)
            {
                return;
            }

            if(m_PedModel != null)
            {
                Destroy(m_PedModel);
            }

            GameObject template = FileLoader.Instance.GetModel(modelIndex);

            if (template == null)
            {
                Debug.LogWarning($"Could not load ped model {modelIndex}.");
                return;
            }

            m_PedModel = GameObject.Instantiate<GameObject>(template);
            m_PedModel.name = template.name.Replace("_Template", string.Empty);
            m_PedModel.transform.SetParent(transform, worldPositionStays: false);
            m_PedModel.transform.localPosition = Vector3.zero;
            m_PedModel.transform.localRotation = s_ModelBasisRotation;
            m_PedModel.transform.localScale = Vector3.one;
            m_PedModel.SetActive(true);
            FileLoader.Instance.PlayPedAnimation(m_PedModel);
        }
    }
}
