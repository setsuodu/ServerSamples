## Get Started

1. 添加依赖。两端都安装 Protobuf 库。服务端 Nuget 安装。客户端导入。
2. 定义消息。创建.proto文件来定义消息结构。
3. 生成代码。使用protoc编译器为C#生成对应的类。
4. 两端处理序列化、反序列化。

## Protobuf
- 相比二进制，序列化和反序列化速度更快，包体更小，安全漏洞更少，跨语言解析。
- protobuf-net：2.0。兼容更多项目，如：ILRuntime。
- Google.Protobuf：3.0。更高版本，更多特性。兼容性低。

## Unity 安装
https://openupm.com/packages/com.gameworkstore.googleprotobufunity/#modal-manualinstallation
Project Settings / Package Manager / 
- Name | package.openupm.com
- URL | https://package.openupm.com
- Scope(s) | com.gameworkstore.googleprotobufunity

## .NET Core 安装
Install-Package Google.Protobuf
Install-Package Google.Protobuf.Tools

## Protoc 安装
比一定要将 Protoc 放到System32中，但是必须是固定的文件夹，如 C:/protoc/protoc.exe。
然后将所在目录添加环境变量 Path，如 C:/protoc
使用 protoc --version 测试是否添加成功。