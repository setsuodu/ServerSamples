using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UserLogin.Demo
{

    public class UserLine : MonoBehaviour
    {
        public Text username;
        public Text coins;
        public Text xp;
        public Text admin;
        public Image avatar;

        public Image online_img;
        public Sprite online_sprite;
        public Sprite offline_sprite;

        public UnityAction<UserLine> onClick;

        private UserData udata;

        void Start()
        {

        }

        public void SetLine(UserData user, bool online = false)
        {
            udata = user;
            username.text = user.username;

            if(coins != null)
                coins.text = user.coin.ToString() + " COINS";
            if (xp != null)
                xp.text = user.xp.ToString() + " XP";

            if (admin != null)
            {
                if (user.permission_level >= 10)
                    admin.text = "ADMIN";
                else if (user.permission_level >= 5)
                    admin.text = "SERVER";
                else
                    admin.text = "";

                if (user.permission_level <= 0)
                    admin.text = "DISABLED";
            }

            if (avatar != null)
            {
                AvatarData avat = AvatarData.Get(user.avatar);
                if (avat != null)
                    avatar.sprite = avat.avatar;
            }

            if (online_img != null)
            {
                online_img.sprite = online ? online_sprite : offline_sprite;
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void OnClick()
        {
            onClick?.Invoke(this);
        }

        public UserData GetUser()
        {
            return udata;
        }
    }
}
