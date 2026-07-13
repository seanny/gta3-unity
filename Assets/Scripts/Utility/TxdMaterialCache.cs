using System;
using System.Collections.Generic;
using System.IO;
using RenderWareIo;
using RenderWareIo.Structs.Dff;
using RenderWareIo.Structs.Txd;
using UnityEngine;
using DffMaterial = RenderWareIo.Structs.Dff.Material;
using ImgFile = GTA3Unity.Img.ImgFile;
using TxdTexture = RenderWareIo.Structs.Txd.Texture;

namespace GTA3Unity.Utility
{
    public sealed class TxdMaterialCache
    {
        private readonly ImgFile m_ImgFile;
        private readonly UnityEngine.Material m_FallbackMaterial;
        private readonly Dictionary<string, TxdFile> m_TxdFiles = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, TxdTexture> m_TxdTextures = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Texture2D> m_Textures = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, UnityEngine.Material> m_Materials = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> m_Warnings = new(StringComparer.OrdinalIgnoreCase);

        private int m_TxdCacheHits;
        private int m_TxdCacheMisses;
        private int m_DecodedTextureCount;
        private int m_FallbackSlotCount;

        public TxdMaterialCache(ImgFile imgFile, UnityEngine.Material fallbackMaterial)
        {
            m_ImgFile = imgFile ?? throw new ArgumentNullException(nameof(imgFile));
            m_FallbackMaterial = fallbackMaterial;
        }

        public UnityEngine.Material[] CreateMaterials(string txdName, Geometry geometry)
        {
            int materialCount = GetMaterialCount(geometry);
            UnityEngine.Material[] materials = new UnityEngine.Material[materialCount];

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = CreateMaterial(txdName, geometry, i);
            }

            return materials;
        }

        public void LogStats()
        {
            Debug.Log(
                $"TXD material cache: txd hits={m_TxdCacheHits}, " +
                $"txd misses={m_TxdCacheMisses}, decoded textures={m_DecodedTextureCount}, " +
                $"fallback slots={m_FallbackSlotCount}.");
        }

