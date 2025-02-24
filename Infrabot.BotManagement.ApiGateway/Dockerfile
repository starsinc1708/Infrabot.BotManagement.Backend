FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5002

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Infrabot.BotManagement.ApiGateway/Infrabot.BotManagement.ApiGateway.csproj", "Infrabot.BotManagement.ApiGateway/"]
RUN dotnet restore "Infrabot.BotManagement.ApiGateway/Infrabot.BotManagement.ApiGateway.csproj"
COPY . .
WORKDIR "/src/Infrabot.BotManagement.ApiGateway"
RUN dotnet build "Infrabot.BotManagement.ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Infrabot.BotManagement.ApiGateway.csproj" -c Release -o /app/publish

# Стадия для исполнения
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Infrabot.BotManagement.ApiGateway.dll"]
