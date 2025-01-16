using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UserLogin.Demo
{
    public class Demo : MonoBehaviour
    {
        private void Awake()
        {
            AvatarData.Load();
        }

        void Start()
        {
            //Check if already connected
            RefreshLogin();
        }

        private async void RefreshLogin()
        {
            LoginResponse res = await ApiClient.Get().RefreshLogin();
            string obj = JsonUtility.ToJson(res);
            Debug.Log(obj);
            if (res.success)
            {
                MainPanel.Get().Show();
            }
            else
            {
                LoginPanel.Get().Show();
            }
        }
    }
}
