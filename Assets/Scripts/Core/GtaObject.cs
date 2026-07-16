using UnityEngine;

namespace GTA3Unity.Core
{
    public class GtaObject : MonoBehaviour
    {
        private static readonly Vector3 s_ModelBasisPosition = new Vector3(0, 1.0f, 0.0f);
        private static readonly Quaternion s_ModelBasisRotation = Quaternion.Euler(-90.0f, 180.0f, 0.0f);

        protected GameObject m_PedModel;

        public bool PlayAnimation(
            string animName,
            float fadeLength = 0.15f,
            WrapMode wrapMode = WrapMode.Loop,
            bool makeInPlace = false)
        {
            if(m_PedModel == null)
            {
                Debug.LogError($"Ped {name} does not have a PedModel attached.");
                return false;
            }

            return FileLoader.Instance.PlayPedAnimation(
                m_PedModel,
                animName,
                fadeLength,
                wrapMode,
                makeInPlace);
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
            m_PedModel.transform.localPosition = s_ModelBasisPosition;
            m_PedModel.transform.localRotation = s_ModelBasisRotation;
            m_PedModel.transform.localScale = Vector3.one;
            m_PedModel.SetActive(true);
            FileLoader.Instance.PlayPedAnimation(m_PedModel);
        }
    }
}
