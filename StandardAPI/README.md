# Standard API Sample
https://grok.com/c/1784c7ec-5528-4172-b96a-f2277c710838

# Steps
1. 创建项目

cd ..\ServerSamples\StandardAPI
dotnet new sln -n StandardAPI
mkdir src && cd src

dotnet new webapi -n ApiGateway -o ApiGateway
dotnet new webapi -n BugService -o BugService

cd ..
dotnet sln add src/ApiGateway/ApiGateway.csproj
dotnet sln add src/BugService/BugService.csproj