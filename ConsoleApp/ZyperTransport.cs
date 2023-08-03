using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.IO;

namespace ConsoleApp
{
    class ZyperTransport : IDisposable
    {
        private string _host;
        private int _port;
        private string _cookie;

        private HttpWebRequest request;

        public ZyperTransport() {}

        public ZyperResponse Init(string host, int port, string username, string password)
        {
            _host = host;
            _port = port;
            _cookie = string.Empty;

            if (Login(username, password))
            {
                return DeviceList();
            }

            return new ZyperResponse();
        }

        public bool Login(string username, string password)
        {
            var zRequest = new ZyperRequest("", "rcLogin.php", username, password);
            var zResponse = new ZyperResponse();

            string requestString = "http://" + _host + ":" + _port + "/" + zRequest.Uri;
            Console.WriteLine("TX: " + requestString);

            request = (HttpWebRequest)WebRequest.Create(requestString);

            var postData = "serverSocketName=rcServerSocket";
            postData += string.Format("&username={0}", zRequest.Username);
            postData += string.Format("&password={0}", zRequest.Password);

            var data = Encoding.ASCII.GetBytes(postData);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            if (responseString.Trim() != "\"Success\"") return false;

            var cookie = response.Headers["Set-Cookie"].Split(';')[0];
            Console.WriteLine("set cookie: " + cookie);
            _cookie = cookie;

            return _cookie != string.Empty;
        }

        public ZyperResponse DeviceList()
        {
            var zRequest = new ZyperRequest("show device status all", "rcCmd.php", "api", "");
            var zResponse = new ZyperResponse();

            string requestString = "http://" + _host + ":" + _port + "/" + zRequest.Uri;
            Console.WriteLine("TX: [show device status all] to " + requestString);

            request = (HttpWebRequest)WebRequest.Create(requestString);

            var postData = "serverSocketName=rcServerSocket";
            postData += string.Format("&username={0}", zRequest.Username);
            postData += string.Format("&password={0}", zRequest.Password);
            //request.Headers.Add(string.Format("Cookie:{0}", _cookie));
            postData += string.Format("&commands={0}", zRequest.Cmd);

            var data = Encoding.ASCII.GetBytes(postData);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            zResponse.Data = responseString.Trim();

            // Console.WriteLine("RX: " + responseString);

            return zResponse;
        }

        public ZyperResponse SendCommand(string cmd)
        {
            var zRequest = new ZyperRequest(cmd);
            var zResponse = new ZyperResponse();

            string requestString = "http://" + _host + ":" + _port + "/" + zRequest.Uri;
            Console.WriteLine("TX: [" + zRequest.Cmd + "] to " + requestString);

            request = (HttpWebRequest)WebRequest.Create(requestString);

            var postData = "serverSocketName=rcServerSocket";
            postData += string.Format("&username={0}", zRequest.Username);
            postData += string.Format("&password={0}", zRequest.Password);

            //request.Headers.Add(string.Format("Cookie:{0}", _cookie));
            postData += string.Format("&commands={0}", zRequest.Cmd);

            
            var data = Encoding.ASCII.GetBytes(postData);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            zResponse.Data = responseString;

            StringBuilder sb = new StringBuilder();
            sb.Append("RX: [" + zResponse.command[0] + "] (" + zResponse.status + ")");
            sb.Append(" text: " + zResponse.text.Count());
            sb.Append(" errors: " + zResponse.errors.Count());
            sb.Append(" warnings: " + zResponse.warnings.Count());
            Console.WriteLine(sb.ToString());

            if (zResponse.text.Count() > 0)
            {
                sb = new StringBuilder();
                for (int i = 0; i < zResponse.text.Count(); i++)
                {
                    sb.Append("T: " + zResponse.text[i].ToString());
                }
                Console.WriteLine(sb.ToString());
            }
            if (zResponse.errors.Count() > 0)
            {
                sb = new StringBuilder();
                for (int i = 0; i < zResponse.errors.Count(); i++)
                {
                    sb.Append("E: " + zResponse.errors[i].ToString());
                }
                Console.WriteLine(sb.ToString());
            }
            if (zResponse.warnings.Count() > 0)
            {
                sb = new StringBuilder();
                for (int i = 0; i < zResponse.warnings.Count(); i++)
                {
                    sb.Append("W: " + zResponse.warnings[i].ToString());
                }
                Console.WriteLine(sb.ToString());
            }
            return zResponse;
        }



        public void Dispose()
        {
            // Dispose of unmanaged resources.
            if (request != null)
            {
                request.Abort();
                request = null;
            }

            // Suppress finalization.
            GC.SuppressFinalize(this);
        }
    }
}