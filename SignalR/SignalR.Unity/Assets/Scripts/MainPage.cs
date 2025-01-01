using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainPage : MonoBehaviour
{
    private SignalRConnector connector;
    private Queue<Message> messageQueue = new Queue<Message>();
    public InputField MessageInput;
    public Button SendButton;
    public Text ReceivedText;

    async void Start()
    {
        connector = new SignalRConnector();
        connector.OnMessageReceived += UpdateReceivedMessages;

        await connector.InitAsync();
        SendButton.onClick.AddListener(SendMessage);
    }
    void Update()
    {
        if (messageQueue.Count > 0)
        {
            Message message = messageQueue.Dequeue();

            var lastMessages = this.ReceivedText.text;
            if (string.IsNullOrEmpty(lastMessages) == false)
            {
                lastMessages += "\n";
            }
            lastMessages += $"{message.UserName} : {message.Text}";
            this.ReceivedText.text = lastMessages;
        }
    }
    async void OnDestroy()
    {
        await connector.DisposeAsync();
    }

    void UpdateReceivedMessages(Message message)
    {
        Debug.Log($"委托：{message.UserName}:{message.Text}");
        messageQueue.Enqueue(message);

        // 这里是线程里，无法更新UI
        //var lastMessages = this.ReceivedText.text;
        //if (string.IsNullOrEmpty(lastMessages) == false)
        //{
        //    lastMessages += "\n";
        //}
        //lastMessages += $"{message.UserName} : {message.Text}";
        //this.ReceivedText.text = lastMessages;
    }
    async void SendMessage()
    {
        await connector.SendMessageAsync(new Message
        {
            UserName = connector.UserName,
            Text = MessageInput.text,
        });
    }
}