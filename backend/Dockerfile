FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
WORKDIR /src
COPY backend.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:11.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "backend.dll"]