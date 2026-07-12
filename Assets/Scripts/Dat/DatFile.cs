using System.IO;
using GTA3Unity.Ipl;
using UnityEngine;

namespace GTA3Unity
{
    public static class DatFile
    {
        public static void LoadDatFile(string pathToDatFile)
        {
            if(!File.Exists(pathToDatFile))
            {
                Debug.LogError($"LoadDatFile: Failed to load \"{pathToDatFile}\": Does not exist");
                return;
            }

            string gtaFolder = Directory.GetParent(new FileInfo(pathToDatFile).Directory.FullName).FullName;
            if(!Directory.Exists(gtaFolder))
            {
                // This shouldn't happen, but a check anyway to prevent edge cases causing problems
                Debug.LogError($"LoadDatFile: Failed to load \"{pathToDatFile}\": Could not get file directory path");
                return;
            }

            var lines = File.ReadAllLines(pathToDatFile);
            foreach(var line in lines)
            {
                if(line.StartsWith('#'))
                {
                    // Lines starting with # are comments
                    continue;
                }

                if(line.StartsWith("IPL"))
                {
                    string pathToIplFile = line.Substring(4);
                    IplFile.LoadIplFile(Path.Combine(gtaFolder, pathToIplFile));
                }
            }
        }
    }
}