// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Net.Mail;

string[] array = { };

Console.WriteLine("Hello, World!");

Program.Main(array);

//string html = BuildHtmlPage();
//File.WriteAllText(@"C:\Users\Administrator\Desktop\index.html", html);

public partial class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Start,,,,,");

        //实例化一个发送邮件类。
        MailMessage mailMessage = new MailMessage();
        //发件人邮箱地址，方法重载不同，可以根据需求自行选择。
        mailMessage.From = new MailAddress("339131001@qq.com");
        //收件人邮箱地址。
        mailMessage.To.Add(new MailAddress("339131001@qq.com"));
        //抄送人邮箱地址。
        //message.CC.Add(sender);
        //邮件标题。
        mailMessage.Subject = "发送邮件测试";
        //邮件内容。
        //mailMessage.Body = "这是我给你发送的第一份邮件哦！ \nhttps://gzc-dfsdown.mail.ftn.qq.com/1373//xt6bc040-811b-fcd2-4042-1ad8e10fb35e?dkey=oBWdcxV7KoNW4KwoDQzzHSG1LsXfT-L8PDsKINy5VXcC8rFX4fpXeyl8cq-J6GJTt1WME_Emodq-7Q2GsIWj1aA&fname=Cx%20File%20Explorer_2.2.7_APKPure.apk&eggs";
        mailMessage.Body = BuildHtmlPage();
        //是否支持内容为HTML。
        mailMessage.IsBodyHtml = true;
        //实例化一个SmtpClient类。
        SmtpClient client = new SmtpClient();
        client.Port = 587;
        //在这里使用的是qq邮箱，所以是smtp.qq.com，如果你使用的是126邮箱，那么就是smtp.126.com。
        //client.Host = "smtp.163.com";
        client.Host = "smtp.qq.com";
        //使用安全加密连接（是否启用SSL）
        client.EnableSsl = true;
        //设置超时时间
        //client.Timeout = 10000;
        //不和请求一块发送。
        client.UseDefaultCredentials = false;
        //验证发件人身份(发件人的邮箱，邮箱里的生成授权码);
        client.Credentials = new NetworkCredential("339131001@qq.com", "xywlnfxmabpxbidd");
        //网易邮箱客户端授权码DJURBEKTXEWXQATX
        //client.Credentials = new NetworkCredential("liulijun3236@163.com", "ZAJDNCKWHUBHQIMY");
        try
        {
            //发送
            client.Send(mailMessage);
            //发送成功
            Console.WriteLine("发送成功,,,,,");
        }
        catch (Exception e)//发送异常
        {
            //发送失败
            //System.IO.File.WriteAllText(@"C:\test.txt", e.ToString(), Encoding.UTF8);
            Console.WriteLine($"发送失败,,,,,{e.ToString()}");
        }
    }

    private static string BuildHtmlPage()
    {
        var htmlPage = "";
        htmlPage += @"<!DOCTYPE HTML>
        <html>";

        //<!--->Head Start<--->
        htmlPage += "\n<head>\n";
        htmlPage += "<title>\r\nexample of onclick button\r\n</title>";

        htmlPage += "\n<style>\n";
        //foreach (var cssPart in cssParts)
        //{
        //    htmlPage += cssPart + "\n";
        //}
        htmlPage += "</style>\n";

        //htmlPage += @"<script type=""text/javascript"">";
        htmlPage += "\n<script>\n";
        htmlPage += "function welcome() {print(\"click!\");\r\nwindow.open(\"https://www.javatpoint.com/\");\r\n}";
        //htmlPage += "window.onload = function() {";
        //foreach (var scriptPart in scriptParts)
        //{
        //    htmlPage += scriptPart + "\n";
        //}
        //htmlPage += @"}
        htmlPage += "\n</script>";

        htmlPage += "\n</head>";
        //<!--->Head End<--->


        //<!--->Body Start<--->
        htmlPage += "\n<body>\n";
        //foreach (var bodyPart in bodyParts)
        //{
        //    htmlPage += bodyPart + "\n";
        //}
        htmlPage += @"<h2 title=""I'm a header"">The title Attribute</h2>
<p title=""I'm a tooltip"">Mouse over this paragraph, to display the title attribute as a tooltip.</p>";
        htmlPage += "<button type=\"button\" onclick=\"welcome()\">点我!</button>";
        htmlPage += "\n</body>";
        //<!--->Body End<--->

        htmlPage += "\n</html>";

        return htmlPage;
    }
}