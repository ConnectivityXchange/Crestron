using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;

namespace ConsoleApp
{
    public enum ModeEnum
    {
        none = 0,
        analogAudio,
        hdmiAudio,
        multiview,
        video,
        fastSwitched,
        genlocked,
        genlockedScaled,
        videoWall,
        usb
    }

    class ZyperDevice
    {
        private ZyperTransport client = new ZyperTransport();

        public List<string> encoderList;
        public List<string> decoderList;
        public List<string> multiviewList;

        public ZyperDevice() {}

        public void Init(string host, int port, string username, string password)
        {
            var response = client.Init(host, port, username, password);

            if (ProcessDeviceList(response))
            {
                Console.WriteLine("");
                Console.WriteLine("encoder list");
                Console.WriteLine("------------");
                foreach (var item in encoderList)
                {
                    Console.WriteLine(item);
                }
                Console.WriteLine("");
                Console.WriteLine("decoder list");
                Console.WriteLine("------------");
                foreach (var item in decoderList)
                {
                    Console.WriteLine(item);
                }
                Console.WriteLine("");
            }
        }

        public void GetVideowall()
        {
            var response = SendCommand(@"show videoWalls");


        }

        public void GetMultiviews()
        {
            var response = SendCommand(@"show multiviews config");


        }

        // # Join command
        public void Join(string encoderName, string decoderName, string modeName)
        {
            var enccoder = encoderName == "" ? "none" : encoderName;
            var command = string.Format(@"join ""{0}"" ""{1}"" {2}", enccoder, decoderName, modeName);
            SendCommand(command);
        }

        public void JoinDefined(ushort enc, ushort dec, ushort mode)
        {
            var encName = encoderList[enc];
            var decName = decoderList[dec];
            var modeName = ((ModeEnum)mode).ToString();
            Join(encName, decName, modeName);
        }

        // Multiview joins
        public void MultiviewJoin(string encoderName, string decoderName)
        {
            string encoder = encoderName == "" ? "none" : encoderName;
            var command = string.Format(@"join ""{0}"" ""{1}"" multiview", encoder, decoderName);

            SendCommand(command);
        }

        // Set multiview MVNAME windowNumber X newEncoderName
        public void MultiviewNewEncoderName(string multiName, ushort windowNumber, string encoderName)
        {
            string encoder = encoderName == "" ? "none" : encoderName;
            var command = string.Format(@"set multiview ""{0}"" windowNumber {1} newEncoderName ""{2}""", multiName, windowNumber, encoder);
            
            SendCommand(command);
        }

        // Set multiview MVNAME windowNumber X layer
        public void MultiviewLayer(string multiName, ushort windowNumber, ushort layer)
        {
            var command = string.Format(@"set multiview ""{0}"" windowNumber {1} layer {2}", multiName, windowNumber, layer);
            SendCommand(command);
        }

        // Set multiview MVNAME windowNumber X channel up
        public void MultiviewChannelUp(string multiName, ushort windowNumber)
        {
            var command = string.Format(@"set multiview ""{0}"" windowNumber {1} channel up", multiName, windowNumber);
            SendCommand(command);
        }

        // Set multiview MVNAME windowNumber X channel down
        public void MultiviewChannelDown(string multiName, ushort windowNumber)
        {
            var command = string.Format(@"set multiview ""{0}"" windowNumber {1} channel down", multiName, windowNumber);
            SendCommand(command);
        }

        // Set multiview MVNAME audioSource windowNumber X
        public void MultiviewAudioSource(string multiName, ushort windowNumber)
        {
            string window = windowNumber == 0 ? "none" : windowNumber.ToString();
            var command = string.Format(@"set multiview ""{0}"" audioSource windowNumber {1}", multiName, window);
            
            SendCommand(command);
        }

        // # Serial/IR commands:

        // send ir
        public void SendIr(string deviceName, string data)
        {
            var command = string.Format(@"send ""{0}"" ir ""{1}""", deviceName, data);

            SendCommand(command);
        }

