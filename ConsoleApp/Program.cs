using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ZyperDevice device = new ZyperDevice();

            // device discovery
            device.Init("172.16.16.118", 80, "api", "");

            //device.GetVideowall();
            device.GetMultiviews();
            // send commands

            device.Join("Laptop", "LG", "fastSwitched");
            device.Join("Amazon", "Kogan", "fastSwitched");

            //device.ShowDeviceConfig("");
            //device.ShowDeviceStatus("");

            Console.ReadLine();

        }
    }
}
