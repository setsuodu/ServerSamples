FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
# 每个服务都要加
ENV ASPNETCORE_URLS=http://+:80
# launchSettings.json 的设置在生产中无效
ENV ASPNETCORE_ENVIRONMENT=Docker

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ProductService.csproj", "."]
RUN dotnet restore "ProductService.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "ProductService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProductService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductService.dll"]