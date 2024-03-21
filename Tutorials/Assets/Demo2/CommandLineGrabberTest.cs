using UnityEngine;
using UnityEngine.UI;

public class CommandLineGrabberTest : MonoBehaviour
{
    public Text m_portText;

    void Start()
    {
        Debug.Log("CommandLineGrabberTest init");
        Debug.Log("grabbed port -> " + CommandLineGrabber.GetArg("-port"));

        m_portText.text = $"grabbed port -> {CommandLineGrabber.GetArg("-port")}";
    }
}

public class CommandLineGrabber
{
    // Helper function for getting the command line arguments
    public static string GetArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            Debug.Log($"[{i}]---{args[i]}");
            if (args[i] == name && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }
        return null;
    }
}