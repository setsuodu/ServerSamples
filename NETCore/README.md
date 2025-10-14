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

### Protoc Editor 配置
该工具提供了完整的PB库和文件生成工，再也不需要去零散下载各种包，配置环境变量等等工作。
Package Install 后，Project 中右键创建 Create / Protobuf / ProtobufConfig。
- [x] Protocol Compiler Enabled //修改工程中的.proto，自动重新生成对应.cs
- [x] C Sharp Compiler Enabled //✔勾选，表示要生成C#
- [x] C Sharp Custom Path //生成C#文件输出路径，可以填工程下的相对路径，也可以填绝对路径

### Protoc 传统设置（可选用，如服务器端）
比一定要将 Protoc 放到System32中，但是必须是固定的文件夹，如 C:/protoc/protoc.exe。
然后将所在目录添加环境变量 Path，如 C:/protoc
使用 protoc --version 测试是否添加成功。

.\Protoc\model\run.bat //设置好了相对路径，可以直接运行生成。

## .NET Core 安装
Install-Package Google.Protobuf
Install-Package Google.Protobuf.Tools
