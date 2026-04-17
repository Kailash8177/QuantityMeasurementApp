FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all the layers required by the API and restore
COPY ["QuantityMeasurementApi/QuantityMeasurementApi.csproj", "QuantityMeasurementApi/"]
COPY ["QuantityMeasurementModelLayer/QuantityMeasurementModelLayer.csproj", "QuantityMeasurementModelLayer/"]
COPY ["QuantityMeasurementbusinessLayer/QuantityMeasurementbusinessLayer.csproj", "QuantityMeasurementbusinessLayer/"]
COPY ["QuantityMeasurementRepositoryLayer/QuantityMeasurementRepositoryLayer.csproj", "QuantityMeasurementRepositoryLayer/"]

RUN dotnet restore "QuantityMeasurementApi/QuantityMeasurementApi.csproj"

# Copy everything else and build the app
COPY . .
WORKDIR "/src/QuantityMeasurementApi"
RUN dotnet build "QuantityMeasurementApi.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "QuantityMeasurementApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose port 8080 (Render's default expected port)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "QuantityMeasurementApi.dll"]
