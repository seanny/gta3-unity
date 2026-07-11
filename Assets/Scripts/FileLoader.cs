using GTA3Unity.Img;
using GTA3Unity.Ipl;
using UnityEngine;

namespace GTA3Unity
{
    public sealed class FileLoader: MonoBehaviour
    {
        // Temporary varaible to test IPL file loading
        [SerializeField] private string m_IplFile;
        [SerializeField] private string m_ImgFile;

        private void Start()
        {
            ImgFile imgFile = new ImgFile(m_ImgFile);
            // TODO: I want to be able to read all txd and dff files inside of the img archive.
            
            IplFile.LoadIplFile(m_IplFile);
        }
    }
}