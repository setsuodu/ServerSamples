using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UserLogin
{
    [CreateAssetMenu(fileName = "ApiData", menuName = "UserLogin/ApiData", order = 5)]
    public class ApiData : ScriptableObject
    {
        [Header("API")]
        public string api_url;
        public string api_version;
        public bool api_https = false;

        public static ApiData Get()
        {
            return ApiClient.Get().data;
        }
    }
}
