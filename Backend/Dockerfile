FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["WebAPI/ITTitans.Hackathon2025.WebAPI.csproj", "WebAPI/"]
COPY ["Service/ITTitans.Hackathon2025.Service.csproj", "Service/"]
COPY ["Model/ITTitans.Hackathon2025.Model.csproj", "Model/"]
COPY ["EntityModel/ITTitans.Hackathon2025.EntityModel.csproj", "EntityModel/"]

RUN dotnet restore "WebAPI/ITTitans.Hackathon2025.WebAPI.csproj"

# Copy the rest of the application code
COPY . .
WORKDIR "/src/WebAPI"

# Publish the application
RUN dotnet publish "ITTitans.Hackathon2025.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/publish .

ENV DOTNET_EnableDiagnostics=0
ENV ASPNETCORE_URLS=http://+:5000

EXPOSE 5000

USER app

# Set the entry point for the application
ENTRYPOINT ["dotnet", "ITTitans.Hackathon2025.WebAPI.dll"]