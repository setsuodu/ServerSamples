@echo off
chcp 65001 >nul
:: add-connection.bat - 添加 .NET 连接字符串环境变量
:: 支持 ConnectionStrings__Default 格式（.NET 配置系统）

set "CONN=Host=localhost;Database=db_msa;Username=msa;Password=123456"

:: 设置当前用户环境变量（永久生效）
setx ConnectionStrings__Default "%CONN%" >nul

:: 同时设置当前窗口环境变量（立即生效）
set ConnectionStrings__Default=%CONN%

echo.
echo 已成功添加环境变量：
echo   ConnectionStrings__Default = %CONN%
echo.
echo 提示：已对当前窗口立即生效，重启终端或 VS Code 后全局生效。
echo.
pause