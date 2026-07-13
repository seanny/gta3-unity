namespace GTA3Unity.Ipl
{
    public sealed class ItemDefinition
    {
        public int Id => m_Id;
        public string DffName => m_DffName;
        public string TxdName => m_TxdName;

        private readonly int m_Id;
        private readonly string m_DffName;
        private readonly string m_TxdName;

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
