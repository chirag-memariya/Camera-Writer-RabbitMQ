# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Only copy the .csproj file (since context is root, and the .csproj is in Storage1Service/)
COPY Storage1Service/Storage1Service.csproj Storage1Service/
RUN dotnet restore "Storage1Service/Storage1Service.csproj"

# Copy everything else
COPY Storage1Service/ Storage1Service/
WORKDIR /src/Storage1Service
RUN dotnet publish "Storage1Service.csproj" -c Release -o /app/publish

# Stage 2: Create the final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Storage1Service.dll"]
