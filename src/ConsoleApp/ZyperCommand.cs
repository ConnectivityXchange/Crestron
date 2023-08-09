using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApp
{
    public class ZyperCommand
    {
        public string Uri { get; set; }
        public bool HasArgs { get; set; }
        public string[] Options { get; set; }

        public ZyperCommand(string uri, bool hasArg, string[] options)
        {
            Uri = uri;
            HasArgs = hasArg;
            Options = options;
        }

        public ZyperRequest GetRequest()
        {
            // empty method to use if there are no args to insert
            return new ZyperRequest(string.Empty, Uri, string.Empty, string.Empty);
        }

        public string SetArg(params object[] args)
        {
            if (!HasArgs) return Uri;

            // i wanted this to use string.format to insert params into the string but it didnt work
            if (args == null || args.Length == 0)
                return Uri.Replace("{0}", "0");
            else
                return Uri.Replace("{0}", args[0].ToString());
        }
    }
}
