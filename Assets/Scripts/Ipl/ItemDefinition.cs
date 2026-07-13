using System.Collections.Generic;
using UnityEngine;
using RenderWareIo;
using RenderWareIo.Structs.Dff;

namespace GTA3Unity.Ipl
{
    public sealed class ItemDefinition
    {
        public int Id => m_Id;
        public string DffName => m_DffName;
        public string TxdName => m_TxdName;

        private int m_Id;
        private string m_DffName;
        private string m_TxdName;

        private GameObject m_LoadedObject;

        private byte[] m_DffBytes;
        private byte[] m_TxdBytes;

        public ItemDefinition(int id, string dffName, string txdName)
        {
            m_Id = id;
            m_DffName = dffName;
            m_TxdName = txdName;
        }

        public void LoadBytes()
        {
        }
    }
}