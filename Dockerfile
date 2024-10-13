# 基礎映像
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 1950

ENV ASPNETCORE_URLS=http://+:1950

# 構建映像
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["StartFMS.DiscordBot.csproj", "./"]
RUN dotnet restore "StartFMS.DiscordBot.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "StartFMS.DiscordBot.csproj" -c $configuration -o /app/build

# 發佈映像
FROM build AS publish
ARG configuration=Release
RUN dotnet publish "StartFMS.DiscordBot.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

# 最終映像
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StartFMS.DiscordBot.dll"]