FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Services/Aggregation/Aggregation.csproj", "Services/Aggregation/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["FitTech.AppHost/FitTech.ServiceDefaults/FitTech.ServiceDefaults.csproj", "FitTech.AppHost/FitTech.ServiceDefaults/"]

RUN dotnet restore "Services/Aggregation/Aggregation.csproj" \
    --runtime linux-x64

COPY . .
RUN dotnet publish "Services/Aggregation/Aggregation.csproj" \
    -c Release -o /app/publish --no-restore \
    --runtime linux-x64 \
    --self-contained false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5103
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5103
ENTRYPOINT ["dotnet", "Aggregation.dll"]
