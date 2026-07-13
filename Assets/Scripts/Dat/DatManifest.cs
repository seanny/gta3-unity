using System.Collections.Generic;
using GTA3Unity.Ipl;

namespace GTA3Unity.Dat
{
    public static class DatManifest
    {
        private static readonly List<string> s_IdeFiles = new();
        private static readonly List<string> s_IplFiles = new();
        private static readonly List<string> s_ImgFiles = new();
        private static readonly List<string> s_ColFiles = new();
        private static readonly List<ItemDefinition> s_ItemDefinitions = new();

        public static IReadOnlyList<string> IdeFiles => s_IdeFiles;
        public static IReadOnlyList<string> IplFiles => s_IplFiles;
        public static IReadOnlyList<string> ImgFiles => s_ImgFiles;
        public static IReadOnlyList<string> ColFiles => s_ColFiles;
        public static IReadOnlyList<ItemDefinition> ItemDefinitions => s_ItemDefinitions;

        public static void AddIdeFile(string filePath)
        {
            s_IdeFiles.Add(filePath);
        }

        public static void AddIplFile(string filePath)
        {
            s_IplFiles.Add(filePath);
        }

        public static void AddItemDefinition(ItemDefinition itemDefinition)
        {
            s_ItemDefinitions.Add(itemDefinition);
        }
    }
}
