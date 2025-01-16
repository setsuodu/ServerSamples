using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace UserLogin
{
    /// <summary>
    /// Auto-refresh user login
    /// </summary>

    public class ApiAutoConnect : MonoBehaviour
    {
        public bool save_login = true;

        public const string user_id_id = "ul_user_id";
        public const string username_id = "ul_username";
        public const string access_token_id = "ul_access_token";
        public const string refresh_token_id = "ul_refresh_token";
        public const float refresh_interval = 300f; //5 min

        private static ApiAutoConnect _instance;

        private float refresh_timer = 0f;
        private float valid_timer = 0f;
        private bool was_logged_in = false;

        void Awake()
        {
            _instance = this;
            Load();
        }

        private void Start()
        {
            ApiClient client = ApiClient.Get();
            if (client != null)
            {
                client.onLogin += OnLogin;
                client.onLogout += OnLogout;
            }
        }

        private void OnDestroy()
        {
            ApiClient client = ApiClient.Get();
            if (client != null)
            {
                client.onLogin -= OnLogin;
                client.onLogout -= OnLogout;
            }
        }

        private void Load()
        {
            ApiClient client = ApiClient.Get();
            if (!client.IsLoggedIn() && save_login)
            {
                client.UserID = PlayerPrefs.GetString(user_id_id);
                client.Username = PlayerPrefs.GetString(username_id);
                client.AccessToken = PlayerPrefs.GetString(access_token_id);
                client.RefreshToken = PlayerPrefs.GetString(refresh_token_id);
            }
        }

        private void Save()
        {
            ApiClient client = ApiClient.Get();
            if (client.IsLoggedIn() && save_login)
            {
                PlayerPrefs.SetString(user_id_id, client.UserID);
                PlayerPrefs.SetString(username_id, client.Username);
                PlayerPrefs.SetString(access_token_id, client.AccessToken);
                PlayerPrefs.SetString(refresh_token_id, client.RefreshToken);
            }
        }

        private void OnLogin(LoginResponse res)
        {
            if (res.success)
            {
                Save();
            }
        }

        private void OnLogout()
        {
            was_logged_in = false;
        }

        private async void Update()
        {
            ApiClient client = ApiClient.Get();

            //Auto refresh login
            refresh_timer += Time.deltaTime;
            if (client.IsLoggedIn() && refresh_timer > refresh_interval)
            {
                refresh_timer = 0f;
                await client.RefreshLogin();
            }

            //Try loggin again if disconnected
            if (valid_timer > 5f && !client.IsLoggedIn() && was_logged_in)
            {
                valid_timer = 0f;
                refresh_timer = 0f;
                await client.RefreshLogin();
            }

            //Check if login still valid
            valid_timer += Time.deltaTime;
            if (valid_timer > 30f && client.IsLoggedIn())
            {
                valid_timer = 0f;
                await client.Validate();
            }

            was_logged_in = was_logged_in || client.IsLoggedIn();
        }

        public static ApiAutoConnect Get()
        {
            return _instance;
        }
    }
}
