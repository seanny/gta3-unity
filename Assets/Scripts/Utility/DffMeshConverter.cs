using System;
using System.Collections.Generic;
using System.Linq;
using GTA3Unity.Utility;
using RenderWareIo;
using RenderWareIo.Structs.Dff;
using UnityEngine;
using UnityEngine.Rendering;

using RwVector3 = System.Numerics.Vector3;

public static class DffMeshConverter
{
    /// <summary>
    /// Converts one RenderWare geometry into a Unity mesh.
    ///
    /// Coordinate conversion:
    ///     RenderWare X -> Unity X
    ///     RenderWare Y -> Unity Z
    ///     RenderWare Z -> Unity Y
    ///
    /// RenderWare triangle indices are emitted in Unity front-face order after
    /// the coordinate conversion below.
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

        // Fall back to the first target containing vertices.
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

        // Unity meshes use 16-bit indices by default.
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
                normals[i] = ConvertVector(morphTarget.Normals[i]).normalized;
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
                    flipTextureV ? 1.0f - sourceUv.Y : sourceUv.Y);
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

            if(flipTextureV)
            {
                indices.Add(indexA);
                indices.Add(indexB);
                indices.Add(indexC);
            }
            else
            {
                indices.Add(indexA);
                indices.Add(indexC);
                indices.Add(indexB);
            }
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

    private static Vector3 ConvertVector(RwVector3 vector)
    {
        return new Vector3(
            vector.X,
            vector.Z,
            vector.Y);
    }

    private static GameObject CreateDffGameObject(
    DffFile dffFile,
    string modelName,
    string txdName,
    TxdMaterialCache materialCache,
    UnityEngine.Material fallbackMaterial)
    {
        var clump = dffFile.Dff?.Clump;

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

        for (int geometryIndex = 0;
             geometryIndex < clump.GeometryList.Geometries.Count;
             geometryIndex++)
        {
            var geometry =
                clump.GeometryList.Geometries[geometryIndex];

            Mesh mesh;

            try
            {
                mesh = DffMeshConverter.CreateMesh(
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

            geometryObject.transform.SetParent(
                rootObject.transform,
                worldPositionStays: false);

            geometryObject.transform.localPosition = Vector3.zero;
            geometryObject.transform.localRotation = Quaternion.identity;
            geometryObject.transform.localScale = Vector3.one;

            MeshFilter meshFilter =
                geometryObject.AddComponent<MeshFilter>();

            MeshRenderer meshRenderer =
                geometryObject.AddComponent<MeshRenderer>();

            meshFilter.sharedMesh = mesh;

            UnityEngine.Material[] materials = materialCache != null
                ? materialCache.CreateMaterials(txdName, geometry)
                : CreateFallbackMaterials(mesh.subMeshCount, fallbackMaterial);

            meshRenderer.sharedMaterials = materials;

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
