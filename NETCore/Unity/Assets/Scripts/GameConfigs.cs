public static class GameConfigs
{
    // 网络相关
    public const string SERVER_IP = "127.0.0.1";
    public const int SERVER_PORT = 8080;

    // 游戏设置相关常量
    public const float PLAYER_MAX_SPEED = 5.0f;
    public const int MAX_PLAYER_HEALTH = 100;
    public const float GRAVITY_SCALE = 9.81f;

    // 场景相关常量
    public const string MAIN_MENU_SCENE = "MainMenu";
    public const string GAMEPLAY_SCENE = "Gameplay";

    // 层级和标签
    public const string PLAYER_TAG = "Player";
    public const int GROUND_LAYER = 8;

    // 物理相关
    public const float JUMP_FORCE = 10.0f;
    public const float BULLET_SPEED = 20.0f;

    // UI相关
    public const float FADE_DURATION = 1.0f;
    public const int SCORE_MULTIPLIER = 10;
}