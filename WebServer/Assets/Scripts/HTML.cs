using System;
using System.IO;
using System.Net;
using UnityEngine;

public class Web : MonoBehaviour
{
    WebServer ws = null;

    //string url = "http://localhost:8080/"; //外网不支持访问
    string url = "http://*:8080/";
    static string wwwFolder;

    void Start()
    {
        wwwFolder = Path.Combine(Environment.CurrentDirectory, "www");
        //Debug.Log(wwwFolder);

        ws = new WebServer(SendResponse, url);
        ws.Run();
    }

    void OnDestroy()
    {
        ws?.Stop();
    }

    public static string SendResponse(HttpListenerRequest request)
    {
        //string htmlPath = Path.Combine(wwwFolder, "index.html");
        //string htmlContent = File.ReadAllText(htmlPath);
        //return htmlContent;
        string html = $"<HTML><BODY>My web page.<br>{DateTime.Now}   <p><input type='submit' value='按钮'></BODY></HTML>";
        return html;
    }
}