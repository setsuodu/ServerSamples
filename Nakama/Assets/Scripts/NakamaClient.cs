using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nakama;
using System;

public class NakamaClient : MonoBehaviour
{
    private readonly IClient client;
    const string email = "hello@example.com";
    const string password = "123456";

    async void Start()
    {
        //var session = await client.AuthenticateEmailAsync(email, password);
        //Debug.Log(session);

        const string scheme = "http";
        const string host = "127.0.0.1";
        const int port = 7350;
        const string serverKey = "defaultkey";
        var client = new Client(scheme, host, port, serverKey);
        var deviceId = SystemInfo.deviceUniqueIdentifier;
        try 
        {
            var session = await client.AuthenticateDeviceAsync(deviceId);
            Debug.Log(session);
        }
        catch(Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    void Update()
    {
        
    }
}