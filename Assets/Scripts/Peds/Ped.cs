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

        private GameObject m_PedModel;

        private IEnumerator Start()
        {
            while(FileLoader.Instance == null || !FileLoader.Instance.IsDone)
            {
                yield return null;
            }

            int randIndex = Random.Range(0, 127);
            while(randIndex >= 26 && randIndex <= 29)
            {
                // Ranges 26-29 are invalid
                randIndex = Random.Range(0, 126);
            }

            GameObject template = FileLoader.Instance.GetModel(randIndex);

            if (template == null)
            {
                Debug.LogWarning($"Could not load ped model {randIndex}.");
                yield break;
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
