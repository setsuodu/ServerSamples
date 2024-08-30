using System;
using System.Net;
using UnityEngine;

public class Web : MonoBehaviour
{
    WebServer ws = null;

    void Start()
    {
        ws = new WebServer(SendResponse, "http://localhost:8080/test/");
        ws.Run();
        //Console.WriteLine("A simple webserver. Press a key to quit.");
        //Console.ReadKey();
        //ws.Stop();
    }

    void OnDestroy()
    {
        ws.Stop();
    }

    public static string SendResponse(HttpListenerRequest request)
    {
        return string.Format("<HTML><BODY>My web page.<br>{0}   <p><input type='submit' value='§°§ä§á§â§Ñ§Ó§Ú§ä§î'></BODY></HTML>", DateTime.Now);
    }
}