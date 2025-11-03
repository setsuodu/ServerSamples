@echo off
chcp 65001 >nul
:: clear-env.bat - 一键清除生产环境变量

echo.
echo 正在清除以下环境变量...

reg delete "HKCU\Environment" /v ConnectionStrings__Default /f >nul 2>&1
reg delete "HKCU\Environment" /v Jwt__Key /f >nul 2>&1
reg delete "HKCU\Environment" /v Jwt__Issuer /f >nul 2>&1
reg delete "HKCU\Environment" /v Jwt__Audience /f >nul 2>&1
reg delete "HKCU\Environment" /v ASPNETCORE_ENVIRONMENT /f >nul 2>&1
reg delete "HKCU\Environment" /v Redis__Connection /f >nul 2>&1
reg delete "HKCU\Environment" /v Redis__InstanceName /f >nul 2>&1

:: 清除当前窗口
set ConnectionStrings__Default=
set Jwt__Key=
set Jwt__Issuer=
set Jwt__Audience=
set ASPNETCORE_ENVIRONMENT=
set Redis__Connection=
set Redis__InstanceName=

echo.
echo 已清除 7 个生产环境变量！
echo.
echo 提示：当前窗口已清除，重启终端后完全移除。
echo.
pause