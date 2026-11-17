FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CuraLink.sln", "./"]
COPY ["src/CuraLink.Domain/CuraLink.Domain.csproj", "src/CuraLink.Domain/"]
COPY ["src/CuraLink.Application/CuraLink.Application.csproj", "src/CuraLink.Application/"]
COPY ["src/CuraLink.Infrastructure/CuraLink.Infrastructure.csproj", "src/CuraLink.Infrastructure/"]
COPY ["src/CuraLink.Web/CuraLink.Web.csproj", "src/CuraLink.Web/"]
COPY ["tests/CuraLink.UnitTests/CuraLink.UnitTests.csproj", "tests/CuraLink.UnitTests/"]
COPY ["tests/CuraLink.IntegrationTests/CuraLink.IntegrationTests.csproj", "tests/CuraLink.IntegrationTests/"]
RUN dotnet restore "CuraLink.sln"
COPY . .
RUN dotnet publish "src/CuraLink.Web/CuraLink.Web.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CuraLink.Web.dll"]
