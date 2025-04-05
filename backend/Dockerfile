# Use the official .NET 6.0 SDK image
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy the project file(s) and restore dependencies
COPY *.csproj .
RUN dotnet restore

# Copy the rest of the application code
COPY . .

# Build the application
RUN dotnet publish -c Release -o /app/publish

# Use the official .NET 6.0 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Expose the port your API runs on
EXPOSE 5000

# Define build-time arguments for secrets
ARG AZURE_STORAGE_CONNECTION_STRING
ARG APPLICATION_INSIGHTS_CONNECTION_STRING

# Set environment variables for runtime
ENV AZURE_STORAGE_CONNECTION_STRING=$AZURE_STORAGE_CONNECTION_STRING
ENV APPLICATION_INSIGHTS_CONNECTION_STRING=$APPLICATION_INSIGHTS_CONNECTION_STRING

# Set the entry point for the container
ENTRYPOINT ["dotnet", "backend.dll"]