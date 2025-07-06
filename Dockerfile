# Set up the build environment
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project files and restore dependencies
COPY ["PlusAppointment.Api/PlusAppointment.Api.csproj", "PlusAppointment.Api/"]
COPY ["PlusAppointment.Models/PlusAppointment.Models.csproj", "PlusAppointment.Models/"]
RUN dotnet restore "PlusAppointment.Api/PlusAppointment.Api.csproj"

# Copy the entire project and build the app
COPY . .
WORKDIR "/src/PlusAppointment.Api"
RUN dotnet publish -c Release -o /app/out

# Set up the runtime environment
FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the appsettings.json file explicitly from the source directory to the runtime environment
COPY --from=build /src/PlusAppointment.Api/appsettings.json /app/appsettings.json

# Copy the published application files
COPY --from=build /app/out .

RUN mkdir -p /app/logs
# Expose port 80
EXPOSE 80

# Set the entry point to the built app
ENTRYPOINT ["dotnet", "PlusAppointment.Api.dll"]
