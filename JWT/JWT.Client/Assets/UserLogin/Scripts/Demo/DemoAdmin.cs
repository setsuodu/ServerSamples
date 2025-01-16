using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UserLogin.Demo
{
    /// <summary>
    /// Demo scene to perform admin-only operations, these could be called on your game server instead
    /// </summary>

    public class DemoAdmin : MonoBehaviour
    {
        public string admin_user;
        public string admin_password;

        [Header("List UI")]
        public ScrollRect scroll_view;
        public RectTransform scroll_content;
        public UserLine line_prefab;
        public UIPanel warning_panel;
        public float line_spacing = 110f;

        [Header("Edit UI")]
        public UIPanel edit_panel;
        public Text username_txt;
        public Image avatar_img;
        public Text coins;
        public Text xp;
        public Text admin;
        public InputField edit_coins;
        public InputField edit_xp;
        public InputField edit_admin;
        public Text error;

        private UserData udata;

        private List<UserLine> lines = new List<UserLine>();

        private void Awake()
        {
            AvatarData.Load();
            error.text = "";
        }

        void Start()
        {
            InitLines();
            RefreshLogin();
        }

        private void InitLines()
        {
            int nlines = 100;

            for (int i = 0; i < nlines; i++)
            {
                UserLine line = AddLine(line_prefab, i);
                line.Hide();
                lines.Add(line);
            }

            line_prefab.gameObject.SetActive(false);
            scroll_content.sizeDelta = new Vector2(scroll_content.sizeDelta.x, nlines * line_spacing + 20f);
            scroll_view.verticalNormalizedPosition = 1f;
        }

        private UserLine AddLine(UserLine template, int index)
        {
            Vector2 pos = template.GetComponent<RectTransform>().anchoredPosition;
            GameObject line = Instantiate(template.gameObject, scroll_content);
            RectTransform rtrans = line.GetComponent<RectTransform>();
            UserLine rline = line.GetComponent<UserLine>();
            rtrans.anchoredPosition = pos + Vector2.down * index * line_spacing;
            rline.onClick += OnClickLine;
            return rline;
        }

        private async void RefreshLogin()
        {
            LoginResponse res = await ApiClient.Get().Login(admin_user, admin_password);
            if (res.success && res.permission_level >= 7)
            {
                RefreshList();
            }
            else
            {
                warning_panel.Show();
            }
        }

        private async void RefreshList()
        {
            string url = ApiClient.ServerURL + "/users";
            WebResponse res = await ApiClient.Get().SendGetRequest(url);
            ListResponse<UserData> list = ApiTool.JsonToArray<UserData>(res.data);
            UserData[] users = list.list;

            foreach (UserLine line in lines)
                line.Hide();

            int index = 0;
            foreach (UserData user in users)
            {
                if (index < lines.Count)
                {
                    UserLine line = lines[index];
                    line.SetLine(user);
                }
                index++;
            }
        }

        private void ShowUser(UserData user)
        {
            udata = user;
            RefreshUser(user);
        }

        private void RefreshUser(UserData user)
        {
            username_txt.text = user.username;
            coins.text = user.coin.ToString() + " COINS";
            xp.text = user.xp.ToString() + " XP";

            AvatarData avatar = AvatarData.Get(udata.avatar);
            if (avatar != null)
                avatar_img.sprite = avatar.avatar;

            UpdateAdmin(user);

            error.text = "";
            edit_panel.Show();
        }

        private void UpdateAdmin(UserData user)
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

        private void OnClickLine(UserLine udata)
        {
            ShowUser(udata.GetUser());
        }

        public void OnClickAddCoin()
        {
            bool success = int.TryParse(edit_coins.text, out int coin);
            if (success)
            {
                SendReward(coin, 0);
            }
        }

        public void OnClickAddXP()
        {
            bool success = int.TryParse(edit_xp.text, out int xp);
            if (success)
            {
                SendReward(0, xp);
            }
        }

        public void OnClickChangePermission()
        {
            bool success = int.TryParse(edit_admin.text, out int perm);
            if (success)
            {
                SendPermission(perm);
            }
        }

        private async void SendReward(int coins, int xp)
        {
            GainRewardRequest req = new GainRewardRequest();
            req.coin = coins;
            req.xp = xp;

            string url = ApiClient.ServerURL + "/users/rewards/gain/" + udata.id;
            string json = ApiTool.ToJson(req);
            error.text = "";

            WebResponse res = await ApiClient.Get().SendPostRequest(url, json);
            if (res.success)
            {
                UserData user = ApiTool.JsonToObject<UserData>(res.data);
                RefreshUser(user);
            }
            else
            {
                error.text = res.error;
            }
        }

        // 1=USER  5=SERVER   10=ADMIN
        private async void SendPermission(int permission)
        {
            EditPermissionRequest req = new EditPermissionRequest();
            req.permission_level = permission;

            string url = ApiClient.ServerURL + "/users/permission/edit/" + udata.id;
            string json = ApiTool.ToJson(req);
            error.text = "";

            WebResponse res = await ApiClient.Get().SendPostRequest(url, json);
            if (res.success)
            {
                udata.permission_level = permission;
                UpdateAdmin(udata);
            }
            else
            {
                error.text = res.error;
            }
        }

        public void OnClickBack()
        {
            RefreshList();
            edit_panel.Hide();
        }
    }
}
