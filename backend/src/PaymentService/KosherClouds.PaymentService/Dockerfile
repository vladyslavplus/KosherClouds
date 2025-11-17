FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./backend/shared/KosherClouds.Contracts/KosherClouds.Contracts.csproj ./shared/KosherClouds.Contracts/
COPY ./backend/shared/KosherClouds.Common/KosherClouds.Common.csproj ./shared/KosherClouds.Common/
COPY ./backend/shared/KosherClouds.ServiceDefaults/KosherClouds.ServiceDefaults.csproj ./shared/KosherClouds.ServiceDefaults/

COPY ./backend/src/PaymentService/KosherClouds.PaymentService/KosherClouds.PaymentService.csproj ./src/PaymentService/KosherClouds.PaymentService/

RUN dotnet restore ./src/PaymentService/KosherClouds.PaymentService/KosherClouds.PaymentService.csproj

COPY ./backend .

RUN dotnet publish ./src/PaymentService/KosherClouds.PaymentService/KosherClouds.PaymentService.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
EXPOSE 8443

ENTRYPOINT ["dotnet", "KosherClouds.PaymentService.dll"]