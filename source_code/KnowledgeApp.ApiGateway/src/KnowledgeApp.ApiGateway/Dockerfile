# Use the official .NET 8 SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy csproj file and restore dependencies
COPY ["source_code/KnowledgeApp.ApiGateway/src/KnowledgeApp.ApiGateway/*.csproj", "./"]

# Restore dependencies
RUN dotnet restore

# Copy the rest of the application code
COPY ["source_code/KnowledgeApp.ApiGateway/src/KnowledgeApp.ApiGateway", "."]

# Build the application in release mode
RUN dotnet publish -c Release -o /out

# Use the official .NET runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory inside the container
WORKDIR /app

# Copy the output from the build stage
COPY --from=build /out .

# Expose port 80 for the application
EXPOSE 80

# Start the application
ENTRYPOINT ["dotnet", "KnowledgeApp.ApiGateway.dll"]
