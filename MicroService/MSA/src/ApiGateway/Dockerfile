FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 现在 context 是 ./src/ApiGateway，所以项目文件就在根目录
COPY *.csproj ./
RUN dotnet restore

# 复制源码
WORKDIR /src/ApiGateway
COPY . .

#RUN echo "这里报错99%是路径错误：COPY . . 前没有指定 WORKDIR"
RUN dotnet build "ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# 配置文件在当前 context 根目录，直接复制
COPY ocelot.Production.json .

ENTRYPOINT ["dotnet", "ApiGateway.dll"]