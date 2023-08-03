using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ZeeVee_Zyper
{
    public class ParentZoneEventArgs : EventArgs
    {
        public string Parent { get; set; }
        public string Name { get; set; }
        public string Encoder { get; set; }
        public string Type { get; set; }

        public ParentZoneEventArgs() { }

        public ParentZoneEventArgs(string parent, string name, string encoder, string type)
        {
            Parent = parent;
            Name = name;
            Encoder = encoder;
            Type = type;
        }
    }
}