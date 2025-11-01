@echo off
chcp 65001 >nul
:: remove-connection.bat - 清除 ConnectionStrings__Default 环境变量

:: 删除用户环境变量
reg delete "HKCU\Environment" /v ConnectionStrings__Default /f >nul 2>&1

:: 清除当前窗口变量
set ConnectionStrings__Default=

echo.
echo 已成功清除环境变量：ConnectionStrings__Default
echo.
echo 提示：当前窗口已清除，重启终端后完全移除。
echo.
pause