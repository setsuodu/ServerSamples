batchmode：正常编译，和客户的一样
headless mode：无渲染，但还是有冗余

最简单的网络连接示例：
TcpClient,TcpListener,UdpClient：是对Socket的封装，用于“同步阻塞”模式下的网络连接。提供API，使用更简单。
TcpClient同时提供了Http相关API。

shortcut（快捷方式）给C# 传值
./ServerBuild.x86_64 --port 6666
创建桌面快捷方式，属性/快捷方式/C:\Users\Administrator\Desktop\ServerSamples\MatchUp\Build\MatchUp.exe后面加 --port 6666

//CMD
C:\Users\Administrator\Desktop\ServerSamples\TcpClient\Build\Server\TcpClient.exe --port 8001
