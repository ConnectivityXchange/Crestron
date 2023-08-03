using System;
using System.Collections.Generic;
using System.Text;

using Crestron.SimplSharp;

namespace ZeeVee_Zyper
{
    public class ZyperDevice
    {
        public delegate void ParentZoneHandler(ParentZoneEventArgs e);
        public static event ParentZoneHandler ParentZoneFeedback;

        public delegate void EnabledHandler(EnabledEventArgs e);
        public static event EnabledHandler EnabledFeedback;

        public delegate void DecoderStateHandler(DecoderStateEventArgs e);
        public static event DecoderStateHandler DecoderStateFeedback;

        public delegate void EncoderStateHandler(EncoderStateEventArgs e);
        public static event EncoderStateHandler EncoderStateFeedback;

        private static bool _hasStarted = false;
        private static bool _isDisposing = false;
        private static ZyperTransport _client;

        private static string Host { get; set; }
        private static ushort Port { get; set; }
        private static string Username { get; set; }
        private static string Password { get; set; }

        private CTimer _pollTimer;
        private int _pollTime;
        private bool _hasDecoder;

        private static string DeviceId { get; set; }

        public static List<EnabledEventArgs> encoderList;
        public static List<EnabledEventArgs> decoderList;
        public static List<EnabledEventArgs> multiviewList;
        public static List<EnabledEventArgs> videowallList;

        public ZyperDevice() {}

        public void Init(string host, ushort port, string username, string password, ushort polltime)
        {
            CrestronConsole.PrintLine("ZyperDevice Init");
            if (DeviceId != null && DeviceId != string.Empty)
            {
                CrestronConsole.PrintLine("guid exists: " + DeviceId);
            }
            else
            {
                DeviceId = Guid.NewGuid().ToString();
                CrestronConsole.PrintLine("created guid: " + DeviceId);
            }
            

            Host = host;
            Port = port;
            Username = username;
            Password = password;

            // init the timer
            _pollTime = polltime * 100; // in hundreths seconds
            _pollTimer = new CTimer(PollCallback, Timeout.Infinite);
            _hasDecoder = false;

            // build client
            _client = new ZyperTransport();
            var response = _client.Init(Host, Port, Username, Password);

            if (ProcessDeviceList(response) && EnabledFeedback != null)
            {
                foreach (var item in encoderList)
                {
                    EnabledFeedback.Invoke(item);
                }
                foreach (var item in decoderList)
                {
                    EnabledFeedback.Invoke(item);
                    _hasDecoder = true;
                }
            }

            var multiviewResponse = SendCommand("show multiviews config");
            if (ProcessMultiviewList(multiviewResponse) && EnabledFeedback != null)
            {
                foreach (var item in multiviewList)
                {
                    EnabledFeedback.Invoke(item);
                }
            }

            var videowallResponse = SendCommand("show videoWalls");
            if (ProcessVideowallList(videowallResponse) && EnabledFeedback != null)
            {
                foreach (var item in videowallList)
                {
                    EnabledFeedback.Invoke(item);
                }
            }

            // if we discovered decoders then start the poll timer
            if (_hasDecoder)
            {
                CrestronConsole.PrintLine("Starting Decoder Poll > repeat time {0} sec", (_pollTime / 1000));
                _pollTimer.Reset(_pollTime, _pollTime);
            }

            _hasStarted = true;
            if (_isDisposing) Disable();
        }

        public void Disable()
        {
            _isDisposing = true;
            if (!_hasStarted) return;

            if (_pollTimer != null)
            {
                _pollTimer.Stop();
                _pollTimer.Dispose();
            }

            if (EnabledFeedback != null)
            {
                CrestronConsole.PrintLine("ZyperDevice Disable");

                if (encoderList != null)
                {
                    Console.WriteLine("sending encoder disable");
                    foreach (var item in encoderList)
                    {
                        EnabledFeedback.Invoke(new EnabledEventArgs(item.Name, item.Type, false));
                    }
                }
                if (decoderList != null)
                {
                    Console.WriteLine("sending decoder disable");
                    foreach (var item in decoderList)
                    {
                        EnabledFeedback.Invoke(new EnabledEventArgs(item.Name, item.Type, false));
                    }
                }
            }
            _isDisposing = false;
        }

        public void ParentZoneMessage(string parent, string name, string encoder, string type)
        {
            if (ParentZoneFeedback == null)
            {
                Console.WriteLine("ParentZoneMessage > ParentZoneFeedback is null");
                return;
            }

            ParentZoneFeedback(new ParentZoneEventArgs(parent, name, encoder, type));
        }

        // # Join command
        public void Join(string encoderName, string decoderName, string modeName)
        {
            var enccoder = encoderName == "" ? "none" : encoderName;
            var command = string.Format(@"join ""{0}"" ""{1}"" ""{2}""", enccoder, decoderName, modeName);
            
            SendCommand(command);
        }

