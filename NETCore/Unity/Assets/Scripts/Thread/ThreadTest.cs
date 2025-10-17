using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ThreadTest : MonoBehaviour
{
    private CancellationTokenSource cancellationTokenSource;

    void Start()
    {
        // 创建 CancellationTokenSource
        cancellationTokenSource = new CancellationTokenSource();

        // 启动线程或任务
        Task.Run(() => RunThread(cancellationTokenSource.Token));
    }

    // 线程执行的函数
    private void RunThread(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            // 模拟长时间运行的任务
            Debug.Log("Thread is running...");
            Thread.Sleep(1000); // 每秒打印一次

            // 检查是否被取消
            if (token.IsCancellationRequested)
            {
                Debug.Log("Thread cancellation requested!");
                return; // 退出线程
            }
        }
    }

    void Update()
    {
        // 按下空格键取消线程
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                Debug.Log("Cancellation requested!");
            }
        }
    }

    void OnDestroy()
    {
        // 清理资源
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel(); // 确保线程在销毁时停止
            cancellationTokenSource.Dispose(); // 释放 CancellationTokenSource
            cancellationTokenSource = null;
        }
    }
}