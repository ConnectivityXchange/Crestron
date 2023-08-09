using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConsoleApp
{

    /// <summary>
    /// Returned from the send requet method with the data and any transformed json
    /// </summary>
    public class ZyperResponse
    {
        public JValue status { get; set; }
        public JArray responses { get; set; }

        public JToken text { get; set; }
        public JToken errors { get; set; }
        public JToken warnings { get; set; }
        public JToken command { get; set; }

        private string _data = string.Empty;
        public string Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;

                var obj = JsonConvert.DeserializeObject<ZyperResponse>(value);

                this.status = obj.status;
                this.responses = obj.responses;

                if (obj.responses.Count > 0)
                {
                    this.text = obj.text;
                    this.errors = obj.errors;
                    this.warnings = obj.warnings;
                    this.command = obj.command;
                }
            }
        }

        public ZyperResponse() { }

        [JsonConstructor]
        public ZyperResponse(JValue status, JArray responses)
        {
            this.status = status;
            this.responses = responses;

            if (responses.Count > 0)
            {
                this.text = responses[0].SelectToken("text");
                this.errors = responses[0].SelectToken("errors");
                this.warnings = responses[0].SelectToken("warnings");
                this.command = responses[0].SelectToken("command");
            }

        }

    }

}