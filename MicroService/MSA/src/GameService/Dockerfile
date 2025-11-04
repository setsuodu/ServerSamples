FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 现在 context 是 ./src/GameService，所以项目文件就在根目录
COPY *.csproj ./
RUN dotnet restore

# 复制源码
WORKDIR /src/GameService
COPY . .

RUN dotnet build "GameService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GameService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "GameService.dll"]