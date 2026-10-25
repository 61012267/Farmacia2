# Cambia SOLO este número para que coincida con el TargetFramework de tu .csproj
# net8.0 -> 8.0   |   net10.0 -> 10.0
ARG DOTNET_VERSION=10.0

# ---------- Etapa de compilación ----------
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src
COPY *.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o /app/publish 

# ---------- Etapa de ejecución ----------
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV PORT=10000
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000
ENTRYPOINT ["dotnet", "Farmacia.dll"]
