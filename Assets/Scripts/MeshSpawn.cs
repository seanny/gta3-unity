using System;
using RenderWareIo;
using UnityEngine;
using ImgFile = GTA3Unity.Img.ImgFile;

namespace GTA3Unity
{
    public static class MeshSpawn
    {
        public static void SpawnMesh(RenderWareIo.Structs.Ide.Obj meshObj, Vector3 position, Quaternion rotation, ImgFile imgFileToReadFrom, Material fallbackMaterial)
        {
            string dffName = $"{meshObj.ModelName}.dff";

            if (!imgFileToReadFrom.Contains(dffName))
            {
                Debug.LogWarning($"Could not find DFF '{meshObj.ModelName}'.");
                return;
            }

            var entry = imgFileToReadFrom[dffName];
            byte[] bytes = entry.GetData();
            Debug.Log($"Loading DFF '{entry.FileNameWithoutExtension}', {bytes.Length} bytes.");

            try
            {
                DffFile dffFile = new DffFile(bytes);
                DffMeshConverter.SpawnDff(dffFile, meshObj.ModelName, position, rotation, fallbackMaterial);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to spawn DFF '{meshObj.ModelName}': {exception.Message}");
            }
        }
    }
}