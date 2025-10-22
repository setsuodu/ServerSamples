using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[System.Serializable]
public class PlayerData
{
    public string id;
    public string name;
    public int level;
    public int gold;
}

public class PlayerInfoFetcher : MonoBehaviour
{
    //private const string baseUrl = "https://localhost:7081/api/player/";
    private const string baseUrl = "http://localhost:5025/api/player/";

    void Start()
    {
        StartCoroutine(GetPlayerInfo("123"));
    }

    IEnumerator GetPlayerInfo(string playerId)
    {
        using (UnityWebRequest req = UnityWebRequest.Get(baseUrl + playerId))
        {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var json = req.downloadHandler.text;
                PlayerData player = JsonUtility.FromJson<PlayerData>(json);
                Debug.Log($"✅ Player: {player.name}, Lv:{player.level}, Gold:{player.gold}");
            }
            else
            {
                Debug.LogError($"❌ Error: {req.error}");
            }
        }
    }
}
