using System;
using System.Collections.Generic;
using GTA3Unity.Img;
using GTA3Unity.Ipl;
using UnityEngine;
using System.IO;

namespace GTA3Unity
{
    public sealed class FileLoader: MonoBehaviour
    {
        // Temporary varaible to test IPL file loading
        [SerializeField] private string m_GtaDirectory;

        private ImgFile m_MainImg;

        private void Start()
        {
            m_MainImg = new ImgFile(Path.Combine(m_GtaDirectory, "models", "gta3.img"));
            DatFile.LoadDatFile(Path.Combine(m_GtaDirectory, "data", "default.dat"));
            DatFile.LoadDatFile(Path.Combine(m_GtaDirectory, "data", "gta3.dat"));
        }
    }
}
