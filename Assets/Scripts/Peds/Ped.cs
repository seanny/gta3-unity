using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

namespace GTA3Unity.Peds
{
    public class Ped: MonoBehaviour
    {
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

            m_PedModel = GameObject.Instantiate<GameObject>(FileLoader.Instance.GetModel(randIndex));
        }
    }
}