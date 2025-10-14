cd /d %~dp0
if exist cs (
	echo cs has existed
) else (
	md cs 
	echo create dir cs
)

for /f "delims=" %%i in ('dir /b proto "proto/*.proto"') do protoc -I=proto/ --csharp_out=cs/ proto/%%i
pause
