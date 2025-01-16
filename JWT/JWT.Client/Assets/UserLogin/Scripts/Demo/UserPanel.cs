using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UserLogin.Demo
{
    public class UserPanel : UIPanel
    {
        public Text username_txt;
        public Image avatar_img;
        public Text coins;
        public Text xp;

        [Header("Edit")]
        public InputField edit_email;
        public Button edit_email_save;
        public InputField edit_password_prev;
        public InputField edit_password_new;
        public InputField edit_password_confirm;
        public Button edit_password_save;
        public Text edit_success;
        public Text edit_error;

        [Header("Avatar")]
        public UIPanel avatar_panel;
        public AvatarIcon[] avatar_icons;

        [Header("Other")]
        public FriendPanel friend_panel;

        private string prev_email;

        private static UserPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;

            foreach (AvatarIcon icon in avatar_icons)
                icon.onClick += OnSelectAvatar;
        }

        protected virtual void RefreshPanel()
        {
            RefreshPanel(ApiClient.Get().UserData);
        }

        protected virtual void RefreshPanel(UserData udata)
        {
            username_txt.text = udata.username;
            coins.text = udata.coin.ToString() + " COINS";
            xp.text = udata.xp.ToString() + " XP";
            edit_email.text = udata.email;
            prev_email = udata.email;

            edit_password_prev.text = ""; 
            edit_password_new.text = ""; 
            edit_password_confirm.text = "";
            edit_success.text = "";
            edit_error.text = "";

            edit_email_save.interactable = true;
            edit_password_save.interactable = true;

            AvatarData avatar = AvatarData.Get(udata.avatar);
            if (avatar != null)
                avatar_img.sprite = avatar.avatar;
        }

        public async void RefreshUserData()
        {
            UserData udata = await ApiClient.Get().RefreshUserData();
            if (udata != null)
            {
                RefreshPanel();
                MainPanel.Get().RefreshPanel();
            }
        }

        public async void SaveEmail()
        {
            edit_success.text = "";
            edit_error.text = "";

            if (edit_email.text.Length == 0)
                return;

            if (edit_email.text == prev_email)
                return;

            UserData udata = ApiClient.Get().UserData;
            EditUserRequest req = new EditUserRequest();
            req.email = edit_email.text;
            edit_email_save.interactable = false;

            string url = ApiClient.ServerURL + "/users/email/edit";
            string data = ApiTool.ToJson(req);

            WebResponse res = await ApiClient.Get().SendPostRequest(url, data);
            if (res.success)
            {
                udata.email = req.email;
                prev_email = req.email;
                RefreshPanel();
                edit_success.text = "Email Changed!";
            }
            else
            {
                edit_email_save.interactable = true;
                edit_error.text = res.GetError();
            }
        }

        public async void SavePassword()
        {
            edit_success.text = "";
            edit_error.text = "";

            if (edit_password_prev.text.Length == 0 || edit_password_new.text.Length == 0 || edit_password_confirm.text.Length == 0)
                return;

            if (edit_password_new.text != edit_password_confirm.text)
            {
                edit_error.text = "Passwords don't match!";
                return;
            }

            EditPasswordRequest req = new EditPasswordRequest();
            req.password_previous = edit_password_prev.text;
            req.password_new = edit_password_new.text;
            edit_password_save.interactable = false;

            string url = ApiClient.ServerURL + "/users/password/edit";
            string data = ApiTool.ToJson(req);
            WebResponse res = await ApiClient.Get().SendPostRequest(url, data);
            if (res.success)
            {
                RefreshPanel();
                edit_success.text = "Password Changed!";
            }
            else
            {
                edit_password_save.interactable = true;
                edit_error.text = res.GetError();
            }
        }

        public async void SelectAvatar(AvatarIcon icon)
        {
            edit_success.text = "";
            edit_error.text = "";
            avatar_panel.Hide();

            UserData udata = ApiClient.Get().UserData;
            AvatarData avatar = icon.GetData();
            if (avatar != null)
            {
                udata.avatar = avatar.id;

                EditUserRequest req = new EditUserRequest();
                req.avatar = avatar.id;

                string url = ApiClient.ServerURL + "/users/edit/" + ApiClient.Get().UserID;
                string data = ApiTool.ToJson(req);

                WebResponse res = await ApiClient.Get().SendPostRequest(url, data);
                if (res.success)
                {
                    MainPanel.Get().RefreshPanel();
                    edit_success.text = "Avatar Changed!";
                }
                else
                {
                    edit_error.text = res.GetError();
                }

                RefreshPanel();
            }
        }

        public async void ReloadUser()
        {
            await ApiClient.Get().RefreshUserData();
            RefreshPanel();
        }

        public void OnClickSaveEmail()
        {
            SaveEmail();
        }

        public void OnClickSavePassword()
        {
            SavePassword();
        }

        public void OnClickAvatar()
        {
            edit_success.text = "";
            edit_error.text = "";
            avatar_panel.Show();

            foreach (AvatarIcon icon in avatar_icons)
                icon.Hide();

            int index = 0;
            foreach (AvatarData avatar in AvatarData.GetAll())
            {
                if (index < avatar_icons.Length && !string.IsNullOrEmpty(avatar.id))
                {
                    AvatarIcon icon = avatar_icons[index];
                    icon.SetAvatar(avatar);
                    index++;
                }
            }
        }

        public void OnSelectAvatar(AvatarIcon icon)
        {
            SelectAvatar(icon);
        }

        public void OnClickFriends()
        {
            friend_panel.Show();
        }

        public void OnClickBack()
        {
            MainPanel.Get().Show();
            Hide();
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshPanel();
        }

        public override void Hide(bool instant = false)
        {
            base.Hide(instant);
            friend_panel.Hide();
        }

        public static UserPanel Get()
        {
            return instance;
        }
    }
}
