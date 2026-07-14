using RenderWareIo.Structs.Ide;
using RenderWareIo.Structs.Txd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace RenderWareIo
{
    [Serializable]
    public class IdeFile
    {
        public Ide Ide => m_Ide;

        [SerializeField] private Ide m_Ide;
        private string m_Path;


        public IdeFile()
        {
            m_Ide = new Ide();
        }

        public IdeFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Ide file '{path}' does not exist");
            }

            string data = File.ReadAllText(path);
            m_Path = path;

            m_Ide = Ide.Read(data);
        }

        public void Write(string path)
        {
            File.WriteAllText(path, this.Ide.Write());
        }

        public override string ToString()
        {
            return m_Path;
        }

    }
}
