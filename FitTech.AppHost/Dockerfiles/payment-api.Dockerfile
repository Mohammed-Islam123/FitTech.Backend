FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Services/Payment/Payment.csproj", "Services/Payment/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["FitTech.AppHost/FitTech.ServiceDefaults/FitTech.ServiceDefaults.csproj", "FitTech.AppHost/FitTech.ServiceDefaults/"]

RUN dotnet restore "Services/Payment/Payment.csproj" \
    --runtime linux-x64

COPY . .
RUN dotnet publish "Services/Payment/Payment.csproj" \
    -c Release -o /app/publish --no-restore \
    --runtime linux-x64 \
    --self-contained false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5246
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5246
ENTRYPOINT ["dotnet", "Payment.dll"]
