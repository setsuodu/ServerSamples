using System;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

public class WebServer
{
    private readonly HttpListener _listener = new HttpListener();
    private readonly Func<HttpListenerRequest, string> _responderMethod;

    public WebServer(Func<HttpListenerRequest, string> method, params string[] prefixes) : this(prefixes, method) { } //用到这个
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

    public void Run()
    {
        ThreadPool.QueueUserWorkItem((o) =>
        {
            Debug.Log("Webserver running...");
            try
            {
                while (_listener.IsListening)
                {
                    ThreadPool.QueueUserWorkItem((c) =>
                    {
                        var contenxt = c as HttpListenerContext;
                        try
                        {
                            //Debug.Log(ctx.Request); //System.Net.HttpListenerRequest
                            string html = _responderMethod(contenxt.Request);
                            //Debug.Log(rstr); //<HTML>...
                            byte[] buffer = Encoding.UTF8.GetBytes(html);
                            contenxt.Response.ContentLength64 = buffer.Length;
                            contenxt.Response.ContentType = "text/html"; //输出网页
                            contenxt.Response.ContentEncoding = Encoding.UTF8; //解决中文乱码
                            contenxt.Response.StatusCode = 200;
                            contenxt.Response.OutputStream.Write(buffer, 0, buffer.Length);
                        }
                        catch { } // suppress any exceptions
                        finally
                        {
                            // always close the stream
                            contenxt.Response.OutputStream.Close();
                        }
                    }, _listener.GetContext());
                }
            }
            catch { } // suppress any exceptions
        });
    }

    public void Stop()
    {
        _listener?.Stop();
        _listener?.Close();
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
}