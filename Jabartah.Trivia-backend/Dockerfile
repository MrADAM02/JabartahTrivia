# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files first for layer caching, restore, then copy everything else.
COPY Jabartah.Trivia.sln ./
COPY src/Jabartah.Trivia.Domain/Jabartah.Trivia.Domain.csproj src/Jabartah.Trivia.Domain/
COPY src/Jabartah.Trivia.Application/Jabartah.Trivia.Application.csproj src/Jabartah.Trivia.Application/
COPY src/Jabartah.Trivia.Infrastructure/Jabartah.Trivia.Infrastructure.csproj src/Jabartah.Trivia.Infrastructure/
COPY src/Jabartah.Trivia.Api/Jabartah.Trivia.Api.csproj src/Jabartah.Trivia.Api/
RUN dotnet restore src/Jabartah.Trivia.Api/Jabartah.Trivia.Api.csproj

COPY src/ src/
RUN dotnet publish src/Jabartah.Trivia.Api/Jabartah.Trivia.Api.csproj \
    -c Release -o /app/publish --no-restore

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# curl is needed for the HEALTHCHECK below -- not present in the slim base image.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# /api/categories is a real, unauthenticated, DB-touching endpoint, so "healthy"
# actually means the app is up AND migrations+seed succeeded, not just a liveness ping.
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD curl -f http://localhost:8080/api/categories || exit 1

ENTRYPOINT ["dotnet", "Jabartah.Trivia.Api.dll"]
