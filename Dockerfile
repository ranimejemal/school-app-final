# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SchoolApp.csproj .
RUN dotnet restore SchoolApp.csproj

COPY . .
RUN dotnet publish SchoolApp.csproj -c Release -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Render injects $PORT; fall back to 8080 for local `docker run`
CMD ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet SchoolApp.dll
