# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["JobRankingSystem.csproj", "."]
RUN dotnet restore "./JobRankingSystem.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "JobRankingSystem.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "JobRankingSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "JobRankingSystem.dll"]
