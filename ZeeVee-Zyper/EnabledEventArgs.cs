using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ZeeVee_Zyper
{
    public class EnabledEventArgs : EventArgs
    {
        public string Name { get; private set; }
        public string Type { get; private set; }
        public ushort State { get; private set; }

        public EnabledEventArgs() { }

        public EnabledEventArgs(string name, string type, bool state)
        {
            Name = name;
            Type = type;
            State = (ushort)(state ? 1 : 0);
        }
    }
}