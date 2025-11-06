using UnityEngine;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;

public class APIService : MonoBehaviour
{
    // 最佳实践：HttpClient 应该是静态且可重用的
    private static readonly HttpClient client = new HttpClient();

    // 用于取消异步任务（例如当对象销毁时）
    private CancellationTokenSource cts;

    void Awake()
    {
        cts = new CancellationTokenSource();
    }

    void OnDestroy()
    {
        // 当对象销毁时，取消所有正在进行的请求
        cts?.Cancel();
        cts?.Dispose();
    }

    public async void GetUserData_Clicked()
    {
        string userId = "123";
        Debug.Log($"Attempting to fetch user data for {userId}...");

        try
        {
            // 调用异步方法
            UserData data = await GetUserDataAsync($"https://api.example.com/users/{userId}", cts.Token);

            // 回到了主线程，可以安全更新 UI
            Debug.Log($"Success! User: {data.Name}, Score: {data.Score}");
            // myUIText.text = data.Name;

        }
        catch (TaskCanceledException)
        {
            // 任务被取消（通常是因为对象 OnDestroy 或超时）
            Debug.Log("Request was canceled.");
        }
        catch (HttpRequestException e)
        {
            // 网络错误 (DNS, 无法连接, 404/500 等)
            Debug.LogError($"Network Error: {e.Message}");
        }
        catch (System.Exception e)
        {
            // 其他所有错误（例如 JSON 解析失败）
            Debug.LogError($"An error occurred: {e.Message}");
        }
    }

    // 核心异步方法
    public async Task<UserData> GetUserDataAsync(string url, CancellationToken token)
    {
        // 1. 发送请求 (不会阻塞)
        // 使用 ConfigureAwait(false) 告诉 Task 不必切回原始上下文（Unity 主线程）
        // 这允许它在后台线程池上继续执行，提高性能。
        HttpResponseMessage response = await client.GetAsync(url, token).ConfigureAwait(false);

        // 2. 检查成功状态（404, 500 等会在此处抛出异常）
        response.EnsureSuccessStatusCode();

        // 3. 读取响应体 (不会阻塞)
        string jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        // 4. (可选) 反序列化 JSON
        // 这一步也可以移到后台线程，避免主线程卡顿
        // Task.Run 适用于 CPU 密集型操作
        return await Task.Run(() =>
            JsonConvert.DeserializeObject<UserData>(jsonResponse),
            token
        );

        // 注意：当此方法返回时，调用方（GetUserData_Clicked）
        // 会自动在 Unity 主线程上继续执行。
    }
}