using System.IO;
using UnityEngine;

namespace GTA3Unity.Ipl
{
    public enum EOBJType
    {
        NonBreakable,
        Breakable,
        ComplexBreakable
    };

    public enum EObjectTypes
    {
        None,
        Objs,
        Mlo,
		TObj,
		Hier,
		Cars,
		Peds,
		Path,
		FX2D, // "2DFX"
        End
    }

    public static class IdeFile
    {
        public static void LoadIdeFile(string pathToIdeFile)
        {
            EObjectTypes section = EObjectTypes.None;

            string[] lines = File.ReadAllLines(pathToIdeFile);
            foreach(string line in lines)
            {
                if(line.StartsWith('#'))
                {
                    // Ignore lines that begin with # as these are comments
                    continue;
                }

                if(line == "end")
                {
                    section = EObjectTypes.None;
                }

                if(section == EObjectTypes.None)
                {
                    if(line == "objs")
                    {
                        section = EObjectTypes.Objs;
                    }
                    else if(line == "tobj")
                    {
                        section = EObjectTypes.TObj;
                    }
                    else if(line == "hier")
                    {
                        section = EObjectTypes.Hier;
                    }
                    else if(line == "cars")
                    {
                        section = EObjectTypes.Cars;
                    }
                    else if(line == "peds")
                    {
                        section = EObjectTypes.Peds;
                    }
                    else if(line == "path")
                    {
                        section = EObjectTypes.Path;
                    }
                    else if(line == "2dfx")
                    {
                        section = EObjectTypes.FX2D;
                    }
                    continue;
                }

                switch(section)
                {
                    case EObjectTypes.Objs:
                        LoadObject(line);
                        break;
                }
            }
        }

        public static void LoadObject(string line)
        {
            int id;
            EOBJType objType;
            string model, texture;
            float[] dist = new float[3];
            //uint flags; // Are flags really neccesary since Unity can probably do most of this out-of-the-box?
            int damaged;

            //Debug.Log(line);
            string[] parts = line.Split(", ");
            id = int.Parse(parts[0]);
            model = parts[1];
            texture = parts[2];
            objType = (EOBJType)int.Parse(parts[3]);

            switch(objType)
            {
                case EOBJType.NonBreakable:
                    dist[0] = float.Parse(parts[4]);
                    //flags = uint.Parse(parts[5]);
                    Debug.Log($"{objType}: #{id} Model: {model} Texture: {texture}");
                    break;
                case EOBJType.Breakable:
                    dist[0] = float.Parse(parts[4]);
                    dist[1] = float.Parse(parts[5]);
                    damaged = dist[0] < dist[1] ? 0 : 1;
                    //flags = uint.Parse(parts[6]);
                    Debug.Log($"{objType}: #{id} Model: {model} Texture: {texture} Distance: {dist[0]}, {dist[1]} Damaged: {damaged}");
                    break;
                case EOBJType.ComplexBreakable:
                    dist[0] = float.Parse(parts[4]);
                    dist[1] = float.Parse(parts[5]);
                    dist[2] = float.Parse(parts[6]);
                    damaged = dist[0] < dist[1] ? (dist[1] < dist[2] ? 0 : 2) : 1;
                    //flags = uint.Parse(parts[7]);
                    Debug.Log($"{objType}: #{id} Model: {model} Texture: {texture} Distance: {dist[0]} {dist[1]}, {dist[2]} Damaged: {damaged}");
                    break;
                default:
                    Debug.LogWarning("Unknown type");
                    break;
            }

            ItemDefinition itemDefinition = new ItemDefinition(id, model, texture);
            DatManifest.ItemDefinitions.Add(itemDefinition);
        }
    }
}