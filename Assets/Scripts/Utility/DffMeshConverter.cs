using System;
using System.Collections.Generic;
using System.Linq;
using GTA3Unity.Utility;
using RenderWareIo;
using RenderWareIo.Structs.Dff;
using RenderWareIo.Structs.Dff.Plugins;
using UnityEngine;
using UnityEngine.Rendering;

using RwVector3 = System.Numerics.Vector3;

namespace GTA3Unity.Utility
{
    public static class DffMeshConverter
    {
        /// <summary>
        /// Converts one RenderWare geometry into a Unity mesh.
        /// </summary>
        public static Mesh CreateMesh(
            Geometry geometry,
            string meshName,
            bool flipTextureV = false)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException(nameof(geometry));
            }

            if (geometry.MorphTargets == null ||
                geometry.MorphTargets.Count == 0)
            {
                throw new InvalidOperationException(
                    "DFF geometry contains no morph targets.");
            }

            MorphTarget morphTarget = geometry.MorphTargets.FirstOrDefault(
                target =>
                    target.HasPosition != 0 &&
                    target.Vertices != null &&
                    target.Vertices.Count == geometry.VertexCount);

            if (morphTarget == null)
            {
                morphTarget = geometry.MorphTargets.FirstOrDefault(
                    target =>
                        target.Vertices != null &&
                        target.Vertices.Count > 0);
            }

            if (morphTarget == null)
            {
                throw new InvalidOperationException(
                    "DFF geometry contains no vertex positions.");
            }

            int vertexCount = morphTarget.Vertices.Count;

            Mesh mesh = new Mesh
            {
                name = meshName
            };

            if (vertexCount > ushort.MaxValue)
            {
                mesh.indexFormat = IndexFormat.UInt32;
            }

            Vector3[] vertices = new Vector3[vertexCount];

            for (int i = 0; i < vertexCount; i++)
            {
                vertices[i] = ConvertVector(morphTarget.Vertices[i]);
            }

            mesh.vertices = vertices;

            bool hasNormals =
                morphTarget.HasNormals != 0 &&
                morphTarget.Normals != null &&
                morphTarget.Normals.Count == vertexCount;

            if (hasNormals)
            {
                Vector3[] normals = new Vector3[vertexCount];

                for (int i = 0; i < vertexCount; i++)
                {
                    normals[i] = -ConvertVector(morphTarget.Normals[i]).normalized;
                }

                mesh.normals = normals;
            }

            bool hasUvs =
                geometry.TexCoords != null &&
                geometry.TexCoords.Count == vertexCount;

            if (hasUvs)
            {
                Vector2[] uvs = new Vector2[vertexCount];

                for (int i = 0; i < vertexCount; i++)
                {
                    Uv sourceUv = geometry.TexCoords[i];

                    uvs[i] = new Vector2(
                        sourceUv.X,
                        flipTextureV ? sourceUv.Y : 1.0f - sourceUv.Y);
                }

                mesh.uv = uvs;
            }

            if (geometry.Triangles == null ||
                geometry.Triangles.Count == 0)
            {
                throw new InvalidOperationException(
                    "DFF geometry contains no triangles.");
            }

            int declaredMaterialCount =
                geometry.MaterialList != null &&
                geometry.MaterialList.Materials != null
                    ? geometry.MaterialList.Materials.Count
                    : 0;

            int referencedMaterialCount = 0;

            foreach (RenderWareIo.Structs.Dff.Triangle triangle
                     in geometry.Triangles)
            {
                referencedMaterialCount = Math.Max(
                    referencedMaterialCount,
                    triangle.MaterialIndex + 1);
            }

            int subMeshCount = Math.Max(
                1,
                Math.Max(declaredMaterialCount, referencedMaterialCount));

            List<int>[] trianglesByMaterial =
                new List<int>[subMeshCount];

            for (int i = 0; i < subMeshCount; i++)
            {
                trianglesByMaterial[i] = new List<int>();
            }

            foreach (RenderWareIo.Structs.Dff.Triangle triangle
                     in geometry.Triangles)
            {
                int indexA = triangle.VertexIndexOne;
                int indexB = triangle.VertexIndexTwo;
                int indexC = triangle.VertexIndexThree;

                if (indexA >= vertexCount ||
                    indexB >= vertexCount ||
                    indexC >= vertexCount)
                {
                    Debug.LogWarning(
                        $"Skipping invalid DFF triangle: " +
                        $"{indexA}, {indexB}, {indexC}; " +
                        $"vertex count is {vertexCount}.");

                    continue;
                }

                int materialIndex = triangle.MaterialIndex;

                if (materialIndex < 0 ||
                    materialIndex >= trianglesByMaterial.Length)
                {
                    materialIndex = 0;
                }

                List<int> indices = trianglesByMaterial[materialIndex];

                indices.Add(indexA);
                indices.Add(indexB);
                indices.Add(indexC);
            }

