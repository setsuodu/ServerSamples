// 由于Unity的API（如Debug.Log）只能在主线程调用，我们需要一个主线程分发器来处理回调。
// 以下是一个简单的实现：
using System;
using System.Collections.Concurrent;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher instance;
    private readonly ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();

    public static UnityMainThreadDispatcher Instance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<UnityMainThreadDispatcher>();
            if (instance == null)
            {
                var go = new GameObject("UnityMainThreadDispatcher");
                instance = go.AddComponent<UnityMainThreadDispatcher>();
                DontDestroyOnLoad(go);
            }
        }
        return instance;
    }

    public void Enqueue(Action action)
    {
        actions.Enqueue(action);
    }

    void Update()
    {
        while (actions.TryDequeue(out Action action))
        {
            action?.Invoke();
        }
    }
}