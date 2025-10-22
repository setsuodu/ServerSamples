// 旧版
//public class LoginResponse
//{
//    public string token;
//}
[System.Serializable]
public class LoginResponse
{
    public string AccessToken; // JWT 短令牌
    public string RefreshToken; // JWT 长令牌
}