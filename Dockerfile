# Set up the build environment
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

ARG TARGETPLATFORM

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

COPY --from=build /src/PlusAppointment.Api/appsettings.json /app/

COPY --from=build /app/out .

# Expose port 80
EXPOSE 80

# Set the entry point to the built app
ENTRYPOINT ["dotnet", "PlusAppointment.Api.dll"]
