FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5002
EXPOSE 5002

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Infrabot.BotManagement.ApiGateway/Infrabot.BotManagement.ApiGateway.csproj", "Infrabot.BotManagement.ApiGateway/"]
COPY ["Infrabot.BotManagement.Broker/Infrabot.BotManagement.Broker.csproj", "Infrabot.BotManagement.Broker/"]
COPY ["Infrabot.BotManagement.Domain/Infrabot.BotManagement.Domain.csproj", "Infrabot.BotManagement.Domain/"]
RUN dotnet restore "Infrabot.BotManagement.ApiGateway/Infrabot.BotManagement.ApiGateway.csproj"
COPY . .
WORKDIR "/src/Infrabot.BotManagement.ApiGateway"
RUN dotnet build "Infrabot.BotManagement.ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Infrabot.BotManagement.ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Infrabot.BotManagement.ApiGateway.dll"]
