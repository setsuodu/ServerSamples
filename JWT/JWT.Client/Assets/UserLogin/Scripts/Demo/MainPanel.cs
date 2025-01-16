using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UserLogin.Demo
{
    public class MainPanel : UIPanel
    {
        public UIPanel play_panel;
        public Text username_txt;
        public Text xp_txt;
        public Image avatar_img;

        private static MainPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        public virtual void RefreshPanel()
        {
            username_txt.text = ApiClient.Get().Username;
            xp_txt.text = "";
            RefreshUserData();
        }

        public async void RefreshUserData()
        {
            UserData udata = await ApiClient.Get().RefreshUserData();
            if (udata != null)
            {
                xp_txt.text = udata.xp.ToString();

                AvatarData avatar = AvatarData.Get(udata.avatar);
                if (avatar != null)
                    avatar_img.sprite = avatar.avatar;
            }
        }

        public void OnClickPlay()
        {
            play_panel.Show();
        }

        public void OnClickAccount()
        {
            UserPanel.Get().Show();
        }

        public void OnClickLogout()
        {
            ApiClient.Get().Logout();
            LoginPanel.Get().Show();
            UserPanel.Get().Hide();
            Hide();
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshPanel();
        }


        public static MainPanel Get()
        {
            return instance;
        }
    }
}
