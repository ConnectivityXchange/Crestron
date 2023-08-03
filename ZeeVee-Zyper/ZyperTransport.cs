using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Crestron.SimplSharp;
using Crestron.SimplSharp.Net.Http;

namespace ZeeVee_Zyper
{
  class ZyperTransport
  {
    private string _host;
    private int _port;
    private string _cookie;

    public ZyperTransport() { }

    public ZyperResponse Init(string host, int port, string username, string password)
    {
      _host = host;
      _port = port;
      _cookie = string.Empty;

      var request = new ZyperRequest("show device status all", "rcCmd.php", username, password);
      return SendCommand(request.ToJson());
    }

    public ZyperResponse SendCommand(string request)
    {
      CrestronConsole.PrintLine("TX: ({0}) {1}", request.Length, request);

      var zRequest = new ZyperRequest(request);
      var zResponse = new ZyperResponse();

      using (HttpClient client = new HttpClient())
      {
        HttpClientResponse Response;
        HttpClientRequest Request;

        string requestString = "http://" + _host + ":" + _port + "/" + zRequest.Uri;

        Request = new HttpClientRequest();
        Request.RequestType = RequestType.Post;
        Request.Header.ContentType = "application/x-www-form-urlencoded";
        Request.Url.Parse(requestString);

        //Request.Header.AddHeader(new HttpHeader("Cookie", _cookie));

        var postData = "serverSocketName=rcServerSocket";
        postData += string.Format("&username={0}", zRequest.Username);
        postData += string.Format("&password={0}", zRequest.Password);
        postData += string.Format("&commands={0}", zRequest.Cmd);
        Request.ContentString = postData;

        // send the request
        Response = client.Dispatch(Request);

        zResponse.Data = Response.ContentString;
        CrestronConsole.PrintLine("RX: ({0}) {1}", Response.ContentString.Length, Response.ContentString);
      }

      return zResponse;
    }
  }
}