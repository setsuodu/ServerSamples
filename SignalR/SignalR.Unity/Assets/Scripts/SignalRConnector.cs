using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Microsoft.AspNetCore.SignalR.Client;

public class SignalRConnector
{
    public Action<Message> OnMessageReceived;
    private HubConnection connection;
    public string UserName;

    public async Task InitAsync()
    {
        connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5129/myHub")
                .Build();
        connection.On<string>
        (
            "OnConnected", (user) =>
            {
                Debug.Log($"[S2C] {user} is Connected");
                this.UserName = user;
            }
        );
        connection.On<string, string>
        (
            "ReceiveMessage", (user, message) =>
            {
                Debug.Log($"[S2C] ReceiveMessage: {user}:{message} : {OnMessageReceived != null}");
                OnMessageReceived?.Invoke(new Message
                {
                    UserName = user,
                    Text = message,
                });
            }
        );
        await StartConnectionAsync();
    }
    public async Task SendMessageAsync(Message message)
    {
        try
        {
            Debug.Log($"[C] SendMessage: {message.UserName}:{message.Text}");
            await connection.InvokeAsync("SendMessage",
                message.UserName, message.Text);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error {ex.Message}");
        }
    }
    private async Task StartConnectionAsync()
    {
        try
        {
            await connection.StartAsync();
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error {ex.Message}");
        }
    }
    public async Task DisposeAsync()
    {
        try
        {
            await connection.DisposeAsync();
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error {ex.Message}");
        }
    }
}