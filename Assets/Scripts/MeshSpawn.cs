using System;
using System.Collections.Generic;
using GTA3Unity.Utility;
using RenderWareIo;
using UnityEngine;
using ImgFile = GTA3Unity.Img.ImgFile;

namespace GTA3Unity
{
    public static class MeshSpawn
    {
        private static readonly Dictionary<string, GameObject> s_Templates =
            new(StringComparer.OrdinalIgnoreCase);

        private static int s_TemplateHits;
        private static int s_TemplateMisses;
        private static int s_SpawnFailures;

        public static bool SpawnMesh(
            RenderWareIo.Structs.Ide.Obj meshObj,
            Vector3 position,
            Quaternion rotation,
            ImgFile imgFileToReadFrom,
            Material fallbackMaterial,
            TxdMaterialCache materialCache)
        {
            string dffName = $"{meshObj.ModelName}.dff";

            if (!imgFileToReadFrom.Contains(dffName))
            {
                Debug.LogWarning($"Could not find DFF '{meshObj.ModelName}'.");
                s_SpawnFailures++;
                return false;
            }

            string templateKey = $"{meshObj.ModelName}|{meshObj.TxdName}";

            try
            {
                GameObject template = GetOrCreateTemplate(
                    templateKey,
                    meshObj,
                    imgFileToReadFrom,
                    fallbackMaterial,
                    materialCache);

                if (template == null)
                {
                    s_SpawnFailures++;
                    return false;
                }

                GameObject instance = UnityEngine.Object.Instantiate(template);
                instance.name = meshObj.ModelName;
                instance.transform.SetPositionAndRotation(position, rotation);
                instance.transform.localScale = Vector3.one;
                instance.SetActive(true);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to spawn DFF '{meshObj.ModelName}': {exception.Message}");
                s_SpawnFailures++;
                return false;
            }
        }

        public static void ClearCache()
        {
            foreach (GameObject template in s_Templates.Values)
            {
                if (template == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(template);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(template);
                }
            }

            s_Templates.Clear();
            s_TemplateHits = 0;
            s_TemplateMisses = 0;
            s_SpawnFailures = 0;
        }

        public static void LogStats()
        {
            Debug.Log(
                $"DFF template cache: templates={s_Templates.Count}, " +
                $"hits={s_TemplateHits}, misses={s_TemplateMisses}, failures={s_SpawnFailures}.");
        }

        private static GameObject GetOrCreateTemplate(
            string templateKey,
            RenderWareIo.Structs.Ide.Obj meshObj,
            ImgFile imgFileToReadFrom,
            Material fallbackMaterial,
            TxdMaterialCache materialCache)
        {
            if (s_Templates.TryGetValue(templateKey, out GameObject template))
            {
                s_TemplateHits++;
                return template;
            }

            s_TemplateMisses++;

            string dffName = $"{meshObj.ModelName}.dff";
            var entry = imgFileToReadFrom[dffName];
            DffFile dffFile = new DffFile(entry.GetData());

            template = DffMeshConverter.CreateDffTemplate(
                dffFile,
                meshObj.ModelName,
                meshObj.TxdName,
                fallbackMaterial,
                materialCache);

            if (template != null)
            {
                s_Templates[templateKey] = template;
            }

            return template;
        }
    }
}
