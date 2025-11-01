@echo off
chcp 65001 >nul
:: set-env-prod.bat - 一键设置 .NET 生产环境变量

echo.
echo 正在设置生产环境变量...

:: 1. 数据库连接字符串
set "CONN=Host=localhost;Database=postgres;Username=postgres;Password=123456"
setx ConnectionStrings__Default "%CONN%" >nul
set ConnectionStrings__Default=%CONN%

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

echo.
echo 生产环境变量设置完成！
echo.
echo   ConnectionStrings__Default = %CONN%
echo   Jwt__Key                   = your-super-secret-jwt-key-1234567890
echo   Jwt__Issuer                = GameLeaderboard
echo   Jwt__Audience              = GameLeaderboard
echo   ASPNETCORE_ENVIRONMENT     = Development
echo.
echo 提示：当前窗口立即生效，重启 VS Code / 终端 后全局生效。
echo.
pause