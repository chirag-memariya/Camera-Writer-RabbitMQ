# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Only copy the .csproj file (since context is root, and the .csproj is in EdgeGateWay/)
COPY EdgeGateWay/EdgeGateWay.csproj EdgeGateWay/
RUN dotnet restore "EdgeGateWay/EdgeGateWay.csproj"

# Copy everything else
COPY EdgeGateWay/ EdgeGateWay/
WORKDIR /src/EdgeGateWay
RUN dotnet publish "EdgeGateWay.csproj" -c Release -o /app/publish

# Stage 2: Create the final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EdgeGateWay.dll"]
