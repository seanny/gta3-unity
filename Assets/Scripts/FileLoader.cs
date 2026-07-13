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
using IdeObj = RenderWareIo.Structs.Ide.Obj;

namespace GTA3Unity
{
    public sealed class FileLoader : MonoBehaviour
    {
        public ImgFile MainImg => m_MainImg;

        // Temporary varaible to test IPL file loading
        [SerializeField] private string m_GtaDirectory;

        private ImgFile m_MainImg;
        [SerializeField] private List<RenderWareIo.DatFile> m_DatFiles = new();
        [SerializeField] private List<IdeObj> m_Objects = new();
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
            MeshSpawn.ClearCache();

            foreach (var dat in m_DatFiles)
            {
                foreach (var ide in dat.Dat.Ides)
                {
                    string path = Path.Combine(m_GtaDirectory, StringExt.ReplaceInvalidSlash(ide)); // We normalise the path before passing it to IdeFile ctor
                    IdeFile ideFile = new(path);
                    m_Objects.AddRange(ideFile.Ide.Objs);
                    //m_Cars.AddRange(ideFile.Ide.Cars);
                }
            }

            Dictionary<int, IdeObj> objectsById = new();
            Dictionary<string, IdeObj> objectsByModelName = new(StringComparer.OrdinalIgnoreCase);

            foreach (IdeObj obj in m_Objects)
            {
                objectsById[obj.Id] = obj;
                objectsByModelName[obj.ModelName] = obj;
            }

            int instanceCount = 0;
            int spawnedCount = 0;
            int missingDefinitionCount = 0;
            var loadTimer = System.Diagnostics.Stopwatch.StartNew();

            foreach (var dat in m_DatFiles)
            {
                foreach(var ipl in dat.Dat.Ipls)
                {
                    string path = Path.Combine(m_GtaDirectory, StringExt.ReplaceInvalidSlash(ipl));
                    IplFile iplFile = new(path);
                    foreach(var inst in iplFile.Ipl.Insts)
                    {
                        instanceCount++;

                        if (!objectsById.TryGetValue(inst.Id, out IdeObj meshObj) &&
                            !objectsByModelName.TryGetValue(inst.ModelName, out meshObj))
                        {
                            missingDefinitionCount++;
                            continue;
                        }

                        if (MeshSpawn.SpawnMesh(meshObj, new Vector3(inst.Position.X, inst.Position.Y, inst.Position.Z), new Quaternion(inst.Rotation.X,inst.Rotation.Y,inst.Rotation.Z,inst.Rotation.W), m_MainImg, m_FallbackMaterial, m_TxdMaterialCache))
                        {
                            spawnedCount++;
                        }
                    }
                }
            }

            loadTimer.Stop();
            Debug.Log(
                $"Loaded {spawnedCount}/{instanceCount} IPL instances in {loadTimer.ElapsedMilliseconds} ms. " +
                $"IDE objects={m_Objects.Count}, missing definitions={missingDefinitionCount}.");
            MeshSpawn.LogStats();
            m_TxdMaterialCache.LogStats();
        }
    }
}
