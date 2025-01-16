using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.Events;

namespace UserLogin
{
    public class ApiClient : MonoBehaviour
    {
        public ApiData data;

        public UnityAction<RegisterResponse> onRegister; //Triggered after register, even if failed
        public UnityAction<LoginResponse> onLogin; //Triggered after login, even if failed
        public UnityAction<LoginResponse> onRefresh; //Triggered after login refresh, even if failed 
        public UnityAction onLogout; //Triggered after logout

        private string user_id = "";
        private string username = "";
        private string access_token = "";
        private string refresh_token = "";
        private string api_version = "";

        private bool logged_in = false;
        private UserData udata = null;
        private int sending = 0;
        private string last_error = "";

        private float refresh_timer = 0f;
        private float expiration_timer = 0f;
        private float expiration_duration = 1f;
        private bool expired = false;

        private static ApiClient instance;

        void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            if (logged_in)
            {
                //Check expiration
                if (!expired)
                {
                    expiration_timer += Time.deltaTime;
                    expired = expiration_timer > (expiration_duration - 10f);
                }

                //Every 5 seconds, check connection status
                refresh_timer += Time.deltaTime;
                if (refresh_timer >= 5f)
                {
                    refresh_timer = 0f;
                    Refresh();
                }
            }
        }

        private async void Refresh()
        {
            if (expired)
                await RefreshLogin(); //Try to relogin
            else
                await Validate(); //Check if expired
        }

        public async Task<RegisterResponse> Register(string email, string user, string password)
        {
            RegisterRequest data = new RegisterRequest();
            data.email = email;
            data.username = user;
            data.password = password;
            data.avatar = "";
            return await Register(data);
        }

        public async Task<RegisterResponse> Register(RegisterRequest data)
        {
            Logout(); //Disconnect

            string url = ServerURL + "/users/register";
            string json = ApiTool.ToJson(data);

            WebResponse res = await SendRequest(url, WebRequest.METHOD_POST, json);
            RegisterResponse regist_res = ApiTool.JsonToObject<RegisterResponse>(res.data);
            regist_res.success = res.success;
            regist_res.error = res.error;
            onRegister?.Invoke(regist_res);
            return regist_res;
        }

        public async Task<LoginResponse> Login(string user, string password)
        {
            Logout(); //Disconnect

            LoginRequest data = new LoginRequest();
            data.password = password;

            if (user.Contains("@"))
                data.email = user;
            else
                data.username = user;

            string url = ServerURL + "/auth";
            string json = ApiTool.ToJson(data);

            WebResponse res = await SendRequest(url, WebRequest.METHOD_POST, json);
            LoginResponse login_res = ApiTool.JsonToObject<LoginResponse>(res.data);
            login_res.success = res.success;
            login_res.error = res.error;
            logged_in = res.success;

            if (res.success)
            {
                user_id = login_res.id;
                username = login_res.username;
                access_token = login_res.access_token;
                refresh_token = login_res.refresh_token;
                api_version = login_res.version;
                expired = false;
                expiration_timer = 0f;
                expiration_duration = login_res.duration;
            }

            onLogin?.Invoke(login_res);
            return login_res;
        }

        public async Task<LoginResponse> RefreshLogin()
        {
            string url = ServerURL + "/auth/refresh";
            AutoLoginRequest data = new AutoLoginRequest();
            data.refresh_token = refresh_token;
            string json = ApiTool.ToJson(data);

            WebResponse res = await SendRequest(url, WebRequest.METHOD_POST, json);
            LoginResponse login_res = ApiTool.JsonToObject<LoginResponse>(res.data);
            login_res.success = res.success;
            login_res.error = res.error;
            logged_in = res.success;

            if (res.success)
            {
                user_id = login_res.id;
                username = login_res.username;
                access_token = login_res.access_token;
                refresh_token = login_res.refresh_token;
                api_version = login_res.version;
                expired = false;
                expiration_timer = 0f;
                expiration_duration = login_res.duration;
            }

            onRefresh?.Invoke(login_res);
            return login_res;
        }

        public async Task<UserData> RefreshUserData()
        {
            udata = await RefreshUserData(this.username);
            return udata;
        }

        public async Task<UserData> RefreshUserData(string username)
        {
            if (!logged_in)
                return null;

            string url = ServerURL + "/users/" + username;
            WebResponse res = await SendRequest(url, WebRequest.METHOD_GET, "");

            UserData udata = null;
            if (res.success)
            {
                udata = ApiTool.JsonToObject<UserData>(res.data);
            }

            return udata;
        }

        public async Task<bool> Validate()
        {
            if (!IsConnected())
                return false;

            //Check if connection is still valid
            string url = ServerURL + "/auth/validate";
            WebResponse res = await SendGetRequest(url);
            expired = !res.success;
            return res.success;
        }

        public void Logout()
        {
            user_id = "";
            username = "";
            access_token = "";
            refresh_token = "";
            api_version = "";
            last_error = "";
            logged_in = false;
            onLogout?.Invoke();
        }

        public async Task<string> SendGetVersion()
        {
            string url = ServerURL + "/version";
            WebResponse res = await SendRequest(url, WebRequest.METHOD_GET, "");

            if (res.success)
            {
                VersionResponse version_data = ApiTool.JsonToObject<VersionResponse>(res.data);
                api_version = version_data.version;
                return api_version;
            }

            return null;
        }

        public async Task<WebResponse> SendGetRequest(string url)
        {
            return await SendRequest(url, WebRequest.METHOD_GET);
        }

        public async Task<WebResponse> SendPostRequest(string url, string json_data)
        {
            return await SendRequest(url, WebRequest.METHOD_POST, json_data);
        }

        public async Task<WebResponse> SendRequest(string url, string method, string json_data = "")
        {
            UnityWebRequest request = WebRequest.Create(url, method, json_data, access_token);
            return await SendRequest(request);
        }

        private async Task<WebResponse> SendRequest(UnityWebRequest request)
        {
            var async_oper = request.SendWebRequest();
            sending++;

            while (!async_oper.isDone)
                await Task.Yield();

            sending--;

            WebResponse response = WebRequest.GetResponse(request);
            response.error = GetError(response);
            last_error = response.error;
            request.Dispose();

            return response;
        }

        private string GetError(WebResponse res)
        {
            if (res.success)
                return "";

            ErrorResponse err = ApiTool.JsonToObject<ErrorResponse>(res.data);
            if (err != null)
                return err.error;
            else
                return res.error;
        }

        public bool IsConnected()
        {
            return logged_in && !expired;
        }

        public bool IsLoggedIn()
        {
            return logged_in;
        }

        public bool IsExpired()
        {
            return expired;
        }

        public bool IsBusy()
        {
            return sending > 0;
        }

        public string GetLastRequest()
        {
            return last_error;
        }

        public string GetLastError()
        {
            return last_error;
        }

        public string GetError() => GetLastError();

        public UserData UserData { get { return udata; } }

        public string UserID { get { return user_id; } set { user_id = value; } }
        public string Username { get { return username; } set { username = value; } }
        public string AccessToken { get { return access_token; } set { access_token = value; } }
        public string RefreshToken { get { return refresh_token; } set { refresh_token = value; } }

        public string ServerVersion { get { return api_version; } }
        public string ClientVersion { get { return ApiData.Get().api_version; }}

        public static string ServerURL
        {
            get
            {
                ApiData data = ApiData.Get();
                string protocol = data.api_https ? "https://" : "http://";
                return protocol + data.api_url;
            }
        }

        public static ApiClient Get()
        {
            if (instance == null)
                instance = FindObjectOfType<ApiClient>();
            return instance;
        }
    }
}
