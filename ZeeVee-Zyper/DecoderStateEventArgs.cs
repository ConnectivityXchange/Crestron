using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ZeeVee_Zyper
{
    public class DecoderStateEventArgs : EventArgs
    {

        public string Mac { get; set; }
        public string Name { get; set; }

        public ushort CableConnected { get; set; }

        public string ConnectedMac { get; set; }
        public string ConnectedName { get; set; }

        public DecoderStateEventArgs() { }

        public DecoderStateEventArgs(string mac, string name, string cableConnected, string connectedMac, string connectedName)
        {
            Mac = mac;
            Name = name;

            CableConnected = (ushort)(cableConnected == "connected" ? 1 : 0);

            ConnectedMac = connectedMac;
            ConnectedName = connectedName;
        }
    }
}