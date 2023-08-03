using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace ZeeVee_Zyper
{
    class ZyperRequest
    {
        public string Cmd { get; set; }
        public string Uri { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public ZyperRequest(string json)
        {
            var temp = JsonConvert.DeserializeObject<ZyperRequest>(json);
            Cmd = temp.Cmd;
            Uri = temp.Uri;
            Username = temp.Username;
            Password = temp.Password;
        }

        [JsonConstructor]
        public ZyperRequest(string cmd, string uri, string username, string password)
        {
            Cmd = cmd;
            Uri = uri;
            Username = username;
            Password = password;
        }

        /// <summary>
        /// Gets the current request object as a json string
        /// </summary>
        /// <returns>string</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}