# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["ShahdCooperative.NotificationService.API/ShahdCooperative.NotificationService.API.csproj", "ShahdCooperative.NotificationService.API/"]
COPY ["ShahdCooperative.NotificationService.Application/ShahdCooperative.NotificationService.Application.csproj", "ShahdCooperative.NotificationService.Application/"]
COPY ["ShahdCooperative.NotificationService.Domain/ShahdCooperative.NotificationService.Domain.csproj", "ShahdCooperative.NotificationService.Domain/"]
COPY ["ShahdCooperative.NotificationService.Infrastructure/ShahdCooperative.NotificationService.Infrastructure.csproj", "ShahdCooperative.NotificationService.Infrastructure/"]

RUN dotnet restore "ShahdCooperative.NotificationService.API/ShahdCooperative.NotificationService.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/ShahdCooperative.NotificationService.API"
RUN dotnet build "ShahdCooperative.NotificationService.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ShahdCooperative.NotificationService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShahdCooperative.NotificationService.API.dll"]
