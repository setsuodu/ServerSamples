using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UserLogin.Demo
{

    public class LoginPanel : UIPanel
    {
        [Header("Login")]
        public InputField login_user;
        public InputField login_password;
        public Text login_error;

        [Header("Register")]
        public UIPanel register_panel;
        public InputField register_user;
        public InputField register_email;
        public InputField register_password;
        public InputField register_confirm;
        public Text register_error;

        [Header("Recovery")]
        public UIPanel reset_panel;
        public UIPanel reset_confirm_panel;
        public InputField reset_email;
        public InputField reset_code;
        public InputField reset_password;
        public InputField reset_confirm;
        public Text reset_error;
        public Text reset_confirm_error;

        private const string user_save_id = "ul_user_save";
        
        private static LoginPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        public virtual void RefreshPanel()
        {
            register_panel.Hide(true);
            login_error.text = "";
            register_error.text = "";
            login_password.text = "";
            register_user.text = "";
            register_email.text = "";
            register_password.text = "";
            register_confirm.text = "";

            reset_panel.Hide(true);
            reset_confirm_panel.Hide(true);
            reset_email.text = "";
            reset_code.text = "";
            reset_password.text = "";
            reset_confirm.text = "";

            login_user.text = PlayerPrefs.GetString(user_save_id, "");
            if(string.IsNullOrEmpty(login_user.text))
                Select(login_user.gameObject);
            else
                Select(login_password.gameObject);
        }

        protected override void Update()
        {
            base.Update();

            if (!IsVisible())
                return;

            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (register_panel.IsVisible())
                    OnClickRegister();
                else
                    OnClickLogin();
            }
        }

        public async void Login()
        {
            if (ApiClient.Get().IsBusy())
                return;

            login_error.text = "";

            string user = login_user.text; //Can be email or username
            string password = login_password.text;

            LoginResponse res = await ApiClient.Get().Login(user, password);
            if (!res.success)
            {
                login_error.text = res.error;
            }
            else
            {
                PlayerPrefs.SetString(user_save_id, res.username);
                MainPanel.Get().Show();
                Hide();
            };
        }

        public async void Register()
        {
            if (ApiClient.Get().IsBusy())
                return;

            register_error.text = "";

            if (register_user.text.Length == 0 || register_password.text.Length == 0 || register_email.text.Length == 0)
                return;

            if (register_password.text != register_confirm.text)
            {
                register_error.text = "Passwords don't match";
                return;
            }

            string user = register_user.text;
            string email = register_email.text;
            string password = register_password.text;

            RegisterResponse res = await ApiClient.Get().Register(email, user, password);
            if (!res.success)
            {
                register_error.text = res.error;
            }
            else
            {
                login_user.text = user;
                login_password.text = password;
                register_panel.Hide();
            }
        }

        public async void ResetPassword()
        {
            if (ApiClient.Get().IsBusy())
                return;

            reset_error.text = "";

            if (reset_email.text.Length == 0)
                return;

            ResetPasswordRequest req = new ResetPasswordRequest();
            req.email = reset_email.text;

            string url = ApiClient.ServerURL + "/users/password/reset";
            string json = ApiTool.ToJson(req);
            WebResponse res = await ApiClient.Get().SendPostRequest(url, json);
            if (!res.success)
            {
                reset_error.text = res.error;
            }
            else
            {
                reset_panel.Hide();
                reset_confirm_panel.Show();
            }
        }

        public async void ResetPasswordConfirm()
        {
            if (ApiClient.Get().IsBusy())
                return;

            reset_confirm_error.text = "";

            if (reset_code.text.Length == 0 || reset_password.text.Length == 0 || reset_confirm.text.Length == 0)
                return;

            if (reset_password.text != reset_confirm.text)
            {
                register_error.text = "Passwords don't match";
                return;
            }

            ResetConfirmPasswordRequest req = new ResetConfirmPasswordRequest();
            req.email = reset_email.text;
            req.code = reset_code.text;
            req.password = reset_password.text;

            string url = ApiClient.ServerURL + "/users/password/reset/confirm";
            string json = ApiTool.ToJson(req);
            WebResponse res = await ApiClient.Get().SendPostRequest(url, json);
            if (!res.success)
            {
                reset_confirm_error.text = res.error;
            }
            else
            {
                login_user.text = req.email;
                login_password.text = "";
                reset_panel.Hide();
                reset_confirm_panel.Hide();
            }
        }

        public void OnClickLogin()
        {
            Login();
        }

        public void OnClickRegister()
        {
            Register();
        }

        public void OnClickSwitchLogin()
        {
            register_panel.Hide();
            Select(login_user.gameObject);
        }

        public void OnClickSwitchRegister()
        {
            register_panel.Show();
            Select(register_user.gameObject);
        }

        public void OnClickSwitchReset()
        {
            reset_error.text = "";
            reset_confirm_error.text = "";
            reset_panel.Show();
            reset_confirm_panel.Hide();
        }

        public void OnClickResetOK()
        {
            ResetPassword();
        }

        public void OnClickResetConfirmOK()
        {
            ResetPasswordConfirm();
        }

        private void Select(GameObject obj)
        {
            EventSystem.current.SetSelectedGameObject(obj);
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshPanel();
        }

        public static LoginPanel Get()
        {
            return instance;
        }
    }
}
