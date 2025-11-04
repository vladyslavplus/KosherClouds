FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./backend/shared/KosherClouds.Contracts/KosherClouds.Contracts.csproj ./shared/KosherClouds.Contracts/
COPY ./backend/shared/KosherClouds.Common/KosherClouds.Common.csproj ./shared/KosherClouds.Common/
COPY ./backend/shared/KosherClouds.ServiceDefaults/KosherClouds.ServiceDefaults.csproj ./shared/KosherClouds.ServiceDefaults/
COPY ./backend/src/UserService/KosherClouds.UserService/KosherClouds.UserService.csproj ./src/UserService/KosherClouds.UserService/

RUN dotnet restore ./src/UserService/KosherClouds.UserService/KosherClouds.UserService.csproj

COPY ./backend .

RUN dotnet publish ./src/UserService/KosherClouds.UserService/KosherClouds.UserService.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
EXPOSE 8443

ENTRYPOINT ["dotnet", "KosherClouds.UserService.dll"]