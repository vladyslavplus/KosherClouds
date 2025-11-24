FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./backend/shared/KosherClouds.ServiceDefaults/KosherClouds.ServiceDefaults.csproj ./shared/KosherClouds.ServiceDefaults/
COPY ./backend/src/Gateway/KosherClouds.ApiGateway/KosherClouds.ApiGateway.csproj ./src/Gateway/KosherClouds.ApiGateway/

RUN dotnet restore ./src/Gateway/KosherClouds.ApiGateway/KosherClouds.ApiGateway.csproj

COPY ./backend .

RUN dotnet publish ./src/Gateway/KosherClouds.ApiGateway/KosherClouds.ApiGateway.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
EXPOSE 8443

ENTRYPOINT ["dotnet", "KosherClouds.ApiGateway.dll"]