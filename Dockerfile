FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["SportsCenter.API/SportsCenter.API.csproj", "SportsCenter.API/"]
COPY ["SportsCenter.Application/SportsCenter.Application.csproj", "SportsCenter.Application/"]
COPY ["SportsCenter.Infrastructure/SportsCenter.Infrastructure.csproj", "SportsCenter.Infrastructure/"]
COPY ["SportsCenter.Domain/SportsCenter.Domain.csproj", "SportsCenter.Domain/"]
RUN dotnet restore "SportsCenter.API/SportsCenter.API.csproj"

COPY . .
WORKDIR "/src/SportsCenter.API"
RUN dotnet build "SportsCenter.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SportsCenter.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "SportsCenter.API.dll"]