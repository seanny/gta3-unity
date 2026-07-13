using System.IO;
using UnityEngine;

namespace GTA3Unity.Ipl
{
    public static class IplFile
    {
        private enum EIplSection
        {
            None = 0,
            Inst,
            Zone,
            Cull,
            Pick,
            End
        }

        public static void LoadIplFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Debug.LogError($"{nameof(IplFile)} cannot load {fileName}: File does not exist.");
                return;
            }

            EIplSection section = EIplSection.None;

            string[] lines = File.ReadAllLines(fileName);
            foreach (string line in lines)
            {
                if (line.StartsWith('#'))
                {
                    continue;
                }

                if (section == EIplSection.None)
                {
                    if (line == "inst")
                    {
                        section = EIplSection.Inst;
                    }
                    else if (line == "zone")
                    {
                        section = EIplSection.Zone;
                    }
                    else if (line == "cull")
                    {
                        section = EIplSection.Cull;
                    }
                    else if (line == "pick")
                    {
                        section = EIplSection.Pick;
                    }
                    else if (line == "end")
                    {
                        section = EIplSection.End;
                    }
                }

                switch (section)
                {
                    case EIplSection.Inst:
                        LoadInstance(line);
                        break;
                }
            }
        }

        private static void LoadInstance(string line)
        {
            // https://gtamods.com/wiki/INST: Id, ModelName, PosX, PosY, PosZ, ScaleX, ScaleY, ScaleZ, RotX, RotY, RotZ, RotW
            if (line == "end")
            {
                return;
            }

            int id;
            string name;
            Vector3 position;
            Vector3 scale;
            Quaternion axis;

            if (string.IsNullOrEmpty(line))
            {
                return;
            }

            string[] parts = line.Split(", ");
            if (parts.Length != 12)
            {
                return;
            }

            id = int.Parse(parts[0]);
            name = parts[1];
            position = new Vector3(float.Parse(parts[2]), float.Parse(parts[4]), float.Parse(parts[3]));
            scale = new Vector3(float.Parse(parts[5]), float.Parse(parts[6]), float.Parse(parts[7]));
            axis = new Quaternion(float.Parse(parts[8]), float.Parse(parts[10]), float.Parse(parts[9]), float.Parse(parts[11]));

            GameObject prefab = Resources.Load<GameObject>("TestObject");
            GameObject gameObject = GameObject.Instantiate(prefab);
            gameObject.name = name;
            gameObject.transform.position = position;
            gameObject.transform.localScale = scale;
            gameObject.transform.rotation = axis;
        }
    }
}
