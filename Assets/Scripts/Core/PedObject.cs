using System;
using UnityEngine;

namespace GTA3Unity.Core
{
    public enum EPedState
    {
        OnFoot,
        Driving,
        Passenger,
        Dead
    };

    public class PedObject : GtaObject
    {
        public EPedState PedState => m_PedState;

        [SerializeField] protected EPedState m_PedState = EPedState.OnFoot;

        public void SetPedState(EPedState pedState)
        {
            m_PedState = pedState;
        }

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
    }
}
