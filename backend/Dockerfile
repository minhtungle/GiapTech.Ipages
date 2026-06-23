FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files và restore
COPY src/GiapTech.Ipages.Domain/GiapTech.Ipages.Domain.csproj src/GiapTech.Ipages.Domain/
COPY src/GiapTech.Ipages.Application/GiapTech.Ipages.Application.csproj src/GiapTech.Ipages.Application/
COPY src/GiapTech.Ipages.Infrastructure/GiapTech.Ipages.Infrastructure.csproj src/GiapTech.Ipages.Infrastructure/
COPY src/GiapTech.Ipages.Api/GiapTech.Ipages.Api.csproj src/GiapTech.Ipages.Api/
RUN dotnet restore src/GiapTech.Ipages.Api/GiapTech.Ipages.Api.csproj

# Copy toàn bộ source và build
COPY . .
RUN dotnet publish src/GiapTech.Ipages.Api/GiapTech.Ipages.Api.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5000
ENTRYPOINT ["dotnet", "GiapTech.Ipages.Api.dll"]
