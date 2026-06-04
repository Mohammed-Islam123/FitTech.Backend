FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Services/Notification/Notification.Api/Notification.Api.csproj", "Services/Notification/Notification.Api/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["FitTech.AppHost/FitTech.ServiceDefaults/FitTech.ServiceDefaults.csproj", "FitTech.AppHost/FitTech.ServiceDefaults/"]

RUN dotnet restore "Services/Notification/Notification.Api/Notification.Api.csproj" \
    --runtime linux-x64

COPY . .
RUN dotnet publish "Services/Notification/Notification.Api/Notification.Api.csproj" \
    -c Release -o /app/publish --no-restore \
    --runtime linux-x64 \
    --self-contained false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 80
ENTRYPOINT ["dotnet", "Notification.Api.dll"]
