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
using Unity.Burst.Intrinsics;
using RenderWareIo.Structs.Ide;
using RenderWareIo.Structs.Ifp;
using Unity.AI.Navigation;

namespace GTA3Unity
{
    public sealed class FileLoader : MonoBehaviour
    {
        public static FileLoader Instance { get; private set; }

        public bool IsDone => m_IsDone;
        public ImgFile MainImg => m_MainImg;

        [SerializeField] private NavMeshSurface m_Surface;

        [SerializeField]
        private List<RenderWareIo.DatFile> m_DatFiles = new();

        [SerializeField]
        private List<IdeObj> m_Objects = new();

        [SerializeField]
        private List<RenderWareIo.Structs.Ide.Car> m_Cars = new();
        [SerializeField]
        private List<RenderWareIo.Structs.Ide.Ped> m_Peds = new();

        private Dictionary<int, GameObject> m_LoadedModels = new();

        private ImgFile m_MainImg;
        private Material m_FallbackMaterial;
        private TxdMaterialCache m_TxdMaterialCache;
        private IfpFile m_PedIfpFile;
        private bool m_IsDone;
        [SerializeField] private bool m_PrintAnims = false;

        void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
        }

        public GameObject GetModel(int modelIndex)
        {
            if(m_LoadedModels.ContainsKey(modelIndex))
            {
                return m_LoadedModels[modelIndex];
            }

            // Load model into memory for use
            foreach(var ideObject in m_Peds)
            {
                if(ideObject.Id == modelIndex)
                {
                    return MeshSpawn.GetOrCreateTemplate<Ped>(modelIndex, ideObject, m_MainImg, m_FallbackMaterial, m_TxdMaterialCache);
                }
            }
            return null;
        }

        public bool TryGetRandomPedModelIndex(out int modelIndex)
        {
            List<int> candidates = new();

            foreach (RenderWareIo.Structs.Ide.Ped ped in m_Peds)
            {
                if (ped.Id == 0 ||
                    ped.Id >= 26 && ped.Id <= 29 ||
                    string.IsNullOrEmpty(ped.ModelName))
                {
                    continue;
                }

                if (m_MainImg != null && m_MainImg.Contains($"{ped.ModelName}.dff"))
                {
                    candidates.Add(ped.Id);
                }
            }

            if (candidates.Count == 0)
            {
                modelIndex = -1;
                return false;
            }

            modelIndex = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            return true;
        }

        public bool PlayPedAnimation(
            GameObject pedModel,
            string preferredAnimationName = "idle_stance")
        {
            if (pedModel == null || m_PedIfpFile?.Ifp?.Animations == null)
            {
                return false;
            }

            IfpAnimation animation = FindPedAnimation(preferredAnimationName);

            if (animation == null)
            {
                Debug.LogWarning($"Could not find a usable ped animation in ped.ifp.");
                return false;
            }

            AnimationClip clip;

            try
            {
                clip = IfpAnimationConverter.CreateLegacyClip(
                    animation,
                    pedModel.transform);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to build ped animation '{animation.Name}': {exception.Message}");
                return false;
            }

            Animation animationComponent = pedModel.GetComponent<Animation>();

            if (animationComponent == null)
            {
                animationComponent = pedModel.AddComponent<Animation>();
            }

            animationComponent.AddClip(clip, clip.name);
            animationComponent.clip = clip;
            animationComponent.wrapMode = WrapMode.Loop;
            animationComponent.Play(clip.name);
            return true;
        }

        public void Init()
        {
            if (m_IsDone == true)
            {
                return;
            }

            m_IsDone = true;
            Debug.Log("FileLoader.Init begin");
            m_FallbackMaterial = Resources.Load<Material>("TestMaterial");
            m_MainImg = new ImgFile(Path.Combine(GameManager.Instance.GtaDirectory, "models", "gta3.img"));
            m_TxdMaterialCache = new TxdMaterialCache(m_MainImg, m_FallbackMaterial);
            m_TxdMaterialCache.RegisterLooseTxdDirectory(
                Path.Combine(GameManager.Instance.GtaDirectory, "models"));
            m_TxdMaterialCache.RegisterTxdFile(
                "generic",
                Path.Combine(GameManager.Instance.GtaDirectory, "models", "generic.txd"));
            LoadPedAnimations();
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
                    m_TxdMaterialCache.RegisterTxdParents(ideFile.Ide.Txdps);
                    m_Peds.AddRange(ideFile.Ide.Peds);
                }
            }

            LoadWorldMap();
        }

        public void LoadWorldMap()
        {
            if(m_IsDone == false)
            {
                return;
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
                        if(inst.ModelName.Contains("LOD", StringComparison.InvariantCultureIgnoreCase))
                        {
                            // Ignore LOD's
                            continue;
                        }

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

            loadTimer = System.Diagnostics.Stopwatch.StartNew();
            m_Surface.BuildNavMesh();
            loadTimer.Stop();
            Debug.Log($"Created navmesh in {loadTimer.ElapsedMilliseconds} ms.");
        }

        private void LoadPedAnimations()
        {
            string pedIfpPath = Path.Combine(
                GameManager.Instance.GtaDirectory,
                "anim",
                "ped.ifp");

            if (!File.Exists(pedIfpPath))
            {
                Debug.LogWarning($"Could not find ped animation file '{pedIfpPath}'.");
                return;
            }

            try
            {
                m_PedIfpFile = new IfpFile(pedIfpPath);
                Debug.Log($"Loaded {m_PedIfpFile.Ifp.Animations.Count} ped animations from '{pedIfpPath}'.");
                if(m_PrintAnims)
                {
                    string animList = "==== ANIM LIST ====\n";
                    for(int i = 0; i < m_PedIfpFile.Ifp.Animations.Count; i++)
                    {
                        animList += m_PedIfpFile.Ifp.Animations[i].Name + "\n";
                    }
                    Debug.Log(animList);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to load ped animations from '{pedIfpPath}': {exception.Message}");
                m_PedIfpFile = null;
            }
        }

        private IfpAnimation FindPedAnimation(string preferredAnimationName)
        {
            IfpAnimation fallback = null;

            foreach (IfpAnimation animation in m_PedIfpFile.Ifp.Animations)
            {
                if (animation.Objects == null || animation.Objects.Count == 0)
                {
                    continue;
                }

                fallback ??= animation;

                if (!string.IsNullOrEmpty(preferredAnimationName) &&
                    string.Equals(
                        animation.Name,
                        preferredAnimationName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return animation;
                }
            }

            return fallback;
        }

        public void PrintPedAnimations()
        {
            string anims = string.Empty;
            foreach (IfpAnimation animation in m_PedIfpFile.Ifp.Animations)
            {
                if (animation.Objects == null || animation.Objects.Count == 0)
                {
                    continue;
                }

                anims += animation.Name + "\n";
            }
        }
    }
}
