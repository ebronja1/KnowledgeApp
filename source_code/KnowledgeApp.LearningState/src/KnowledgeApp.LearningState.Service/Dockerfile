# Use the official .NET 8 SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy csproj file and restore dependencies
COPY ["source_code/KnowledgeApp.LearningState/src/KnowledgeApp.LearningState.Service/*.csproj", "./"]

# Copy NuGet.config from the root directory of the context
COPY ["NuGet.config", "."]

# Copy the NuGet packages from the root packages folder
COPY ["packages", "packages/"]

# Restore dependencies
RUN dotnet restore

# Copy the rest of the application code
COPY ["source_code/KnowledgeApp.LearningState/src/KnowledgeApp.LearningState.Service", "."]

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
ENTRYPOINT ["dotnet", "KnowledgeApp.LearningState.Service.dll"]

