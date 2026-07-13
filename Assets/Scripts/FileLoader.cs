using System;
using System.Collections.Generic;
using GTA3Unity.Img;
using UnityEngine;
using System.IO;
using RenderWareIo;
using ImgFile = GTA3Unity.Img.ImgFile;
//using IplFile = GTA3Unity.Ipl.IplFile;
using GTA3Unity.Utility;
using Material = UnityEngine.Material;

namespace GTA3Unity
{
    public sealed class FileLoader : MonoBehaviour
    {
        public ImgFile MainImg => m_MainImg;

        // Temporary varaible to test IPL file loading
        [SerializeField] private string m_GtaDirectory;

        private ImgFile m_MainImg;
        [SerializeField] private List<RenderWareIo.DatFile> m_DatFiles = new();
        [SerializeField] private List<RenderWareIo.Structs.Ide.Obj> m_Objects = new();
        [SerializeField] private List<RenderWareIo.Structs.Ide.Car> m_Cars = new();

        private Material m_FallbackMaterial;
        private TxdMaterialCache m_TxdMaterialCache;

        private void Start()
        {
            m_FallbackMaterial = Resources.Load<Material>("TestMaterial");
            m_MainImg = new ImgFile(Path.Combine(m_GtaDirectory, "models", "gta3.img"));
            m_TxdMaterialCache = new TxdMaterialCache(m_MainImg, m_FallbackMaterial);
            m_DatFiles.Add(new(Path.Combine(m_GtaDirectory, "data", "default.dat")));
            m_DatFiles.Add(new(Path.Combine(m_GtaDirectory, "data", "gta3.dat")));

            foreach (var dat in m_DatFiles)
            {
                foreach (var ide in dat.Dat.Ides)
                {
                    string path = Path.Combine(m_GtaDirectory, StringExt.ReplaceInvalidSlash(ide)); // We normalise the path before passing it to IdeFile ctor
                    Debug.Log(path);
                    IdeFile ideFile = new(path);
                    m_Objects.AddRange(ideFile.Ide.Objs);
                    //m_Cars.AddRange(ideFile.Ide.Cars);
                }

                foreach(var ipl in dat.Dat.Ipls)
                {
                    string path = Path.Combine(m_GtaDirectory, StringExt.ReplaceInvalidSlash(ipl));
                    Debug.Log(path);
                    IplFile iplFile = new(path);
                    foreach(var inst in iplFile.Ipl.Insts)
                    {
                        Debug.Log(inst.Write());
                        foreach(var meshObj in m_Objects)
                        {
                            if(meshObj.ModelName != inst.ModelName)
                            {
                                continue;
                            }
                            Debug.Log($"Spawning {inst.ModelName}: [X:{inst.Position.X} Y:{inst.Position.Y} Z:{inst.Position.Z}]");
                            MeshSpawn.SpawnMesh(meshObj, new Vector3(inst.Position.X, inst.Position.Y, inst.Position.Z), new Quaternion(inst.Rotation.X,inst.Rotation.Y,inst.Rotation.Z,inst.Rotation.W), m_MainImg, m_FallbackMaterial, m_TxdMaterialCache);
                        }
                    }
                }
            }

            m_TxdMaterialCache.LogStats();
        }
    }
}
