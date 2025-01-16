using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UserLogin.Demo
{

    public class FriendLine : MonoBehaviour
    {
        public Text username;
        public Image avatar;

        public Image online_img;
        public Sprite online_sprite;
        public Sprite offline_sprite;
        public Button accept_btn;
        public Button reject_btn;

        public UnityAction<FriendLine> onClick;
        public UnityAction<FriendLine> onClickAccept;
        public UnityAction<FriendLine> onClickReject;

        private FriendData fdata;

        void Start()
        {
            if (accept_btn != null)
                accept_btn.onClick.AddListener(() => { onClickAccept?.Invoke(this); });
            if(reject_btn != null)
                reject_btn.onClick.AddListener(() => { onClickReject?.Invoke(this); });
        }

        public void SetLine(FriendData user, bool online, bool buttons = false)
        {
            fdata = user;
            username.text = user.username;

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

            if (accept_btn != null)
                accept_btn.gameObject.SetActive(buttons);
            if (reject_btn != null)
                reject_btn.gameObject.SetActive(buttons);

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

        public FriendData GetFriend()
        {
            return fdata;
        }
    }
}
