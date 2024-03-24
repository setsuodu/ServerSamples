using System;
using UnityEngine;
using UnityEngine.UI;
using Nakama;

public class NakamaClient : MonoBehaviour
{
    private readonly IClient client;

    public string scheme = "http";
    public string host = "192.168.1.106";
    public int port = 7351;
    public string serverKey = "defaultkey";

    public string email = "hello@example.com";
    public string username = "hello";
    public string password = "12345678";

    public Button btn_CreateUserByEmail;
    public Button btn_CreateUserByDevice;

    void Awake()
    {
        btn_CreateUserByEmail.onClick.AddListener(CreateUserByEmail);
        btn_CreateUserByDevice.onClick.AddListener(CreateUserByDevice);
    }

    async void CreateUserByEmail()
    {
        var client = new Client(scheme, host, port, serverKey);
        try
        {
            var session = await client.AuthenticateEmailAsync(email, password, username, true);
            Debug.Log(session);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    async void CreateUserByDevice()
    {
        var client = new Client(scheme, host, port, serverKey);
        var deviceId = SystemInfo.deviceUniqueIdentifier;
        try
        {
            var session = await client.AuthenticateDeviceAsync(deviceId, username, true);
            Debug.Log(session);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }
}