        // send rs232
        public void SendRs232(string deviceName, string data)
        {
            var command = string.Format(@"send ""{0}"" rs232 ""{1}""", deviceName, data);

            SendCommand(command);
        }

        // dataConnect between server and endpoint (This command sets up the link between the server and endpoint and is only run once)\
        public void DataConnectIr(string connectFromName, string connectToName, ushort port)
        {
            DataConnect(connectFromName, connectToName, "ir", port);
        }

        public void DataConnectRs232(string connectFromName, string connectToName, ushort port)
        {
            DataConnect(connectFromName, connectToName, "rs232", port);
        }

        // # Preset commands:
        // run preset
        public void RunPreset(string presetName)
        {
            var command = string.Format(@"run preset ""{0}""", presetName);

            SendCommand(command);
        }

        // # CEC commands:
        // send cec on
        public void CecOn(string deviceName)
        {
            var command = string.Format(@"send ""{0}"" cec on", deviceName);

            SendCommand(command);
        }
        // send cec off
        public void CecOff(string deviceName)
        {
            var command = string.Format(@"send ""{0}"" cec off", deviceName);

            SendCommand(command);
        }
        // send cec hexString
        public void CecHexString(string deviceName, string hexString)
        {
            var command = string.Format(@"send ""{0}"" cec hexString ""{1}""", deviceName, hexString);

            SendCommand(command);
        }

        // # Status Commands
        // Show device config
        public void ShowDeviceConfig(string deviceName)
        {
            var device = deviceName == "" ? "all" : deviceName;
            var command = string.Format(@"show device config ""{0}""", device);

            SendCommand(command);
        }
        // Show device status
        public void ShowDeviceStatus(string deviceName)
        {
            var device = deviceName == "" ? "all" : deviceName;
            var command = string.Format(@"show device status ""{0}""", device);

            SendCommand(command);
        }

        // # Miscellaneous commands:
        // channel up
        public void ChannelUp(string deviceName)
        {
            var command = string.Format(@"channel up ""{0}""", deviceName);

            SendCommand(command);
        }
        // channel down
        public void ChannelDown(string deviceName)
        {
            var command = string.Format(@"channel down ""{0}""", deviceName);

            SendCommand(command);
        }

        private void DataConnect(string connectFromName, string connectToName, string type, ushort port)
        {
            string command;
            switch (connectToName)
            {
                case "":
                    command = string.Format(@"dataConnect ""{0}"" none {1}", connectFromName, type);
                    break;
                case "server":
                    command = string.Format(@"dataConnect ""{0}"" server {1} tunnelPort {2}", connectFromName, type, port);
                    break;
                default:
                    command = string.Format(@"dataConnect ""{0}"" ""{1}"" {2}", connectFromName, connectToName, port);
                    break;
            }
            SendCommand(command);
        }

        private ZyperResponse SendCommand(string command)
        {
            var request = new ZyperRequest(command, "rcCmd.php", "api", "");
            return client.SendCommand(request.ToJson());
        }

        private bool ProcessMultiviewList(ZyperResponse response)
        {
            multiviewList = new List<string>();

            if (response.Data == string.Empty) return false;

            foreach (var item in response.text)
            {
                var gen = item.SelectToken("gen");
                var type = gen.SelectToken("type").ToString();
                var name = gen.SelectToken("name").ToString();

                multiviewList.Add(name);
            }

            return multiviewList.Count > 0;
        }

        private bool ProcessDeviceList(ZyperResponse response)
        {
            // init and reset list
            encoderList = new List<string>();
            encoderList.Add("none");

            decoderList = new List<string>();
            decoderList.Add("none");

            if (response.Data == string.Empty) return false;

            foreach (var item in response.text)
            {
                var gen = item.SelectToken("gen");
                var type = gen.SelectToken("type").ToString();
                var name = gen.SelectToken("name").ToString();

                if (type == "encoder")
                {
                    encoderList.Add(name);
                }
                if (type == "decoder")
                {
                    decoderList.Add(name);
                }
            }

            return (encoderList.Count > 1 && decoderList.Count > 1);
        }

        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }
    }
}
