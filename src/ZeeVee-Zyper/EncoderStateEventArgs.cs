using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ZeeVee_Zyper
{
    public class EncoderStateEventArgs
    {
        public string Mac { get; set; }
        public string Name { get; set; }

        public ushort CableConnected { get; set; }

        public EncoderStateEventArgs() { }

        public EncoderStateEventArgs(string mac, string name, string cableConnected)
        {
            Mac = mac;
            Name = name;

            CableConnected = (ushort)(cableConnected == "connected" ? 1 : 0);
        }
    }
}