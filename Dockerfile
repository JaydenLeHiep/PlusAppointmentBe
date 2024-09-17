# Use the official .NET SDK image to build and publish the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the csproj file and restore any dependencies
COPY ["PlusAppointment.Api/PlusAppointment.Api.csproj", "PlusAppointment.Api/"]
COPY ["PlusAppointment.Models/PlusAppointment.Models.csproj", "PlusAppointment.Models/"]
RUN dotnet restore "PlusAppointment.Api/PlusAppointment.Api.csproj"

# Copy the entire project and build the app
COPY . .
WORKDIR "/src/PlusAppointment.Api"
RUN dotnet build "PlusAppointment.Api.csproj" -c Release -o /app/build

# Publish the app
RUN dotnet publish "PlusAppointment.Api.csproj" -c Release -o /app/publish

# Use the official .NET runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose port 80
EXPOSE 80

# Set the entry point to the built app
ENTRYPOINT ["dotnet", "PlusAppointment.Api.dll"]
