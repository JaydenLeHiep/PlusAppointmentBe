# Use the official .NET SDK image to build and publish the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Set environment variable to help with architecture-specific issues
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Copy the project files and restore dependencies
COPY ["PlusAppointment.Api/PlusAppointment.Api.csproj", "PlusAppointment.Api/"]
COPY ["PlusAppointment.Models/PlusAppointment.Models.csproj", "PlusAppointment.Models/"]
RUN dotnet restore "PlusAppointment.Api/PlusAppointment.Api.csproj"

# Copy the entire project, including the appsettings.json file, and build the app
COPY . .
WORKDIR "/src/PlusAppointment.Api"
RUN dotnet publish -c Release -o /app/out

# Use the official .NET runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the appsettings.json file into the container
COPY --from=build /src/PlusAppointment.Api/appsettings.json /app/

# Copy the published application files
COPY --from=build /app/out .

# Expose port 8080 (or the port your application is configured to use)
EXPOSE 8080

# Set the entry point to the built app
ENTRYPOINT ["dotnet", "PlusAppointment.Api.dll"]
