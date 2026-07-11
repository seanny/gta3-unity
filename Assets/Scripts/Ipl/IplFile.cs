using UnityEngine;
using System.IO;
using Unity.VisualScripting;

namespace GTA3Unity.Ipl
{
    public static class IplFile
    {
        private enum EIPLSection
        {
            None,
            Inst,
            Zone,
            Cull,
            Pick,
            End
        }

        public static void LoadIplFile(string fileName)
        {
            if(!File.Exists(fileName))
            {
                Debug.LogError($"{nameof(IplFile)} cannot load {fileName}: File does not exist.");
                return;
            }

            EIPLSection section = EIPLSection.None;

            string[] lines = File.ReadAllLines(fileName);
            foreach(string line in lines)
            {
                if(line.StartsWith('#'))
                {
                    // Ignore lines that begin with # as these are comments
                    continue;
                }

                if(section == EIPLSection.None)
                {
                    if(line == "inst")
                    {
                        section = EIPLSection.Inst;
                    }
                    else if(line == "zone")
                    {
                        section = EIPLSection.Zone;
                    }
                    else if(line == "cull")
                    {
                        section = EIPLSection.Cull;
                    }
                    else if(line == "pick")
                    {
                        section = EIPLSection.Pick;
                    }
                    else if(line == "end")
                    {
                        section = EIPLSection.End;
                    }
                }

                switch(section)
                {
                    case EIPLSection.Inst:
                        LoadInstance(line);
                        break;
                }
            }
        }

        private static void LoadInstance(string line)
        {
            // https://gtamods.com/wiki/INST: Id, ModelName, PosX, PosY, PosZ, ScaleX, ScaleY, ScaleZ, RotX, RotY, RotZ, RotW
            if(line == "end")
            {
                return;
            }

            int id;
            string name;
            Vector3 position;
            Vector3 scale;
            Quaternion axis;

            if(string.IsNullOrEmpty(line))
            {
                return;
            }

            string[] parts = line.Split(", ");
            if(parts.Length != 12)
            {
                return;
            }

            // TODO: Add error checking to ensure all these types can be parsed correctly.
            id = int.Parse(parts[0]);
            name = parts[1];
            position = new Vector3(float.Parse(parts[2]), float.Parse(parts[4]), float.Parse(parts[3]));
            scale = new Vector3(float.Parse(parts[5]), float.Parse(parts[6]), float.Parse(parts[7]));
            axis = new Quaternion(float.Parse(parts[8]), float.Parse(parts[10]), float.Parse(parts[9]), float.Parse(parts[11]));
            
            // TODO: Spawn actual object. For now we are simply spawning a cube
            var prefab = Resources.Load<GameObject>("TestObject");
            var gameObject = GameObject.Instantiate(prefab);
            gameObject.name = name;
            gameObject.transform.position = position;
            gameObject.transform.localScale = scale; // Scale is always 1, so we could probably just set it to Vector3.One
            gameObject.transform.rotation = axis;
        }
    }
}