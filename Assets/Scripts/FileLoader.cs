using GTA3Unity.Ipl;
using UnityEngine;

namespace GTA3Unity
{
    public sealed class FileLoader: MonoBehaviour
    {
        // Temporary varaible to test IPL file loading
        [SerializeField] private string m_IplFile;

        private void Start()
        {
            IplFile.LoadIplFile(m_IplFile);
        }
    }
}