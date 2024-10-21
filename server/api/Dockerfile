# Use the official ASP.NET Core base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install the Cloud SQL Proxy
ADD https://dl.google.com/cloudsql/cloud_sql_proxy.linux.amd64 /cloud_sql_proxy
RUN chmod +x /cloud_sql_proxy

# Switch to a non-root user after setting up Cloud SQL Proxy
USER $APP_UID

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["server/api/api.csproj", "server/api/"]
COPY ["server/service/service.csproj", "server/service/"]
COPY ["server/dataAccess/dataAccess.csproj", "server/dataAccess/"]
RUN dotnet restore "server/api/api.csproj"
COPY . .
WORKDIR "/src/server/api"
RUN dotnet build "api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the app
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage: create a container with the app and Cloud SQL Proxy
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=base /cloud_sql_proxy /cloud_sql_proxy

# Switch to root to run the Cloud SQL Proxy, but keep the app running as non-root
USER root
ENTRYPOINT ["/bin/sh", "-c", "/cloud_sql_proxy -dir=/cloudsql paper-lobster:europe-west1:paper-shop-db & dotnet api.dll"]