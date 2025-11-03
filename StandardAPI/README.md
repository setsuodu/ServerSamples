# Standard API Sample
https://grok.com/c/1784c7ec-5528-4172-b96a-f2277c710838

# Steps
1. 创建项目

cd ..\ServerSamples\StandardAPI
dotnet new sln -n StandardAPI
mkdir src && cd src

dotnet new webapi -n ApiGateway -o ApiGateway
dotnet new webapi -n WebApi -o WebApi

cd ..
dotnet sln add src/ApiGateway/ApiGateway.csproj
dotnet sln add src/WebApi/WebApi.csproj

2. 引用9.0.0似乎就会导致database update报错。 __EF...Histroy。用8.0.8目前正常。
```
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.8" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.8"/>
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.8"/>
```
单开容器
docker-compose up db -d
dotnet ef migrations add InitWebApi
dotnet ef database update

docker compose up --build