using System;
using System.Collections.Generic;
using System.IO;
using GTA3Unity.Utility;
using RenderWareIo;
using UnityEngine;
using Material = UnityEngine.Material;
using IdeObj = RenderWareIo.Structs.Ide.Obj;
using ImgFile = GTA3Unity.Img.ImgFile;
using GTA3Unity.Core;

namespace GTA3Unity
{
    public sealed class FileLoader : MonoBehaviour
    {
        public static FileLoader Instance { get; private set; }

        public ImgFile MainImg => m_MainImg;

        [SerializeField]
        private List<RenderWareIo.DatFile> m_DatFiles = new();

        [SerializeField]
        private List<IdeObj> m_Objects = new();

        [SerializeField]
        private List<RenderWareIo.Structs.Ide.Car> m_Cars = new();

        private ImgFile m_MainImg;
        private Material m_FallbackMaterial;
        private TxdMaterialCache m_TxdMaterialCache;

        void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
        }

        public void Init()
        {
            m_FallbackMaterial = Resources.Load<Material>("TestMaterial");
            m_MainImg = new ImgFile(Path.Combine(GameManager.Instance.GtaDirectory, "models", "gta3.img"));
            m_TxdMaterialCache = new TxdMaterialCache(m_MainImg, m_FallbackMaterial);
            m_DatFiles.Add(new(Path.Combine(GameManager.Instance.GtaDirectory, "data", "default.dat")));
            m_DatFiles.Add(new(Path.Combine(GameManager.Instance.GtaDirectory, "data", "gta3.dat")));
            MeshSpawn.ClearCache();

            foreach (RenderWareIo.DatFile dat in m_DatFiles)
            {
                foreach (string ide in dat.Dat.Ides)
                {
                    string path = Path.Combine(GameManager.Instance.GtaDirectory, StringExt.ReplaceInvalidSlash(ide));
                    IdeFile ideFile = new(path);
                    m_Objects.AddRange(ideFile.Ide.Objs);
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
            System.Diagnostics.Stopwatch loadTimer = System.Diagnostics.Stopwatch.StartNew();

            foreach (RenderWareIo.DatFile dat in m_DatFiles)
            {
                foreach (string ipl in dat.Dat.Ipls)
                {
                    string path = Path.Combine(GameManager.Instance.GtaDirectory, StringExt.ReplaceInvalidSlash(ipl));
                    IplFile iplFile = new(path);
                    foreach (RenderWareIo.Structs.Ipl.Inst inst in iplFile.Ipl.Insts)
                    {
                        instanceCount++;

                        if (!objectsById.TryGetValue(inst.Id, out IdeObj meshObj) &&
                            !objectsByModelName.TryGetValue(inst.ModelName, out meshObj))
                        {
                            missingDefinitionCount++;
                            continue;
                        }

                        Vector3 position = new Vector3(inst.Position.X, inst.Position.Y, inst.Position.Z);
                        Quaternion rotation = new Quaternion(
                            inst.Rotation.X,
                            inst.Rotation.Y,
                            inst.Rotation.Z,
                            inst.Rotation.W);

                        if (MeshSpawn.SpawnMesh(
                                meshObj,
                                position,
                                rotation,
                                m_MainImg,
                                m_FallbackMaterial,
                                m_TxdMaterialCache))
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
