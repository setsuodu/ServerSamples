using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//TODO:
//1.进度条
//2.统计文件夹数，文件数
//3.全量下载，自动判断数量，自动停止
//4.批量重命名
//5.（爬虫）自动分析url前缀
public class Downloader : MonoBehaviour
{
    public Button btn_Download;
    public Button btn_CreateFolder;

    public string outputPath; //保存到
    public string startUrl = "https://cdn.favcomic.com/file/bucket-media/image/comic/872862632514822144/1/1.webp";

    public int startNum;
    public int endNum;

    void Awake()
    {
        btn_Download.onClick.AddListener(StartDownload);
        btn_CreateFolder.onClick.AddListener(CreateFolder);
    }

    async void StartDownload()
    {
        string[] array0 = startUrl.Split('.');
        string format = array0[array0.Length - 1];
        Debug.Log($"图片格式是：{format}");

        int last = startUrl.LastIndexOf("/"); //去掉最后一个 / 后面的字串
        string baseUrl = startUrl.Substring(0, last + 1);
        Debug.Log($"基础URL：{baseUrl}");

        List<string> urls = new List<string>();
        for (int i = startNum; i <= endNum - startNum + 1; i++)
        {
            string _url = $"{baseUrl}{i}.{format}";
            urls.Add(_url);
        }

        //WebClientTest.Run(urls);
        HttpClientTest.outputPath = outputPath;
        await HttpClientTest.Run(urls);
    }

    // 批创建文件夹
    void CreateFolder()
    {
        for (int i = 1; i <= 19; i++)
        {
            string folderName = $"第{i.ToString().PadLeft(2, '0')}话";
            Directory.CreateDirectory($"{outputPath}/{folderName}");
        }
    }
}

public class WebClientTest
{
    public static void Run(List<string> urls)
    {
        List<Task> downloadTasks = new List<Task>();
        foreach (string url in urls)
        {
            WebClient client = new WebClient();
            Task downloadTask = client.DownloadFileTaskAsync(new Uri(url), GetFileNameFromUrl(url));

            downloadTasks.Add(downloadTask);
        }

        Task.WaitAll(downloadTasks.ToArray());

        Debug.Log("<color=green>All files downloaded.</color>");
    }

    static string GetFileNameFromUrl(string url)
    {
        Uri uri = new Uri(url);
        return uri.Segments[uri.Segments.Length - 1];
    }
}

public class HttpClientTest
{
    public static string outputPath;

    public static async Task Run(List<string> urls)
    {
        List<Task> downloadTasks = new List<Task>();
        using (HttpClient client = new HttpClient())
        {
            foreach (string url in urls)
            {
                Task downloadTask = DownloadFileAsync(client, url);
                downloadTasks.Add(downloadTask);
            }

            await Task.WhenAll(downloadTasks);
        }

        Debug.Log("<color=green>All files downloaded.</color>");
    }

    static async Task DownloadFileAsync(HttpClient client, string url)
    {
        byte[] data = await client.GetByteArrayAsync(url);
        string fileName = GetFileNameFromUrl(url);

        // 将文件保存到磁盘或进行其他处理
        // ...
        string saveto = @$"{outputPath}\{fileName}";
        Debug.Log("saveto: " + saveto);
        MemoryStream ms = new MemoryStream(data); //把那个byte[] 数组传进去, 然后
        FileStream fs = new FileStream(saveto, FileMode.OpenOrCreate);
        ms.WriteTo(fs);
        ms.Close();
        fs.Close();

        //Debug.Log("Downloaded " + fileName);
    }

    static string GetFileNameFromUrl(string url)
    {
        Uri uri = new Uri(url);
        return uri.Segments[uri.Segments.Length - 1];
    }
}