        public void JoinDefined(ushort enc, ushort dec, ushort mode)
        {
            var encName = encoderList[enc].Name;
            var decName = decoderList[dec].Name;
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
            string encoder = encoderName == "" ? "  " : encoderName;
            var command = string.Format(@"set multiview ""{0}"" windowNumber {1} newEncoderName ""{2}""", multiName, windowNumber, encoder);

            SendCommand(command);
        }

        // Set multiview MVNAME windowNumber X layer
        public void MultiviewLayer(string multiName, ushort windowNumber, ushort layer)
        {
            var command = string.Format(@"set multiview {0} windowNumber {1} layer {2}", multiName, windowNumber, layer);
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
            var inBytes = Encoding.ASCII.GetBytes(data);
            var inData = BitConverter.ToString(inBytes);
            CrestronConsole.PrintLine("SendRs232 ({0}): {1}", inBytes.Length, inData);

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
                    command = string.Format(@"dataConnect ""{0}"" ""{1}"" {2}", connectFromName, connectToName, type);
                    break;
            }
            SendCommand(command);
        }

        private ZyperResponse SendCommand(string command)
        {
            CrestronConsole.PrintLine("send command: " + command);

            if (_client == null)
            {
                CrestronConsole.PrintLine("client is null");
                return null;
            } 

            var request = new ZyperRequest(command, "rcCmd.php", Username, Password);
            return _client.SendCommand(request.ToJson());
        }

        private void PollCallback(object userSpecific)
        {
            if (DecoderStateFeedback == null)
            {
                Console.WriteLine("PollDecodersCallback > DecoderStateFeedback is null");
                return;
            }

            if (EncoderStateFeedback == null)
            {
                Console.WriteLine("PollDecodersCallback > EncoderStateFeedback is null");
                return;
            }

            var command = "show device status all";
            var response = SendCommand(command);

            if (response != null)
                ProcessPoll(response);
        }

        private void ProcessPoll(ZyperResponse response)
        {
            if (response.Data == string.Empty) return;

            foreach (var item in response.text)
            {
                var gen = item.SelectToken("gen");
                var type = gen.SelectToken("type").ToString();
                var mac = gen.SelectToken("mac").ToString();
                var name = gen.SelectToken("name").ToString();

                switch (type)
                {
                    case "encoder":
                        var hdmiInput = item.SelectToken("hdmiInput");
                        var hdmiInputCableConnected = hdmiInput.SelectToken("cableConnected").ToString();

                        EncoderStateFeedback.Invoke(
                            new EncoderStateEventArgs(
                                mac,
                                name,
                                hdmiInputCableConnected
                                ));
                        break;
                    case "decoder":
                        var hdmiOutput = item.SelectToken("hdmiOutput");
                        var hdmiOutputCableConnected = hdmiOutput.SelectToken("cableConnected").ToString();

                        var connectedEncoder = item.SelectToken("connectedEncoder");
                        var connectedMac = connectedEncoder.SelectToken("mac").ToString();
                        var connectedName = connectedEncoder.SelectToken("name").ToString();

                        DecoderStateFeedback.Invoke(
                            new DecoderStateEventArgs(
                                mac,
                                name,
                                hdmiOutputCableConnected,
                                connectedMac,
                                connectedName
                                ));
                        break;
                    default:
                        Console.WriteLine("unknown type: " + type);
                        break;
                }
            }
        }

        private bool ProcessDeviceList(ZyperResponse response)
        {
            // init and reset list
            encoderList = new List<EnabledEventArgs>();
            decoderList = new List<EnabledEventArgs>();

            if (response.Data == string.Empty) return false;

            foreach (var item in response.text)
            {
                var gen = item.SelectToken("gen");
                var type = gen.SelectToken("type").ToString();
                var name = gen.SelectToken("name").ToString();

                if (type == "encoder")
                {
                    encoderList.Add(new EnabledEventArgs(name, type, true));
                }
                if (type == "decoder")
                {
                    decoderList.Add(new EnabledEventArgs(name, type, true));
                }
            }

            return (encoderList.Count > 1 && decoderList.Count > 1);
        }

        private bool ProcessMultiviewList(ZyperResponse response)
        {
            multiviewList = new List<EnabledEventArgs>();

            if (response.Data == string.Empty) return false;

            foreach (var item in response.text)
            {
                var gen = item.SelectToken("gen");
                var type = "multiview";
                var name = gen.SelectToken("name").ToString();

                multiviewList.Add(new EnabledEventArgs(name, type, true));
            }

            return multiviewList.Count > 0;
        }

        private bool ProcessVideowallList(ZyperResponse response)
        {
            videowallList = new List<EnabledEventArgs>();

            if (response.Data == string.Empty) return false;

            foreach (var item in response.text)
            {
                var gen = item.SelectToken("gen");
                var type = "videowall";
                var name = gen.SelectToken("name").ToString();

                videowallList.Add(new EnabledEventArgs(name, type, true));
            }

            return multiviewList.Count > 0;
        }


    }
}
