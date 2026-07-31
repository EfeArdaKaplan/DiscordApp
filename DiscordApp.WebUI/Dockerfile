FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY DiscordApp.WebUI/DiscordApp.WebUI.csproj DiscordApp.WebUI/
RUN dotnet nuget locals all --clear
RUN dotnet restore DiscordApp.WebUI/DiscordApp.WebUI.csproj --no-cache

COPY DiscordApp.WebUI/ DiscordApp.WebUI/
RUN dotnet publish DiscordApp.WebUI/DiscordApp.WebUI.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

USER app

COPY --from=build /app/publish .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "DiscordApp.WebUI.dll"]
