cd /d %~dp0
for /f "delims=" %%i in ('dir /b proto "proto/*.proto"') do protoc -I=proto/ --csharp_out=cs/ proto/%%i
pause