            mesh.subMeshCount = subMeshCount;

            for (int i = 0; i < subMeshCount; i++)
            {
                mesh.SetTriangles(
                    trianglesByMaterial[i],
                    i,
                    calculateBounds: false);
            }

            if (!hasNormals)
            {
                mesh.RecalculateNormals();
            }

            mesh.RecalculateBounds();

            if (hasUvs)
            {
                mesh.RecalculateTangents();
            }

            return mesh;
        }

        private static Mesh CreateSkinnedMesh(
            Geometry geometry,
            SkinPlugin skin,
            string meshName,
            bool flipTextureV = false)
        {
            Mesh mesh = CreateMesh(geometry, meshName, flipTextureV);

            if (skin == null ||
                skin.VertexBoneIndices == null ||
                skin.VertexBoneWeights == null ||
                skin.VertexBoneIndices.Count != mesh.vertexCount ||
                skin.VertexBoneWeights.Count != mesh.vertexCount)
            {
                return mesh;
            }

            BoneWeight[] boneWeights = new BoneWeight[mesh.vertexCount];

            for (int vertexIndex = 0; vertexIndex < mesh.vertexCount; vertexIndex++)
            {
                byte[] indices = skin.VertexBoneIndices[vertexIndex];
                float[] weights = skin.VertexBoneWeights[vertexIndex];

                boneWeights[vertexIndex] = new BoneWeight
                {
                    boneIndex0 = indices[0],
                    boneIndex1 = indices[1],
                    boneIndex2 = indices[2],
                    boneIndex3 = indices[3],
                    weight0 = weights[0],
                    weight1 = weights[1],
                    weight2 = weights[2],
                    weight3 = weights[3]
                };
            }

            mesh.boneWeights = boneWeights;
            return mesh;
        }

        private static Vector3 ConvertVector(RwVector3 vector)
        {
            return new Vector3(
                vector.X,
                vector.Z,
                vector.Y);
        }

        private static Quaternion ConvertFrameRotation(Frame frame)
        {
            Vector3 up = ConvertVector(frame.Rot3);
            Vector3 forward = ConvertVector(frame.Rot2);

            if (up.sqrMagnitude < 0.0001f || forward.sqrMagnitude < 0.0001f)
            {
                return Quaternion.identity;
            }

            return Quaternion.LookRotation(forward.normalized, up.normalized);
        }

        private static GameObject CreateDffGameObject(
            DffFile dffFile,
            string modelName,
            string txdName,
            TxdMaterialCache materialCache,
            UnityEngine.Material fallbackMaterial)
        {
            Clump clump = dffFile.Dff?.Clump;

            if (clump?.GeometryList?.Geometries == null ||
                clump.GeometryList.Geometries.Count == 0)
            {
                Debug.LogError($"DFF '{modelName}' contains no geometry.");
                return null;
            }

            GameObject rootObject = new GameObject(modelName);

            rootObject.transform.localPosition = Vector3.zero;
            rootObject.transform.localRotation = Quaternion.identity;
            rootObject.transform.localScale = Vector3.one;

            DffFrameContext frameContext = BuildFrameHierarchy(
                rootObject,
                clump,
                HasSkinnedGeometry(clump));

            for (int geometryIndex = 0;
                 geometryIndex < clump.GeometryList.Geometries.Count;
                 geometryIndex++)
            {
                Geometry geometry =
                    clump.GeometryList.Geometries[geometryIndex];
                Atomic atomic = FindAtomicForGeometry(clump, geometryIndex);
                SkinPlugin skin = GetExtension<SkinPlugin>(geometry.Extension);

                Mesh mesh;

                try
                {
                    mesh = skin != null
                        ? CreateSkinnedMesh(
                            geometry,
                            skin,
                            $"{modelName}_Geometry_{geometryIndex}")
                        : CreateMesh(
                        geometry,
                        $"{modelName}_Geometry_{geometryIndex}");
                }
                catch (Exception exception)
                {
                    Debug.LogWarning(
                        $"Skipping geometry {geometryIndex} in '{modelName}': {exception.Message}");

                    continue;
                }

                if (mesh == null || mesh.vertexCount == 0)
                {
                    Debug.LogWarning(
                        $"Skipping empty geometry {geometryIndex} in '{modelName}'.");

                    continue;
                }

                GameObject geometryObject =
                    new GameObject($"{modelName}_Geometry_{geometryIndex}");

                Transform parent = frameContext.GetFrameTransform(atomic?.FrameIndex);
                geometryObject.transform.SetParent(
                    parent != null ? parent : rootObject.transform,
                    worldPositionStays: false);

                geometryObject.transform.localPosition = Vector3.zero;
                geometryObject.transform.localRotation = Quaternion.identity;
                geometryObject.transform.localScale = Vector3.one;

                UnityEngine.Material[] materials = materialCache != null
                    ? materialCache.CreateMaterials(txdName, geometry)
                    : CreateFallbackMaterials(mesh.subMeshCount, fallbackMaterial);

                if (skin != null && frameContext.TryGetBones(skin, out Transform[] bones))
                {
                    SkinnedMeshRenderer skinnedMeshRenderer =
                        geometryObject.AddComponent<SkinnedMeshRenderer>();

                    mesh.bindposes = CreateBindPoses(bones, geometryObject.transform);
                    skinnedMeshRenderer.sharedMesh = mesh;
                    skinnedMeshRenderer.bones = bones;
                    skinnedMeshRenderer.rootBone = bones.Length > 0 ? bones[0] : frameContext.RootFrame;
                    skinnedMeshRenderer.sharedMaterials = materials;
                    skinnedMeshRenderer.updateWhenOffscreen = true;
                }
                else
                {
                    MeshFilter meshFilter =
                        geometryObject.AddComponent<MeshFilter>();

                    MeshRenderer meshRenderer =
                        geometryObject.AddComponent<MeshRenderer>();

                    meshFilter.sharedMesh = mesh;
                    meshRenderer.sharedMaterials = materials;
                }

                geometryObject.SetActive(true);
            }

            if (rootObject.transform.childCount == 0)
            {
                DestroyGameObject(rootObject);
                Debug.LogWarning($"DFF '{modelName}' did not produce any renderable geometry.");
                return null;
            }

            rootObject.SetActive(true);

            return rootObject;
        }

        public static GameObject SpawnDff(
            DffFile dffFile,
            string modelName,
            string txdName,
            Vector3 position,
            Quaternion rotation,
            UnityEngine.Material fallbackMaterial,
            TxdMaterialCache materialCache)
        {
            GameObject instance = CreateDffGameObject(
                dffFile,
                modelName,
                txdName,
                materialCache,
                fallbackMaterial);

            if (instance == null)
            {
                return null;
            }

            instance.transform.SetPositionAndRotation(
                position,
                rotation);

            instance.transform.localScale = Vector3.one;

            Debug.Log(
                $"Spawned '{modelName}' at {position}.",
                instance);

            return instance;
        }

        public static GameObject CreateDffTemplate(
            DffFile dffFile,
            string modelName,
            string txdName,
            UnityEngine.Material fallbackMaterial,
            TxdMaterialCache materialCache)
        {
            GameObject template = CreateDffGameObject(
                dffFile,
                modelName,
                txdName,
                materialCache,
                fallbackMaterial);

            if (template == null)
            {
                return null;
            }

            template.name = $"{modelName}_Template";
            template.SetActive(false);
            return template;
        }

        public static GameObject SpawnDff(
            DffFile dffFile,
            string modelName,
            Vector3 position,
            Quaternion rotation,
            UnityEngine.Material fallbackMaterial)
        {
            return SpawnDff(
                dffFile,
                modelName,
                string.Empty,
                position,
                rotation,
                fallbackMaterial,
                materialCache: null);
        }

        private static UnityEngine.Material[] CreateFallbackMaterials(
            int materialCount,
            UnityEngine.Material fallbackMaterial)
        {
            UnityEngine.Material[] materials =
                new UnityEngine.Material[Math.Max(1, materialCount)];

            for (int materialIndex = 0;
                 materialIndex < materials.Length;
                 materialIndex++)
            {
                materials[materialIndex] = fallbackMaterial;
            }

            return materials;
        }

        private static Atomic FindAtomicForGeometry(Clump clump, int geometryIndex)
        {
            if (clump?.Atomics == null)
            {
                return null;
            }

            foreach (Atomic atomic in clump.Atomics)
            {
                if (atomic.GeometryIndex == geometryIndex)
                {
                    return atomic;
                }
            }

            return null;
        }

        private static Matrix4x4[] CreateBindPoses(Transform[] bones, Transform meshTransform)
        {
            Matrix4x4[] bindPoses = new Matrix4x4[bones.Length];

            for (int i = 0; i < bones.Length; i++)
            {
                bindPoses[i] = bones[i].worldToLocalMatrix * meshTransform.localToWorldMatrix;
            }

            return bindPoses;
        }

        private static T GetExtension<T>(Extension extension)
            where T : class, IExtensionPlugin
        {
            if (extension?.Extensions == null)
            {
                return null;
            }

            foreach (IExtensionPlugin plugin in extension.Extensions)
            {
                if (plugin is T typedPlugin)
                {
                    return typedPlugin;
                }
            }

            return null;
        }

        private static bool HasSkinnedGeometry(Clump clump)
        {
            if (clump?.GeometryList?.Geometries == null)
            {
                return false;
            }

            foreach (Geometry geometry in clump.GeometryList.Geometries)
            {
                if (GetExtension<SkinPlugin>(geometry.Extension) != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static DffFrameContext BuildFrameHierarchy(
            GameObject rootObject,
            Clump clump,
            bool applyFramePositions)
        {
            FrameList frameList = clump.FrameList;
            int frameCount = frameList?.Frames?.Count ?? 0;
            Transform[] transforms = new Transform[frameCount];

            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                string frameName = GetFrameName(frameList, frameIndex);
                GameObject frameObject = new GameObject(frameName);
                transforms[frameIndex] = frameObject.transform;
            }

            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                Frame frame = frameList.Frames[frameIndex];
                Transform parent = frame.Parent == uint.MaxValue ||
                    frame.Parent >= transforms.Length
                        ? rootObject.transform
                        : transforms[frame.Parent];

                transforms[frameIndex].SetParent(parent, worldPositionStays: false);
                transforms[frameIndex].localPosition = applyFramePositions
                    ? ConvertVector(frame.Position)
                    : Vector3.zero;
                transforms[frameIndex].localRotation = ConvertFrameRotation(frame);
                transforms[frameIndex].localScale = Vector3.one;
            }

            return new DffFrameContext(
                transforms,
                FindHierarchy(frameList));
        }

        private static string GetFrameName(FrameList frameList, int frameIndex)
        {
            FramePlugin framePlugin = frameList.Extensions != null &&
                frameIndex < frameList.Extensions.Count
                    ? GetExtension<FramePlugin>(frameList.Extensions[frameIndex])
                    : null;

            string name = framePlugin?.Value;

            if (!string.IsNullOrEmpty(name))
            {
                return name.TrimEnd('\0', ' ', '\r', '\n', '\t');
            }

            return $"Frame_{frameIndex}";
        }

        private static HAnimPlugin FindHierarchy(FrameList frameList)
        {
            if (frameList?.Extensions == null)
            {
                return null;
            }

            foreach (Extension extension in frameList.Extensions)
            {
                HAnimPlugin hierarchy = GetExtension<HAnimPlugin>(extension);

                if (hierarchy != null && hierarchy.Nodes != null && hierarchy.Nodes.Count > 0)
                {
                    return hierarchy;
                }
            }

            return null;
        }

        private sealed class DffFrameContext
        {
            private readonly Transform[] m_Frames;
            private readonly HAnimPlugin m_Hierarchy;

            public Transform RootFrame => m_Frames.Length > 0 ? m_Frames[0] : null;

            public DffFrameContext(Transform[] frames, HAnimPlugin hierarchy)
            {
                m_Frames = frames ?? Array.Empty<Transform>();
                m_Hierarchy = hierarchy;
            }

            public Transform GetFrameTransform(uint? frameIndex)
            {
                if (!frameIndex.HasValue || frameIndex.Value >= m_Frames.Length)
                {
                    return null;
                }

                return m_Frames[frameIndex.Value];
            }

            public bool TryGetBones(SkinPlugin skin, out Transform[] bones)
            {
                int boneCount = Math.Max(0, (int)skin.BoneCount);
                bones = new Transform[boneCount];

                for (int boneIndex = 0; boneIndex < boneCount; boneIndex++)
                {
                    int frameIndex = GetFrameIndexForBone(boneIndex);

                    if (frameIndex < 0 || frameIndex >= m_Frames.Length)
                    {
                        bones = null;
                        return false;
                    }

                    bones[boneIndex] = m_Frames[frameIndex];
                }

                return bones.Length > 0;
            }

            private int GetFrameIndexForBone(int boneIndex)
            {
                if (m_Hierarchy?.Nodes != null && boneIndex < m_Hierarchy.Nodes.Count)
                {
                    return m_Hierarchy.Nodes[boneIndex].NodeIndex;
                }

                return boneIndex;
            }
        }

        private static void DestroyGameObject(GameObject gameObject)
        {
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(gameObject);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }
    }
}
