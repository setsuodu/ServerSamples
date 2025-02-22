using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UserLogin.Demo
{

    public class FriendPanel : UIPanel
    {
        public ScrollRect friend_scroll;
        public RectTransform friend_content;
        public FriendLine line_prefab;
        public InputField friend_input;
        public TabButton friends_tab;
        public TabButton requests_tab;
        public int online_duration = 10; //In minutes
        public Text error;

        private List<FriendLine> friend_lines = new List<FriendLine>();

        protected override void Awake()
        {
            base.Awake();
            InitLines();

            friends_tab.onClick += RefreshPanel;
            requests_tab.onClick += RefreshPanel;
        }

        private void InitLines()
        {
            int nlines = 100;
            for (int i = 0; i < nlines; i++)
            {
                FriendLine line = AddLine(line_prefab, i);
                line.Hide();
                friend_lines.Add(line);
            }

            line_prefab.gameObject.SetActive(false);
            friend_scroll.verticalNormalizedPosition = 1f;
        }

        private FriendLine AddLine(FriendLine template, int index)
        {
            Vector2 pos = template.GetComponent<RectTransform>().anchoredPosition;
            GameObject line = Instantiate(template.gameObject, friend_content);
            RectTransform rtrans = line.GetComponent<RectTransform>();
            FriendLine rline = line.GetComponent<FriendLine>();
            rline.onClick += OnClickFriendLine;
            rline.onClickAccept += OnClickFriendAccept;
            rline.onClickReject += OnClickFriendReject;
            return rline;
        }

        private async void RefreshPanel()
        {
            foreach (FriendLine line in friend_lines)
                line.Hide();

            string url = ApiClient.ServerURL + "/users/friends/list";
            WebResponse res = await ApiClient.Get().SendGetRequest(url);
            if (res.success)
            {
                FriendResponse contract_list = ApiTool.JsonToObject<FriendResponse>(res.data);
                if (friends_tab.active)
                    SetFriends(contract_list);
                else if (requests_tab.active)
                    SetRequests(contract_list);
            }
        }

        private void SetFriends(FriendResponse contract_list)
        {
            DateTime server_time = DateTime.Parse(contract_list.server_time);
            DateTime login_time = server_time.AddMinutes(-online_duration);

            int index = 0;
            foreach (FriendData user in contract_list.friends)
            {
                if (index < friend_lines.Count)
                {
                    FriendLine line = friend_lines[index];
                    DateTime last_login = DateTime.Parse(user.last_login_time);
                    bool online = last_login > login_time;
                    line.SetLine(user, online);
                }
                index++;
            }
        }

        private void SetRequests(FriendResponse contract_list)
        {
            DateTime server_time = DateTime.Parse(contract_list.server_time);
            DateTime login_time = server_time.AddMinutes(-10);

            int index = 0;
            foreach (FriendData user in contract_list.friends_requests)
            {
                if (index < friend_lines.Count)
                {
                    FriendLine line = friend_lines[index];
                    DateTime last_login = DateTime.Parse(user.last_login_time);
                    bool online = last_login > login_time;
                    line.SetLine(user, online, true);
                }
                index++;
            }
        }

        private async void AddFriend(string fuser)
        {
            ContactAddRequest req = new ContactAddRequest();
            req.username = fuser;

            string url = ApiClient.ServerURL + "/users/friends/add";
            string json = ApiTool.ToJson(req);

            WebResponse res = await ApiClient.Get().SendPostRequest(url, json);
            if (res.success)
            {
                RefreshPanel();
            }
            else
            {
                error.text = res.error;
            }
        }

        private async void RemoveFriend(string fuser)
        {
            ContactAddRequest req = new ContactAddRequest();
            req.username = fuser;

            string url = ApiClient.ServerURL + "/users/friends/remove";
            string json = ApiTool.ToJson(req);

            WebResponse res = await ApiClient.Get().SendPostRequest(url, json);
            if (res.success)
            {
                RefreshPanel();
            }
            else
            {
                error.text = res.error;
            }
        }

        public void OnClickBack()
        {
            Hide();
        }

        private void OnClickFriendLine(FriendLine user)
        {

        }

        private void OnClickFriendAccept(FriendLine user)
        {
            FriendData friend = user.GetFriend();
            AddFriend(friend.username);
        }

        private void OnClickFriendReject(FriendLine user)
        {
            FriendData friend = user.GetFriend();
            RemoveFriend(friend.username);
        }

        public void OnClickAddFriend()
        {
            string fuser = friend_input.text;
            if (string.IsNullOrWhiteSpace(fuser))
                return;

            error.text = "";
            AddFriend(fuser);
        }

        public void OnClickRemoveFriend()
        {
            string fuser = friend_input.text;
            if (string.IsNullOrWhiteSpace(fuser))
                return;

            error.text = "";
            RemoveFriend(fuser);
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            error.text = "";
            friend_input.text = "";
            friends_tab.Activate();
            RefreshPanel();
        }
    }
}
