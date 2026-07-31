FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY DiscordApp.WebApi/DiscordApp.WebApi.csproj DiscordApp.WebApi/
COPY Business/Business.csproj Business/
COPY Core/Core.csproj Core/
COPY Data/Data.csproj Data/
RUN dotnet nuget locals all --clear
RUN dotnet restore DiscordApp.WebApi/DiscordApp.WebApi.csproj --no-cache


COPY DiscordApp.WebApi/ DiscordApp.WebApi/
COPY Business/ Business/
COPY Core/ Core/
COPY Data/ Data/
RUN dotnet publish DiscordApp.WebApi/DiscordApp.WebApi.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

USER app

COPY --from=build /app/publish .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "DiscordApp.WebApi.dll"]