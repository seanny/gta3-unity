using System.IO;
using GTA3Unity.Dat;
using UnityEngine;

namespace GTA3Unity.Ipl
{
    public enum EObjectBreakability
    {
        None = 0,
        NonBreakable,
        Breakable,
        ComplexBreakable
    }

    public enum EIdeSection
    {
        None = 0,
        Objs,
        Mlo,
        TObj,
        Hier,
        Cars,
        Peds,
        Path,
        TwoDfx,
        End
    }

    public static class IdeFile
    {
        public static void LoadIdeFile(string pathToIdeFile)
        {
            EIdeSection section = EIdeSection.None;

            string[] lines = File.ReadAllLines(pathToIdeFile);
            foreach (string line in lines)
            {
                if (line.StartsWith('#'))
                {
                    continue;
                }

                if (line == "end")
                {
                    section = EIdeSection.None;
                }

                if (section == EIdeSection.None)
                {
                    if (line == "objs")
                    {
                        section = EIdeSection.Objs;
                    }
                    else if (line == "tobj")
                    {
                        section = EIdeSection.TObj;
                    }
                    else if (line == "hier")
                    {
                        section = EIdeSection.Hier;
                    }
                    else if (line == "cars")
                    {
                        section = EIdeSection.Cars;
                    }
                    else if (line == "peds")
                    {
                        section = EIdeSection.Peds;
                    }
                    else if (line == "path")
                    {
                        section = EIdeSection.Path;
                    }
                    else if (line == "2dfx")
                    {
                        section = EIdeSection.TwoDfx;
                    }

                    continue;
                }

                switch (section)
                {
                    case EIdeSection.Objs:
                        LoadObject(line);
                        break;
                }
            }
        }

        public static void LoadObject(string line)
        {
            int id;
            EObjectBreakability objectBreakability;
            string model, texture;
            float[] distances = new float[3];
            int damaged;

            string[] parts = line.Split(", ");
            id = int.Parse(parts[0]);
            model = parts[1];
            texture = parts[2];
            objectBreakability = (EObjectBreakability)int.Parse(parts[3]);

            switch (objectBreakability)
            {
                case EObjectBreakability.NonBreakable:
                    distances[0] = float.Parse(parts[4]);
                    break;

                case EObjectBreakability.Breakable:
                    distances[0] = float.Parse(parts[4]);
                    distances[1] = float.Parse(parts[5]);
                    damaged = distances[0] < distances[1] ? 0 : 1;
                    break;

                case EObjectBreakability.ComplexBreakable:
                    distances[0] = float.Parse(parts[4]);
                    distances[1] = float.Parse(parts[5]);
                    distances[2] = float.Parse(parts[6]);
                    damaged = distances[0] < distances[1] ? (distances[1] < distances[2] ? 0 : 2) : 1;
                    break;

                default:
                    Debug.LogWarning($"Unknown IDE object breakability '{objectBreakability}' for line '{line}'.");
                    break;
            }

            ItemDefinition itemDefinition = new ItemDefinition(id, model, texture);
            DatManifest.AddItemDefinition(itemDefinition);
        }
    }
}
