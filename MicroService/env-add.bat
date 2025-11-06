@echo off
chcp 65001 >nul
:: set-env-prod.bat - 一键设置 .NET 生产环境变量

echo.
echo 正在设置生产环境变量...

:: 1. 数据库连接字符串
set "CONN__UserDb=Host=localhost;Port=5433;Database=user_db;Username=postgres;Password=123456"
setx ConnectionStrings__UserDb "%CONN__UserDb%" >nul
set ConnectionStrings__UserDb=%CONN__UserDb%

set "CONN__GameDb=Host=localhost;Port=5434;Database=game_db;Username=postgres;Password=123456"
setx ConnectionStrings__GameDb "%CONN__GameDb%" >nul
set ConnectionStrings__GameDb=%CONN__GameDb%

:: 2. JWT 配置
setx Jwt__Key "your-super-secret-jwt-key-1234567890" >nul
set Jwt__Key=your-super-secret-jwt-key-1234567890

setx Jwt__Issuer "GameLeaderboard" >nul
set Jwt__Issuer=GameLeaderboard

setx Jwt__Audience "GameLeaderboard" >nul
set Jwt__Audience=GameLeaderboard

:: 3. 运行环境
setx ASPNETCORE_ENVIRONMENT "Development" >nul
set ASPNETCORE_ENVIRONMENT=Development

:: 4. 排行榜使用Redis（msa-redis是容器用的，不会读Windows环境变量。这咯用localhost，给VS调试用）
setx Redis__Connection "localhost:6379" >nul
set Redis__Connection=localhost:6379

setx Redis__InstanceName "LeaderboardCache" >nul
set Redis__InstanceName=LeaderboardCache

echo.
echo 生产环境变量设置完成！
echo.
echo   ConnectionStrings__UserDb = %CONN__UserDb%
echo   ConnectionStrings__GameDb = %CONN__GameDb%
echo   Jwt__Key                   = your-super-secret-jwt-key-1234567890
echo   Jwt__Issuer                = GameLeaderboard
echo   Jwt__Audience              = GameLeaderboard
echo   ASPNETCORE_ENVIRONMENT     = Development
echo   Redis__Connection          = msa-redis:6379
echo   Redis__InstanceName        = LeaderboardCache
echo.
echo 提示：当前窗口立即生效，重启 VS Code / 终端 后全局生效。
echo.
pause