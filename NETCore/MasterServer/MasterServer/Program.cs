using Google.Protobuf;
using Tutorial;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

TheMsg msg = new TheMsg
{
    Name = "Hello",
    Content = "This is on Server"
};

byte[] byteArray = msg.ToByteArray(); //序列化
TheMsg msg2 = TheMsg.Parser.ParseFrom(byteArray); //反序列化