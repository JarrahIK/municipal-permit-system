# ---- build stage: restore packages, then publish a release build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the project file first so the package restore is cached between builds.
COPY src/MunicipalPermitSystem.Api/MunicipalPermitSystem.Api.csproj src/MunicipalPermitSystem.Api/
RUN dotnet restore src/MunicipalPermitSystem.Api/MunicipalPermitSystem.Api.csproj

COPY src/ src/
RUN dotnet publish src/MunicipalPermitSystem.Api/MunicipalPermitSystem.Api.csproj \
    -c Release -o /app/publish --no-restore

# ---- runtime stage: just the ASP.NET runtime + the published app, non-root ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "MunicipalPermitSystem.Api.dll"]
