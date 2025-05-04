# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the .csproj and restore dependencies (via dotnet restore)
COPY ["EchoCore.csproj", "."]
RUN dotnet restore "./EchoCore.csproj"

# Copy the rest of the code
COPY . .

# Publish the application to the /app/publish directory
RUN dotnet publish "./EchoCore.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published app from the build stage
COPY --from=build /app/publish .

# Set the environment variable for ASP.NET Core
ENV ASPNETCORE_URLS=http://+:80

# Expose port 80 for the container
EXPOSE 80

# Set the entry point for the container to run the application
ENTRYPOINT ["dotnet", "EchoCore.dll"]
