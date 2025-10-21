using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json; // 需要安装 Newtonsoft.Json（Unity包管理器可装）

public class AuthManager : MonoBehaviour
{
    private string baseUrl = "http://localhost:5015/api/auth";
    public string JwtToken { get; private set; }

    public IEnumerator Login(string username, string password)
    {
        var body = new { Username = username, Password = password };
        string json = JsonConvert.SerializeObject(body);

        using (UnityWebRequest request = new UnityWebRequest(baseUrl + "/login", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonConvert.DeserializeObject<LoginResponse>(request.downloadHandler.text);
                JwtToken = response.token;
                Debug.Log("JWT: " + JwtToken);
            }
            else
            {
                Debug.LogError("Login failed: " + request.error);
            }
        }
    }

    public IEnumerator GetProtectedData()
    {
        using (UnityWebRequest request = new UnityWebRequest("http://localhost:5015/api/values", "GET"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + JwtToken);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                Debug.Log("Data: " + request.downloadHandler.text);
            else
                Debug.LogError("Error: " + request.error);
        }
    }

    public InputField m_usernameInput;
    public InputField m_passwordInput;
    public void OnLoginClick()
    {
        StartCoroutine(Login(m_usernameInput.text, m_passwordInput.text));
    }
    public void OnGetProtectedData()
    {
        StartCoroutine(GetProtectedData());
    }
}

[System.Serializable]
public class LoginResponse
{
    public string token;
}