        private UnityEngine.Material CreateMaterial(string txdName, Geometry geometry, int materialIndex)
        {
            DffMaterial dffMaterial = GetDffMaterial(geometry, materialIndex);

            if (dffMaterial == null ||
                dffMaterial.Textured <= 0 ||
                dffMaterial.Texture?.Name == null)
            {
                return Fallback($"untextured:{NormalizeName(txdName)}:{materialIndex}");
            }

            string textureName = NormalizeName(dffMaterial.Texture.Name.Value);

            if (string.IsNullOrEmpty(textureName))
            {
                return Fallback($"empty-texture-name:{NormalizeName(txdName)}:{materialIndex}");
            }

            Texture2D texture = LoadTexture(txdName, textureName);

            if (texture == null)
            {
                return Fallback($"missing-texture:{NormalizeName(txdName)}:{textureName}");
            }

            string materialKey = $"{NormalizeName(txdName)}/{textureName}";

            if (m_Materials.TryGetValue(materialKey, out UnityEngine.Material cachedMaterial))
            {
                return cachedMaterial;
            }

            UnityEngine.Material material = m_FallbackMaterial != null
                ? new UnityEngine.Material(m_FallbackMaterial)
                : new UnityEngine.Material(Shader.Find("Universal Render Pipeline/Lit"));

            material.name = materialKey;

            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", texture);
            }

            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", texture);
            }

            m_Materials[materialKey] = material;
            return material;
        }

        private Texture2D LoadTexture(string txdName, string textureName)
        {
            string txdKey = NormalizeName(txdName);

            if (string.IsNullOrEmpty(txdKey))
            {
                WarnOnce("empty-txd", "Cannot resolve texture because the TXD name is empty.");
                return null;
            }

            string textureKey = $"{txdKey}/{textureName}";

            if (m_Textures.TryGetValue(textureKey, out Texture2D cachedTexture))
            {
                return cachedTexture;
            }

            TxdTexture txdTexture = FindTxdTexture(txdKey, textureName);

            if (txdTexture?.Data == null)
            {
                return null;
            }

            Texture2D texture = DecodeTexture(textureKey, txdTexture.Data);

            if (texture == null)
            {
                return null;
            }

            m_Textures[textureKey] = texture;
            m_DecodedTextureCount++;
            return texture;
        }

        private TxdTexture FindTxdTexture(string txdName, string textureName)
        {
            string textureKey = $"{txdName}/{textureName}";

            if (m_TxdTextures.TryGetValue(textureKey, out TxdTexture cachedTexture))
            {
                return cachedTexture;
            }

            TxdFile txdFile = LoadTxd(txdName);

            if (txdFile?.Txd?.TextureContainer?.Textures == null)
            {
                return null;
            }

            foreach (TxdTexture texture in txdFile.Txd.TextureContainer.Textures)
            {
                string candidateName = NormalizeName(texture.Data?.TextureName);

                if (string.IsNullOrEmpty(candidateName))
                {
                    continue;
                }

                string candidateKey = $"{txdName}/{candidateName}";

                if (!m_TxdTextures.ContainsKey(candidateKey))
                {
                    m_TxdTextures[candidateKey] = texture;
                }
            }

            if (m_TxdTextures.TryGetValue(textureKey, out cachedTexture))
            {
                return cachedTexture;
            }

            WarnOnce(
                $"missing-texture:{textureKey}",
                $"TXD '{txdName}' does not contain texture '{textureName}'.");

            return null;
        }

        private TxdFile LoadTxd(string txdName)
        {
            if (m_TxdFiles.TryGetValue(txdName, out TxdFile cachedTxd))
            {
                m_TxdCacheHits++;
                return cachedTxd;
            }

            string fileName = $"{txdName}.txd";

            if (!m_ImgFile.Contains(fileName))
            {
                m_TxdCacheMisses++;
                WarnOnce(
                    $"missing-txd:{txdName}",
                    $"Could not find TXD '{fileName}' in gta3.img.");
                return null;
            }

            try
            {
                TxdFile txdFile = new TxdFile(m_ImgFile[fileName].GetData());
                m_TxdFiles[txdName] = txdFile;
                return txdFile;
            }
            catch (Exception exception)
            {
                WarnOnce(
                    $"bad-txd:{txdName}",
                    $"Failed to load TXD '{fileName}': {exception.Message}");
                return null;
            }
        }

        private Texture2D DecodeTexture(string textureKey, TextureData data)
        {
            if (data.Data == null ||
                data.Data.Length == 0 ||
                data.Width == 0 ||
                data.Height == 0)
            {
                WarnOnce(
                    $"empty-texture:{textureKey}",
                    $"Texture '{textureKey}' has no image data.");
                return null;
            }

            try
            {
                if (TryDecodePalettedTexture(textureKey, data, out Texture2D palettedTexture))
                {
                    return palettedTexture;
                }

                TextureFormat textureFormat;

                if (!TryGetUnityTextureFormat(data, out textureFormat))
                {
                    WarnOnce(
                        $"unsupported-format:{textureKey}:{data.TextureFormat}:{data.Depth}",
                        $"Texture '{textureKey}' has unsupported format '{data.TextureFormatString}' " +
                        $"depth {data.Depth}, data size {data.Data.Length}.");
                    return null;
                }

                Texture2D texture = new Texture2D(
                    data.Width,
                    data.Height,
                    textureFormat,
                    mipChain: false);

                texture.name = Path.GetFileName(textureKey);
                texture.LoadRawTextureData(data.Data);
                texture.Apply(updateMipmaps: false, makeNoLongerReadable: false);

                return texture;
            }
            catch (Exception exception)
            {
                WarnOnce(
                    $"decode-failed:{textureKey}",
                    $"Failed to decode texture '{textureKey}': {exception.Message}");
                return null;
            }
        }

        private bool TryDecodePalettedTexture(
            string textureKey,
            TextureData data,
            out Texture2D texture)
        {
            texture = null;

            if (data.Depth != 8 ||
                data.Pallette == null ||
                data.Pallette.Length < 256 * 4 ||
                data.Data.Length != data.Width * data.Height)
            {
                return false;
            }

            byte[] rgba = new byte[data.Data.Length * 4];

            for (int pixelIndex = 0; pixelIndex < data.Data.Length; pixelIndex++)
            {
                int paletteIndex = data.Data[pixelIndex] * 4;
                int rgbaIndex = pixelIndex * 4;

                rgba[rgbaIndex] = data.Pallette[paletteIndex + 2];
                rgba[rgbaIndex + 1] = data.Pallette[paletteIndex + 1];
                rgba[rgbaIndex + 2] = data.Pallette[paletteIndex];
                rgba[rgbaIndex + 3] = data.Pallette[paletteIndex + 3];
            }

            texture = new Texture2D(
                data.Width,
                data.Height,
                TextureFormat.RGBA32,
                mipChain: false);

            texture.name = Path.GetFileName(textureKey);
            texture.LoadRawTextureData(rgba);
            texture.Apply(updateMipmaps: false, makeNoLongerReadable: false);

            return true;
        }

        private static bool TryGetUnityTextureFormat(TextureData data, out TextureFormat textureFormat)
        {
            uint format = data.TextureFormat;

            if (format == 0 ||
                format == FourCc("DXT1"))
            {
                int dxt1Size = GetDxt1Size(data.Width, data.Height);

                if (data.Data.Length == dxt1Size)
                {
                    textureFormat = TextureFormat.DXT1;
                    return true;
                }
            }

            if (format == 21 ||
                format == 22)
            {
                if (data.Data.Length != data.Width * data.Height * 4)
                {
                    textureFormat = TextureFormat.BGRA32;
                    return false;
                }

                textureFormat = TextureFormat.BGRA32;
                return true;
            }

            textureFormat = TextureFormat.RGBA32;
            return false;
        }

        private static int GetMaterialCount(Geometry geometry)
        {
            int materialListCount = geometry?.MaterialList?.Materials != null
                ? geometry.MaterialList.Materials.Count
                : 0;

            int referencedMaterialCount = 0;

            if (geometry?.Triangles != null)
            {
                foreach (RenderWareIo.Structs.Dff.Triangle triangle in geometry.Triangles)
                {
                    referencedMaterialCount = Math.Max(
                        referencedMaterialCount,
                        triangle.MaterialIndex + 1);
                }
            }

            int materialCount = Math.Max(materialListCount, referencedMaterialCount);

            if (materialCount == 0)
            {
                return 1;
            }

            return materialCount;
        }

        private static DffMaterial GetDffMaterial(Geometry geometry, int materialIndex)
        {
            if (geometry?.MaterialList?.Materials == null ||
                materialIndex < 0 ||
                materialIndex >= geometry.MaterialList.Materials.Count)
            {
                return null;
            }

            return geometry.MaterialList.Materials[materialIndex];
        }

        private UnityEngine.Material Fallback(string reason)
        {
            m_FallbackSlotCount++;
            return m_FallbackMaterial;
        }

        private void WarnOnce(string key, string message)
        {
            if (m_Warnings.Add(key))
            {
                Debug.LogWarning(message);
            }
        }

        private static string NormalizeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string normalized = value.Replace("\0", string.Empty).Trim();
            normalized = Path.GetFileNameWithoutExtension(normalized);
            return normalized.ToLowerInvariant();
        }

        private static uint FourCc(string value)
        {
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(value);
            return BitConverter.ToUInt32(bytes, 0);
        }

        private static int GetDxt1Size(int width, int height)
        {
            int blockWidth = Math.Max(1, (width + 3) / 4);
            int blockHeight = Math.Max(1, (height + 3) / 4);
            return blockWidth * blockHeight * 8;
        }
    }
}
