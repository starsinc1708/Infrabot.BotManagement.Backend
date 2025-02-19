FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Infrabot.BotManagement.WebAPI/Infrabot.BotManagement.WebAPI.csproj", "Infrabot.BotManagement.WebAPI/"]
COPY ["Infrabot.BotManagement.Domain/Infrabot.BotManagement.Domain.csproj", "Infrabot.BotManagement.Domain/"]
RUN dotnet restore "Infrabot.BotManagement.WebAPI/Infrabot.BotManagement.WebAPI.csproj"
COPY . .
WORKDIR "/src/Infrabot.BotManagement.WebAPI"
RUN dotnet build "Infrabot.BotManagement.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Infrabot.BotManagement.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Infrabot.BotManagement.WebAPI.dll"]
