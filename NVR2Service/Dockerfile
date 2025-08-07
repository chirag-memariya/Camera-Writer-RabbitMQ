# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Only copy the .csproj file (since context is root, and the .csproj is in NVR2Service/)
COPY NVR2Service/NVR2Service.csproj NVR2Service/
RUN dotnet restore "NVR2Service/NVR2Service.csproj"

# Copy everything else
COPY NVR2Service/ NVR2Service/
WORKDIR /src/NVR2Service
RUN dotnet publish "NVR2Service.csproj" -c Release -o /app/publish

# Stage 2: Create the final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NVR2Service.dll"]
