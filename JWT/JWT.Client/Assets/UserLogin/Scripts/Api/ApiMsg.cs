using System;

namespace UserLogin
{
    //--------- Requests -----------

    [Serializable]
    public struct LoginRequest
    {
        public string email;
        public string username;
        public string password;
    }

    [Serializable]
    public struct AutoLoginRequest
    {
        public string refresh_token;
    }

    [Serializable]
    public struct RegisterRequest
    {
        public string email;
        public string username;
        public string password;
        public string avatar;
    }

    [Serializable]
    public class EditUserRequest
    {
        public string username;
        public string email;
        public string avatar;
    }

    [Serializable]
    public class EditPermissionRequest
    {
        public int permission_level;
    }

    [Serializable]
    public class EditPasswordRequest
    {
        public string password_previous;
        public string password_new;
    }

    [Serializable]
    public class ResetPasswordRequest
    {
        public string email;
    }

    [Serializable]
    public class ResetConfirmPasswordRequest
    {
        public string email;
        public string code;
        public string password;
    }

    [Serializable]
    public class GainRewardRequest
    {
        public int coin;
        public int xp;
    }

    [Serializable]
    public class ContactAddRequest
    {
        public string username;
    }

    //--------- Response -----------

    [Serializable]
    public struct VersionResponse
    {
        public string version;
    }

    [Serializable]
    public struct IdResponse
    {
        public string id;
    }

    [Serializable]
    public struct LoginResponse
    {
        public string id;
        public string username;
        public string refresh_token;
        public string access_token;
        public int permission_level;
        public int validation_level;
        public int duration;
        public string version;
        public string error;
        public bool success;
    }

    [Serializable]
    public struct RegisterResponse
    {
        public string id;
        public string username;
        public string error;
        public bool success;
    }

    [System.Serializable]
    public class UserData
    {
        public string id;
        public string username;
        public string email;

        public string avatar;
        public int coin;
        public int xp;

        public int permission_level;
        public int validation_level;
    }

    [Serializable]
    public struct FriendResponse
    {
        public string username;
        public string server_time;
        public FriendData[] friends;
        public FriendData[] friends_requests;
    }

    [System.Serializable]
    public class FriendData
    {
        public string username;
        public string avatar;
        public string last_login_time;
    }

}
