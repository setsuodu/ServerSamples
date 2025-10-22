using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class AuthManager : MonoBehaviour
{
    public string AccessToken;
    public string RefreshToken;

    private string baseUrl = "http://localhost:5015/api/auth";


    // 登录获取令牌
    public IEnumerator Login(string username, string password)
    {
        var body = new { Username = username, Password = password };
        var json = JsonConvert.SerializeObject(body);
        using (var request = new UnityWebRequest(baseUrl + "/login", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonConvert.DeserializeObject<LoginResponse>(request.downloadHandler.text);
                AccessToken = response.AccessToken;
                RefreshToken = response.RefreshToken;
                Debug.Log("Login Success");
            }
            else
            {
                Debug.LogError("Login failed: " + request.error);
            }
        }
    }

    // 调用接口时自动刷新过期的 Token
    public IEnumerator GetProtectedData()
    {
        string url = "http://localhost:5015/api/values";

        using (var request = new UnityWebRequest(url, "GET"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + AccessToken);
            yield return request.SendWebRequest();

            if (request.responseCode == 401) // token失效
            {
                Debug.Log("Access token expired, refreshing...");
                yield return RefreshAccessToken();

                // 重试请求
                using (var retryRequest = new UnityWebRequest(url, "GET"))
                {
                    retryRequest.downloadHandler = new DownloadHandlerBuffer();
                    retryRequest.SetRequestHeader("Authorization", "Bearer " + AccessToken);
                    yield return retryRequest.SendWebRequest();

                    if (retryRequest.result == UnityWebRequest.Result.Success)
                        Debug.Log("Data: " + retryRequest.downloadHandler.text);
                    else
                        Debug.LogError("Error after refresh: " + retryRequest.error);
                }
            }
            else
            {
                Debug.Log("Data: " + request.downloadHandler.text);
            }
        }
    }
    private IEnumerator RefreshAccessToken()
    {
        var body = new { RefreshToken = RefreshToken };
        var json = JsonConvert.SerializeObject(body);
        using (var request = new UnityWebRequest(baseUrl + "/refresh", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonConvert.DeserializeObject<LoginResponse>(request.downloadHandler.text);
                AccessToken = response.AccessToken;
                RefreshToken = response.RefreshToken;
                Debug.Log("Access token refreshed");
            }
            else
            {
                Debug.LogError("Failed to refresh token: " + request.error);
            }
        }
    }


    #region GUI Test
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
    #endregion
}
