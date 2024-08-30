using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

public class WebServer
{
    private readonly HttpListener _listener = new HttpListener();
    private readonly Func<HttpListenerRequest, string> _responderMethod;

    public WebServer(string[] prefixes, Func<HttpListenerRequest, string> method)
    {
        // A responder method is required
        if (method == null)
            throw new ArgumentException("method");

        foreach (string s in prefixes)
            _listener.Prefixes.Add(s);

        _responderMethod = method;
        _listener.Start();
    }
    public WebServer(Func<HttpListenerRequest, string> method, params string[] prefixes) : this(prefixes, method) { }

    public void Run()
    {
        ThreadPool.QueueUserWorkItem((o) =>
        {
            Console.WriteLine("Webserver running...");
            try
            {
                while (_listener.IsListening)
                {
                    ThreadPool.QueueUserWorkItem((c) =>
                    {
                        var ctx = c as HttpListenerContext;
                        try
                        {
                            string rstr = _responderMethod(ctx.Request);
                            byte[] buf = Encoding.UTF8.GetBytes(rstr);
                            ctx.Response.ContentLength64 = buf.Length;
                            ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                        }
                        catch { } // suppress any exceptions
                        finally
                        {
                            // always close the stream
                            ctx.Response.OutputStream.Close();
                        }
                    }, _listener.GetContext());
                }
            }
            catch { } // suppress any exceptions
        });
    }

    public string SendResponse(HttpListenerRequest request)
    {
        string path = @"static\index.html";
        Debug.Log($"file : {File.Exists(path)}");
        return File.ReadAllText(path);
    }

    void Detect()
    {
        //string userIP = Request.ServerVariables["REMOTE_ADDR"];
        //string localeAPIURL = "http://api.hostip.info/get_html.php?ip=" + userIP;

        //HttpWebRequest r = (HttpWebRequest)WebRequest.Create(localeAPIURL);
        //r.Method = "Get";
        //HttpWebResponse res = (HttpWebResponse)r.GetResponse();
        //Stream sr = res.GetResponseStream();
        //StreamReader sre = new StreamReader(sr);

        //// check response for FRANCE
        //string s = sre.ReadToEnd();
        //string sub = s.Substring(9, 6);
        //if (sub == "FRANCE")
        //{
        //    Response.Redirect("http://fr.mysite.com");
        //}
    }

    public void Stop()
    {
        _listener.Stop();
        _listener.Close();
    }
}