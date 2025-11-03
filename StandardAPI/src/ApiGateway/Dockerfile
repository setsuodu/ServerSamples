# 1. 构建阶段
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 只复制 csproj
COPY *.csproj ./
RUN dotnet restore

# 复制当前项目源码
COPY . ./

# 构建
RUN dotnet publish -c Release -o /app/publish

# 2. 运行阶段
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ApiGateway.dll"]