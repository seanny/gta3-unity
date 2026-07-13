using System.IO;
using GTA3Unity.Dat;
using GTA3Unity.Ipl;
using UnityEngine;

namespace GTA3Unity.Dat
{
    public static class DatFile
    {
        public static void LoadDatFile(string pathToDatFile)
        {
            if (!File.Exists(pathToDatFile))
            {
                Debug.LogError($"{nameof(DatFile)} failed to load '{pathToDatFile}': file does not exist.");
                return;
            }

            string gtaFolder = Directory.GetParent(new FileInfo(pathToDatFile).Directory.FullName).FullName;
            if (!Directory.Exists(gtaFolder))
            {
                Debug.LogError($"{nameof(DatFile)} failed to load '{pathToDatFile}': GTA directory was not found.");
                return;
            }

            string[] lines = File.ReadAllLines(pathToDatFile);
            foreach (string line in lines)
            {
                if (line.StartsWith('#'))
                {
                    continue;
                }

                if (line.StartsWith("IDE"))
                {
                    string pathToIdeFile = line.Substring(4);
                    string finalIdePath = Path.Combine(gtaFolder, pathToIdeFile);
                    DatManifest.AddIdeFile(finalIdePath);
                    IdeFile.LoadIdeFile(finalIdePath);
                }
                else if (line.StartsWith("IPL"))
                {
                    string pathToIplFile = line.Substring(4);
                    string finalIplPath = Path.Combine(gtaFolder, pathToIplFile);
                    DatManifest.AddIplFile(finalIplPath);
                    IplFile.LoadIplFile(finalIplPath);
                }
            }
        }
    }
}
