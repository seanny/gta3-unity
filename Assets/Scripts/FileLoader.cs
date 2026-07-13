using System;
using System.Collections.Generic;
using GTA3Unity.Img;
using UnityEngine;
using System.IO;
using RenderWareIo.Structs.Dff;
using RenderWareIo;
using ImgFile = GTA3Unity.Img.ImgFile;
using GTA3Unity.Utility;

namespace GTA3Unity
{
    public sealed class FileLoader: MonoBehaviour
    {
        public ImgFile MainImg => m_MainImg;

        // Temporary varaible to test IPL file loading
        [SerializeField] private string m_GtaDirectory;

        private ImgFile m_MainImg;
        [SerializeField] private List<RenderWareIo.DatFile> m_DatFiles = new();
        [SerializeField] private List<RenderWareIo.Structs.Ide.Obj> m_Objects = new();
        [SerializeField] private List<RenderWareIo.Structs.Ide.Car> m_Cars = new();

        private void Start()
        {
            m_MainImg = new ImgFile(Path.Combine(m_GtaDirectory, "models", "gta3.img"));
            m_DatFiles.Add(new(Path.Combine(m_GtaDirectory, "data", "default.dat")));
            m_DatFiles.Add(new(Path.Combine(m_GtaDirectory, "data", "gta3.dat")));

            foreach(var dat in m_DatFiles)
            {
                foreach(var ide in dat.Dat.Ides)
                {
                    string path = Path.Combine(m_GtaDirectory, StringExt.ReplaceInvalidSlash(ide)); // We normalise the path before passing it to IdeFile ctor
                    Debug.Log(path);
                    IdeFile ideFile = new(path);
                    m_Objects.AddRange(ideFile.Ide.Objs);
                    m_Cars.AddRange(ideFile.Ide.Cars);
                }
            }
        }
    }
}
