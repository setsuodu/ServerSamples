// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Net.Mail;

Console.WriteLine("Hello, World!");

Program.Main(new string[0]);

//string html = BuildHtmlPage();
//File.WriteAllText(@"C:\Users\Administrator\Desktop\index.html", html);

public partial class Program
{
    //文件下载地址
    const string link = "https://gzc-dfsdown.mail.ftn.qq.com/1373//xt6bc040-811b-fcd2-4042-1ad8e10fb35e?dkey=oBWdcxV7KoNW4KwoDQzzHSG1LsXfT-L8PDsKINy5VXcC8rFX4fpXeyl8cq-J6GJTt1WME_Emodq-7Q2GsIWj1aA&fname=Cx%20File%20Explorer_2.2.7_APKPure.apk&eggs";
    //256*256图片
    const string base64_image = "data:image/png;base64,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";

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
        //mailMessage.Attachments.Add(new Attachment("FilePath")); //发送附件
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
        //htmlPage += "function welcome() {print(\"click!\");\r\nwindow.open(\"https://www.javatpoint.com/\");\r\n}";
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
        //htmlPage += "<div>";
        //htmlPage += "<button type=\"button\" onclick=\"welcome()\">点我!</button>";
        //htmlPage += "</div>";

        //htmlPage += "<div>";
        //htmlPage += $"<a href='{link}'>Click here</a>";
        //htmlPage += "</div>";

        htmlPage += "<div>";
        htmlPage += "<a href=\"http://www.google.com\">" +
            //"Hyper Link" +
            //"<button>点我!</button>" +

            //QQ邮箱不支持跨域图片显示
            //"<img src=\"http://blog.moegijinka.cn/upload/backdash.png\"/>" +
            $"<img src=\"{base64_image}\"/>" + 
            "</a>\r\n";
        htmlPage += "</div>";
        htmlPage += "\n</body>";
        //<!--->Body End<--->

        htmlPage += "\n</html>";

        return htmlPage;
    }
}