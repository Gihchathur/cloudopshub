# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["CloudOpsHub.NotificationService.csproj", "./"]

RUN dotnet restore "CloudOpsHub.NotificationService.csproj"

COPY . .

RUN dotnet publish "CloudOpsHub.NotificationService.csproj" \
    -c Release \
    -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CloudOpsHub.NotificationService.dll"]