using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
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
                            CheckIP(contenxt);
                            CheckVPN();
                            bool vpn = CheckForVPNInterface();
                            Debug.Log($"vpn : {vpn}");

                            string html = _responderMethod(contenxt.Request);
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

    // 检查来访用户地址
    void CheckIP(HttpListenerContext contenxt)
    {
        string userIP = contenxt.Request.RemoteEndPoint.Address.ToString();
        Debug.Log($"client ip : {userIP}");

        string localeAPIURL = "http://api.hostip.info/get_html.php?ip=" + userIP;
        HttpWebRequest r = (HttpWebRequest)WebRequest.Create(localeAPIURL);
        r.Method = "Get";
        HttpWebResponse res = (HttpWebResponse)r.GetResponse();
        Stream sr = res.GetResponseStream();
        StreamReader sre = new StreamReader(sr);

        // check response for FRANCE
        string s = sre.ReadToEnd();
        Debug.Log(s);
        //Country: (Private Address) (XX)
        //City: (Private Address)
        //IP: 127.0.0.1

        //string sub = s.Substring(9, 6);
        //if (sub == "FRANCE")
        //{
        //    contenxt.Response.Redirect("http://fr.mysite.com");
        //}
    }

    // 检查服务器还是客户端？
    void CheckRegion()
    {
        var regionInfo = System.Globalization.RegionInfo.CurrentRegion;
        var name = regionInfo.Name;
        var englishName = regionInfo.EnglishName;
        var displayName = regionInfo.DisplayName;
        var geo = regionInfo.GeoId;
        Debug.Log($"Name: {name}");
        Debug.Log($"EnglishName: {englishName}");
        Debug.Log($"DisplayName: {displayName}");
        Debug.Log($"geo: {geo}"); //45→China
    }

    public bool CheckForVPNInterface()
    {
        if (NetworkInterface.GetIsNetworkAvailable())
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface Interface in interfaces)
            {
                // This is the OpenVPN driver for windows. 
                if (Interface.Description.Contains("TAP-Windows Adapter")
                  && Interface.OperationalStatus == OperationalStatus.Up)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool CheckVPN()
    {
        if (NetworkInterface.GetIsNetworkAvailable())
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface Interface in interfaces)
            {
                if (Interface.OperationalStatus == OperationalStatus.Up)
                {
                    if ((Interface.NetworkInterfaceType == NetworkInterfaceType.Ppp) && (Interface.NetworkInterfaceType != NetworkInterfaceType.Loopback))
                    {
                        IPv4InterfaceStatistics statistics = Interface.GetIPv4Statistics();
                        Debug.Log(Interface.Name + " " + Interface.NetworkInterfaceType.ToString() + " " + Interface.Description);
                        return true;
                    }
                    else
                    {
                        Debug.Log("VPN Connection is lost!");
                        return false;
                    }
                }
            }
        }
        Debug.Log("No Network");
        return false;
    }
}